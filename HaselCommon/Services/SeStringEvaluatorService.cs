using System.Runtime.CompilerServices;
using System.Text;
using Dalamud.Game;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Component.Text;
using HaselCommon.Extensions;
using HaselCommon.Services.SeStringEvaluation;
using Lumina.Excel.GeneratedSheets;
using Lumina.Text;
using Lumina.Text.Expressions;
using Lumina.Text.Payloads;
using Lumina.Text.ReadOnly;
using Microsoft.Extensions.Logging;

namespace HaselCommon.Services;

/// <summary>Evaluator for SeStrings.</summary>
public partial class SeStringEvaluatorService(
    ILogger<SeStringEvaluatorService> Logger,
    IDataManager dataManager,
    ExcelService excelService,
    TextService textService,
    TextureService textureService,
    TextDecoder textDecoder)
{
    private readonly IDataManager DataManager = dataManager;
    private readonly ExcelService ExcelService = excelService;
    private readonly TextService TextService = textService;
    private readonly TextureService TextureService = textureService;
    private readonly TextDecoder TextDecoder = textDecoder;

    public ReadOnlySeString Evaluate(byte[] str)
        => Evaluate(str, new());

    public ReadOnlySeString Evaluate(ReadOnlySeString str)
        => Evaluate(str, new());

    public ReadOnlySeString Evaluate(ReadOnlySeStringSpan str)
        => Evaluate(str, new());

    public ReadOnlySeString Evaluate(byte[] str, SeStringContext context)
        => Evaluate(str.AsSpan(), context);

    public ReadOnlySeString Evaluate(ReadOnlySeString str, SeStringContext context)
        => Evaluate(str.AsSpan(), context);

    public ReadOnlySeString Evaluate(ReadOnlySeStringSpan str, SeStringContext context)
    {
        context.Language ??= TextService.ClientLanguage.Value;

        foreach (var payload in str)
            ResolveStringPayload(ref context, payload);

        return context.Builder.ToReadOnlySeString();
    }

    public ReadOnlySeString EvaluateFromAddon(uint addonId, SeStringContext context)
    {
        context.Language ??= TextService.ClientLanguage.Value;

        var addonRow = ExcelService.GetRow<Addon>(addonId, context.Language);
        if (addonRow == null)
            return new();

        return Evaluate(addonRow.Text.AsReadOnly(), context);
    }

    public unsafe bool TryGetGNumDefault(uint parameterIndex, out uint value)
    {
        value = 0u;

        var rtm = RaptureTextModule.Instance();
        if (rtm is null)
            return false;

        if (!ThreadSafety.IsMainThread)
        {
            Logger.LogError("Global parameters may only be used from the main thread.");
            return false;
        }

        ref var gp = ref rtm->TextModule.MacroDecoder.GlobalParameters;
        if (parameterIndex >= gp.MySize)
            return false;

        var p = rtm->TextModule.MacroDecoder.GlobalParameters[parameterIndex];
        switch (p.Type)
        {
            case TextParameterType.Integer:
                value = (uint)p.IntValue;
                return true;

            case TextParameterType.ReferencedUtf8String:
                Logger.LogError("Requested a number; Utf8String global parameter at {parameterIndex}.", parameterIndex);
                return false;

            case TextParameterType.String:
                Logger.LogError("Requested a number; string global parameter at {parameterIndex}.", parameterIndex);
                return false;

            case TextParameterType.Uninitialized:
                Logger.LogError("Requested a number; uninitialized global parameter at {parameterIndex}.", parameterIndex);
                return false;

            default:
                return false;
        }
    }

    /// <inheritdoc/>
    public unsafe bool TryProduceGStrDefault(ref SeStringContext ctx, uint parameterIndex)
    {
        var rtm = RaptureTextModule.Instance();
        if (rtm is null)
            return false;

        ref var gp = ref rtm->TextModule.MacroDecoder.GlobalParameters;
        if (parameterIndex >= gp.MySize)
            return false;

        if (!ThreadSafety.IsMainThread)
        {
            Logger.LogError("Global parameters may only be used from the main thread.");
            return false;
        }

        var p = rtm->TextModule.MacroDecoder.GlobalParameters[parameterIndex];
        switch (p.Type)
        {
            case TextParameterType.Integer:
                ctx.Builder.Append(p.IntValue.ToString());
                return true;

            case TextParameterType.ReferencedUtf8String:
                var str = new ReadOnlySeStringSpan(p.ReferencedUtf8StringValue->Utf8String.AsSpan());
                foreach (var payload in str)
                    return ResolveStringPayload(ref ctx, payload);
                return false;

            case TextParameterType.String:
                str = new ReadOnlySeStringSpan(p.StringValue);
                foreach (var payload in str)
                    return ResolveStringPayload(ref ctx, payload);
                return false;
            case TextParameterType.Uninitialized:
            default:
                return false;
        }
    }

    public unsafe bool ResolveStringPayload(ref SeStringContext context, ReadOnlySePayloadSpan payload)
    {
        if (payload.Type == ReadOnlySePayloadType.Invalid)
            return false;

        //if (context.HandlePayload(payload, ref context))
        //    return true;

        if (payload.Type == ReadOnlySePayloadType.Text)
        {
            context.Builder.Append(payload.Body);
            return false;
        }

        // Note: "x" means that nothing must come after. We ignore any extra expressions.
        switch (payload.MacroCode)
        {
            case MacroCode.SetResetTime:
                {
                    DateTime date;
                    if (payload.TryGetExpression(out var eHour, out var eWeekday)
                        && TryResolveInt(ref context, eHour, out var eHourVal)
                        && TryResolveInt(ref context, eWeekday, out var eWeekdayVal))
                    {
                        var t = DateTime.UtcNow.AddDays((eWeekdayVal - (int)DateTime.UtcNow.DayOfWeek + 7) % 7);
                        date = new DateTime(t.Year, t.Month, t.Day, eHourVal, 0, 0, DateTimeKind.Utc).ToLocalTime();
                    }
                    else if (payload.TryGetExpression(out eHour)
                             && TryResolveInt(ref context, eHour, out eHourVal))
                    {
                        var t = DateTime.UtcNow;
                        date = new DateTime(t.Year, t.Month, t.Day, eHourVal, 0, 0, DateTimeKind.Utc).ToLocalTime();
                    }
                    else
                    {
                        return false;
                    }

                    MacroDecoder.GetMacroTime()->SetTime(date);
                    return true;
                }

            case MacroCode.SetTime:
                {
                    if (payload.TryGetExpression(out var eTime)
                        && TryResolveUInt(ref context, eTime, out var eTimeVal))
                    {
                        var date = DateTimeOffset.FromUnixTimeSeconds(eTimeVal).LocalDateTime;
                        MacroDecoder.GetMacroTime()->SetTime(date);
                        return true;
                    }

                    return false;
                }

            case MacroCode.If:
                {
                    return
                        payload.TryGetExpression(out var eCond, out var eTrue, out var eFalse)
                        && ResolveStringExpression(
                            ref context,
                            TryResolveBool(ref context, eCond, out var eCondVal) && eCondVal
                                ? eTrue
                                : eFalse);
                }

            case MacroCode.Switch:
                {
                    var cond = -1;
                    foreach (var e in payload)
                    {
                        switch (cond)
                        {
                            case -1:
                                cond = TryResolveUInt(ref context, e, out var eVal) ? (int)eVal : 0;
                                break;
                            case > 1:
                                cond--;
                                break;
                            default:
                                return ResolveStringExpression(ref context, e);
                        }
                    }

                    return false;
                }

            case MacroCode.NewLine:
                context.Builder.BeginMacro(MacroCode.NewLine).EndMacro();
                return true;

            case MacroCode.Icon:
            case MacroCode.Icon2: // ?
                {
                    if (!payload.TryGetExpression(out var eIcon)
                        || !TryResolveUInt(ref context, eIcon, out var eIconValue))
                    {
                        return false;
                    }

                    context.Builder.AppendIcon(eIconValue);
                    return true;
                }

            case MacroCode.Color:
                {
                    if (!payload.TryGetExpression(out var eColor))
                        return false;
                    if (eColor.TryGetPlaceholderExpression(out var ph) && ph == (int)ExpressionType.StackColor)
                        context.Builder.PopColor();
                    else if (TryResolveUInt(ref context, eColor, out var eColorVal))
                        context.Builder.PushColorBgra(eColorVal);
                    return true;
                }

            case MacroCode.EdgeColor:
                {
                    if (!payload.TryGetExpression(out var eColor))
                        return false;
                    if (eColor.TryGetPlaceholderExpression(out var ph) && ph == (int)ExpressionType.StackColor)
                        context.Builder.PopEdgeColor();
                    else if (TryResolveUInt(ref context, eColor, out var eColorVal))
                        context.Builder.PushEdgeColorBgra(eColorVal);
                    return true;
                }

            case MacroCode.SoftHyphen:
                if (!context.StripSoftHypen)
                    context.Builder.Append("\u00AD"u8);
                return true;

            case MacroCode.Bold:
                {
                    if (payload.TryGetExpression(out var eEnable)
                        && TryResolveBool(ref context, eEnable, out var eEnableVal))
                    {
                        context.Builder.AppendSetBold(eEnableVal);
                        return true;
                    }

                    return false;
                }

            case MacroCode.Italic:
                {
                    if (payload.TryGetExpression(out var eEnable)
                        && TryResolveBool(ref context, eEnable, out var eEnableVal))
                    {
                        context.Builder.AppendSetItalic(eEnableVal);
                        return true;
                    }

                    return false;
                }

            case MacroCode.NonBreakingSpace:
                context.Builder.Append("\u00A0"u8);
                return true;

            case MacroCode.Hyphen:
                context.Builder.Append('-');
                return true;

            case MacroCode.Num:
                {
                    if (payload.TryGetExpression(out var eInt) && TryResolveInt(ref context, eInt, out var eIntVal))
                    {
                        context.Builder.Append(eIntVal.ToString());
                        return true;
                    }

                    context.Builder.Append('0');
                    return true;
                }

            case MacroCode.Hex:
                {
                    if (payload.TryGetExpression(out var eUInt) &&
                        TryResolveUInt(ref context, eUInt, out var eUIntVal))
                    {
                        context.Builder.Append("0x{0:X08}".Format(eUIntVal));
                        return true;
                    }

                    context.Builder.Append("0x00000000"u8);
                    return true;
                }

            case MacroCode.Kilo:
                {
                    if (payload.TryGetExpression(out var eInt, out var eSep)
                        && TryResolveInt(ref context, eInt, out var eIntVal))
                    {
                        if (eIntVal == int.MinValue)
                        {
                            context.Builder.Append("-2"u8);
                            // 2147483648
                            ResolveStringExpression(ref context, eSep);
                            context.Builder.Append("147"u8);
                            ResolveStringExpression(ref context, eSep);
                            context.Builder.Append("483"u8);
                            ResolveStringExpression(ref context, eSep);
                            context.Builder.Append("648"u8);
                            return true;
                        }

                        if (eIntVal < 0)
                        {
                            context.Builder.Append('-');
                            eIntVal = -eIntVal;
                        }

                        if (eIntVal == 0)
                        {
                            context.Builder.Append('0');
                            return true;
                        }

                        var anyDigitPrinted = false;
                        for (var i = 1_000_000_000; i > 0; i /= 10)
                        {
                            var digit = eIntVal / i % 10;
                            switch (anyDigitPrinted)
                            {
                                case false when digit == 0:
                                    continue;
                                case true when i % 3 == 0:
                                    ResolveStringExpression(ref context, eSep);
                                    break;
                            }

                            anyDigitPrinted = true;
                            context.Builder.Append((char)('0' + digit));
                        }

                        return true;
                    }

                    context.Builder.Append('0');
                    return true;
                }

            case MacroCode.Digit:
                {
                    if (!payload.TryGetExpression(out var eValue, out var eTargetLength))
                        return false;

                    if (!TryResolveInt(ref context, eValue, out var eValueVal))
                        return false;

                    if (!TryResolveInt(ref context, eTargetLength, out var eTargetLengthVal))
                        return false;

                    context.Builder.Append(eValueVal.ToString($"{new string('0', eTargetLengthVal)}"));
                    return true;
                }

            case MacroCode.Sec:
                {
                    if (payload.TryGetExpression(out var eInt) && TryResolveUInt(ref context, eInt, out var eIntVal))
                    {
                        context.Builder.Append("{0:00}".Format(eIntVal));
                        return true;
                    }

                    context.Builder.Append("00"u8);
                    return true;
                }

            case MacroCode.Float:
                {
                    if (!payload.TryGetExpression(out var eValue, out var eRadix, out var eSeparator)
                        || !TryResolveInt(ref context, eValue, out var eValueVal)
                        || !TryResolveInt(ref context, eRadix, out var eRadixVal))
                    {
                        return false;
                    }

                    var (integerPart, fractionalPart) = int.DivRem(eValueVal, eRadixVal);
                    if (fractionalPart < 0)
                    {
                        integerPart--;
                        fractionalPart += eRadixVal;
                    }

                    context.Builder.Append(integerPart.ToString());
                    ResolveStringExpression(ref context, eSeparator);

                    // brain fried code
                    Span<byte> fractionalDigits = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
                    var pos = fractionalDigits.Length - 1;
                    for (var r = eRadixVal; r > 1; r /= 10)
                    {
                        fractionalDigits[pos--] = (byte)('0' + fractionalPart % 10);
                        fractionalPart /= 10;
                    }

                    context.Builder.Append(fractionalDigits[(pos + 1)..]);
                    return true;
                }

            case MacroCode.Sheet:
                {
                    var eColParamValue = 0u;

                    var enu = payload.GetEnumerator();
                    if (!enu.MoveNext())
                        return false;

                    var eSheetName = enu.Current;
                    if (!eSheetName.TryGetString(out var eSheetNameStr))
                        return false;

                    if (!enu.MoveNext())
                        return false;

                    var eRowId = enu.Current;
                    if (!TryResolveUInt(ref context, eRowId, out var eRowIdValue))
                        return false;

                    if (!enu.MoveNext())
                        return false;

                    var eColIndex = enu.Current;
                    if (!TryResolveUInt(ref context, eColIndex, out var eColIndexValue))
                        return false;

                    if (!enu.MoveNext())
                        goto processSheet;

                    if (!TryResolveUInt(ref context, enu.Current, out eColParamValue))
                        return false;

processSheet:
                    var resolvedSheetName = Evaluate(eSheetNameStr, context with { Builder = new() }).ExtractText();

                    var sheet = DataManager.Excel.GetSheetRaw(resolvedSheetName, (context.Language ?? TextService.ClientLanguage.Value).ToLumina());
                    if (sheet == null)
                        return false;

                    var row = sheet.GetRow(eRowIdValue);
                    if (row == null)
                        return false;

                    var column = row.ReadColumnRaw((int)eColIndexValue);
                    if (column == null)
                        return false;

                    var expressions = payload.Body[(eSheetName.Body.Length + eRowId.Body.Length + eColIndex.Body.Length)..];

                    switch (column)
                    {
                        case SeString val:
                            context.Builder.Append(Evaluate(val, context with { Builder = new(), LocalParameters = [eColParamValue] }));
                            return true;

                        // TODO: these should be strings. see Client::UI::Misc::RaptureTextModule___Component::Text::MacroDecoder_vf32 into Component::Text::TextModule_vf15
                        // need to parse the raw number string then in TryResolveUInt
                        case bool val:
                            context.Builder.BeginMacro(MacroCode.Num).AppendUIntExpression(val ? 1u : 0).EndMacro();
                            return true;

                        case sbyte val:
                            context.Builder.BeginMacro(MacroCode.Num).AppendIntExpression(val).EndMacro();
                            return true;

                        case byte val:
                            context.Builder.BeginMacro(MacroCode.Num).AppendUIntExpression(val).EndMacro();
                            return true;

                        case short val:
                            context.Builder.BeginMacro(MacroCode.Num).AppendIntExpression(val).EndMacro();
                            return true;

                        case ushort val:
                            context.Builder.BeginMacro(MacroCode.Num).AppendUIntExpression(val).EndMacro();
                            return true;

                        case int val:
                            context.Builder.BeginMacro(MacroCode.Num).AppendIntExpression(val).EndMacro();
                            return true;

                        case uint val:
                            context.Builder.BeginMacro(MacroCode.Num).AppendUIntExpression(val).EndMacro();
                            return true;

                        case { } val:
                            context.Builder.Append(val.ToString());
                            return true;
                    }

                    return false;
                }

            case MacroCode.String:
                return payload.TryGetExpression(out var eStr) && ResolveStringExpression(ref context, eStr);

            case MacroCode.Head:
                {
                    if (!payload.TryGetExpression(out eStr))
                        return false;

                    var headContext = context with { Builder = new() };

                    if (!ResolveStringExpression(ref headContext, eStr))
                        return false;

                    var str = headContext.Builder.ToReadOnlySeString();
                    var pIdx = 0;

                    foreach (var p in str)
                    {
                        pIdx++;

                        if (p.Type == ReadOnlySePayloadType.Invalid)
                            continue;

                        if (pIdx == 1 && p.Type == ReadOnlySePayloadType.Text)
                        {
                            context.Builder.Append(Encoding.UTF8.GetString(p.Body.ToArray()).FirstCharToUpper());
                            continue;
                        }

                        context.Builder.Append(p);
                    }

                    return true;
                }

            case MacroCode.HeadAll:
                {
                    if (!payload.TryGetExpression(out eStr))
                        return false;

                    var headContext = context with { Builder = new() };

                    if (!ResolveStringExpression(ref headContext, eStr))
                        return false;

                    var str = headContext.Builder.ToReadOnlySeString();

                    foreach (var p in str)
                    {
                        if (p.Type == ReadOnlySePayloadType.Invalid)
                            continue;

                        if (p.Type == ReadOnlySePayloadType.Text)
                        {
                            var cultureInfo = TextService.ClientLanguage.Value == context.Language
                                ? TextService.CultureInfo.Value
                                : TextService.GetCultureInfoFromLangCode((context.Language ?? ClientLanguage.English).ToCode());
                            context.Builder.Append(cultureInfo.TextInfo.ToTitleCase(Encoding.UTF8.GetString(p.Body.ToArray())));
                            continue;
                        }

                        context.Builder.Append(p);
                    }

                    return true;
                }

            case MacroCode.ColorType:
                {
                    if (payload.TryGetExpression(out var eColorType)
                        && TryResolveUInt(ref context, eColorType, out var eColorTypeVal))
                    {
                        if (eColorTypeVal == 0)
                            context.Builder.PopColor();
                        else if (ExcelService.GetRow<UIColor>(eColorTypeVal) is { } row)
                            context.Builder.PushColorBgra((row.UIForeground >> 8) | (row.UIForeground << 24));
                        return true;
                    }

                    return false;
                }

            case MacroCode.EdgeColorType:
                {
                    if (payload.TryGetExpression(out var eColorType) &&
                        TryResolveUInt(ref context, eColorType, out var eColorTypeVal))
                    {
                        if (eColorTypeVal == 0)
                            context.Builder.PopEdgeColor();
                        else if (ExcelService.GetRow<UIColor>(eColorTypeVal) is { } row)
                            context.Builder.PushEdgeColorBgra((row.UIForeground >> 8) | (row.UIForeground << 24));
                        return true;
                    }

                    return false;
                }

            case MacroCode.LevelPos:
                {
                    if (!payload.TryGetExpression(out var eLevel)
                        || !TryResolveUInt(ref context, eLevel, out var eLevelVal))
                    {
                        goto invalidLevelPos;
                    }

                    var level = ExcelService.GetRow<Level>(eLevelVal);
                    if (level == null || level.Map.Value == null)
                        goto invalidLevelPos;

                    var placeName = ExcelService.GetRow<PlaceName>(level.Map.Value.PlaceName.Row, context.Language);
                    if (placeName == null)
                        goto invalidLevelPos;

                    var levelFormatRow = ExcelService.GetRow<Addon>(1637, context.Language);
                    if (levelFormatRow == null)
                        goto invalidLevelPos;

                    var addonSeString = levelFormatRow.Text.RawData;
                    var mapPosX = ConvertRawToMapPosX(level.Map.Value, level.X);
                    var mapPosY = ConvertRawToMapPosY(level.Map.Value, level.Z); // Z is [sic]

                    context.Builder.Append(
                        Evaluate(
                            (ReadOnlySeString)addonSeString.ToArray(),
                            new SeStringContext() { LocalParameters = [placeName.Name, mapPosX, mapPosY] }));
                    return true;

invalidLevelPos:
                    context.Builder.Append("??? ( ???  , ??? )"); // TODO: missing new line?
                    return false;

                    // "41 0F BF C0 66 0F 6E D0 B8"
                    static uint ConvertRawToMapPos(Map map, short offset, float value)
                    {
                        var scale = map.SizeFactor / 100.0f;
                        return (uint)(10 - (int)(((value + offset) * scale + 1024f) * -0.2f / scale));
                    }

                    static uint ConvertRawToMapPosX(Map map, float x)
                        => ConvertRawToMapPos(map, map.OffsetX, x);

                    static uint ConvertRawToMapPosY(Map map, float y)
                        => ConvertRawToMapPos(map, map.OffsetY, y);
                }

            case MacroCode.JaNoun:
                return TryResolveNoun(ClientLanguage.Japanese, ref context, ref payload);

            case MacroCode.EnNoun:
                return TryResolveNoun(ClientLanguage.English, ref context, ref payload);

            case MacroCode.DeNoun:
                return TryResolveNoun(ClientLanguage.German, ref context, ref payload);

            case MacroCode.FrNoun:
                return TryResolveNoun(ClientLanguage.French, ref context, ref payload);

            default:
                context.Builder.Append(payload);
                return false;
        }
    }

    public unsafe bool TryResolveUInt(ref SeStringContext context, ReadOnlySeExpressionSpan expression, out uint value)
    {
        if (expression.TryGetUInt(out value))
            return true;

        if (expression.TryGetPlaceholderExpression(out var exprType))
        {
            // if (ctx.TryGetPlaceholderNum(exprType, out value))
            //     return true;

            switch ((ExpressionType)exprType)
            {
                case ExpressionType.Millisecond:
                    value = (uint)DateTime.Now.Millisecond;
                    return true;
                case ExpressionType.Second:
                    value = (uint)MacroDecoder.GetMacroTime()->tm_sec;
                    return true;
                case ExpressionType.Minute:
                    value = (uint)MacroDecoder.GetMacroTime()->tm_min;
                    return true;
                case ExpressionType.Hour:
                    value = (uint)MacroDecoder.GetMacroTime()->tm_hour;
                    return true;
                case ExpressionType.Day:
                    value = (uint)MacroDecoder.GetMacroTime()->tm_mday;
                    return true;
                case ExpressionType.Weekday:
                    value = (uint)MacroDecoder.GetMacroTime()->tm_wday;
                    return true;
                case ExpressionType.Month:
                    value = (uint)MacroDecoder.GetMacroTime()->tm_mon + 1;
                    return true;
                case ExpressionType.Year:
                    value = (uint)MacroDecoder.GetMacroTime()->tm_year + 1900;
                    return true;
                default:
                    return false;
            }
        }

        if (expression.TryGetParameterExpression(out exprType, out var operand1))
        {
            if (!TryResolveUInt(ref context, operand1, out var paramIndex))
                return false;
            if (paramIndex == 0)
                return false;
            paramIndex--;
            switch ((ExpressionType)exprType)
            {
                case ExpressionType.LocalNumber: // lnum
                    return context.TryGetLNum((int)paramIndex, out value);

                case ExpressionType.GlobalNumber: // gnum
                    return TryGetGNumDefault(paramIndex, out value);

                case ExpressionType.GlobalString: // gstr
                case ExpressionType.LocalString: // lstr
                default:
                    return false;
            }
        }

        if (expression.TryGetBinaryExpression(out exprType, out operand1, out var operand2))
        {
            switch ((ExpressionType)exprType)
            {
                case ExpressionType.GreaterThanOrEqualTo:
                case ExpressionType.GreaterThan:
                case ExpressionType.LessThanOrEqualTo:
                case ExpressionType.LessThan:
                    if (!TryResolveInt(ref context, operand1, out var value1)
                        || !TryResolveInt(ref context, operand2, out var value2))
                    {
                        return false;
                    }

                    value = (ExpressionType)exprType switch
                    {
                        ExpressionType.GreaterThanOrEqualTo => value1 >= value2 ? 1u : 0u,
                        ExpressionType.GreaterThan => value1 > value2 ? 1u : 0u,
                        ExpressionType.LessThanOrEqualTo => value1 <= value2 ? 1u : 0u,
                        ExpressionType.LessThan => value1 < value2 ? 1u : 0u,
                        _ => 0u,
                    };
                    return true;

                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                    if (TryResolveInt(ref context, operand1, out value1) && TryResolveInt(ref context, operand2, out value2))
                    {
                        if ((ExpressionType)exprType == ExpressionType.Equal)
                            value = value1 == value2 ? 1u : 0u;
                        else
                            value = value1 == value2 ? 0u : 1u;
                        return true;
                    }

                    if (operand1.TryGetString(out var strval1) && operand2.TryGetString(out var strval2))
                    {
                        var resolvedStr1 = Evaluate(new ReadOnlySeStringSpan(strval1.Data), context);
                        var resolvedStr2 = Evaluate(new ReadOnlySeStringSpan(strval1.Data), context);

                        if ((ExpressionType)exprType == ExpressionType.Equal)
                            value = resolvedStr1.Equals(resolvedStr2) ? 1u : 0u;
                        else
                            value = resolvedStr1.Equals(resolvedStr2) ? 0u : 1u;
                        return true;
                    }

                    // compare int with string, string with int??

                    return true;

                default:
                    return false;
            }
        }

        if (expression.TryGetString(out var str))
        {
            var evaluatedStr = Evaluate(str, context with { Builder = new() });

            foreach (var payload in evaluatedStr)
            {
                if (!payload.TryGetExpression(out var expr))
                    return false;

                return TryResolveUInt(ref context, expr, out value);
            }

            return false;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryResolveInt(ref SeStringContext ctx, ReadOnlySeExpressionSpan expression, out int value)
    {
        if (TryResolveUInt(ref ctx, expression, out var u32))
        {
            value = (int)u32;
            return true;
        }

        value = 0;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryResolveBool(ref SeStringContext ctx, ReadOnlySeExpressionSpan expression, out bool value)
    {
        if (TryResolveUInt(ref ctx, expression, out var u32))
        {
            value = u32 != 0;
            return true;
        }

        value = false;
        return false;
    }

    public bool ResolveStringExpression(ref SeStringContext ctx, ReadOnlySeExpressionSpan expression)
    {
        uint u32;

        if (expression.TryGetString(out var innerString))
        {
            Evaluate(innerString, ctx);
            return true;
        }

        /*
        if (expression.TryGetPlaceholderExpression(out var exprType))
        {
            if (ctx.TryProducePlaceholder(ref ctx, exprType))
                return true;
        }
        */

        if (expression.TryGetParameterExpression(out var exprType, out var operand1))
        {
            if (!TryResolveUInt(ref ctx, operand1, out var paramIndex))
                return false;
            if (paramIndex == 0)
                return false;
            paramIndex--;
            switch ((ExpressionType)exprType)
            {
                case ExpressionType.LocalNumber: // lnum
                    if (!ctx.TryGetLNum((int)paramIndex, out u32))
                        return false;

                    ctx.Builder.Append(unchecked((int)u32).ToString());
                    return true;

                case ExpressionType.LocalString: // lstr
                    if (!ctx.TryGetLStr((int)paramIndex, out var str))
                        return false;

                    ctx.Builder.Append(str);
                    return true;

                case ExpressionType.GlobalNumber: // gnum
                    if (!TryGetGNumDefault(paramIndex, out u32))
                        return false;

                    ctx.Builder.Append(unchecked((int)u32).ToString());
                    return true;

                case ExpressionType.GlobalString: // gstr
                    return TryProduceGStrDefault(ref ctx, paramIndex);

                default:
                    return false;
            }
        }

        // Handles UInt and Binary expressions
        if (!TryResolveUInt(ref ctx, expression, out u32))
            return false;

        ctx.Builder.Append(((int)u32).ToString());
        return true;
    }

    private bool TryResolveNoun(ClientLanguage language, ref SeStringContext context, ref ReadOnlySePayloadSpan payload)
    {
        var eAmountVal = 1;
        var eCaseVal = 1;
        var eUnkInt5Val = 1;

        var enu = payload.GetEnumerator();
        if (!enu.MoveNext())
            return false;

        var eSheetName = enu.Current;

        if (!eSheetName.TryGetString(out var eSheetNameStr))
            return false;

        var resolvedSheetName = Evaluate(eSheetNameStr, context with { Builder = new() }).ExtractText();

        if (!enu.MoveNext())
            return false;

        var ePerson = enu.Current;
        if (!TryResolveInt(ref context, ePerson, out var ePersonVal))
            return false;

        if (!enu.MoveNext())
            return false;

        var eRowId = enu.Current;
        if (!TryResolveInt(ref context, eRowId, out var eRowIdVal))
            return false;

        if (!enu.MoveNext())
            goto decode;

        var eAmount = enu.Current;
        if (!TryResolveInt(ref context, eAmount, out eAmountVal))
            return false;

        if (!enu.MoveNext())
            goto decode;

        var eCase = enu.Current;
        if (!TryResolveInt(ref context, eCase, out eCaseVal))
            return false;

        if (!enu.MoveNext())
            goto decode;

        var eUnkInt5 = enu.Current;
        if (!TryResolveInt(ref context, eUnkInt5, out eUnkInt5Val))
            return false;

decode:
        context.Builder.Append(TextDecoder.ProcessNoun(language, resolvedSheetName, ePersonVal, eRowIdVal, eAmountVal, eCaseVal, eUnkInt5Val));

        return true;
    }
}
