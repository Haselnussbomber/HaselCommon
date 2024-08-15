using System.Runtime.CompilerServices;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Component.Text;
using HaselCommon.Services.SeStringEvaluation;
using Lumina.Excel.GeneratedSheets;
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
    TranslationManager translationManager,
    TextureService textureService)
{
    private readonly IDataManager DataManager = dataManager;
    private readonly ExcelService ExcelService = excelService;
    private readonly TranslationManager TranslationManager = translationManager;
    private readonly TextureService TextureService = textureService;

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
        context.Language ??= TranslationManager.ClientLanguage;

        foreach (var payload in str)
            ResolveStringPayload(ref context, payload);

        return context.Builder.ToReadOnlySeString();
    }

    public ReadOnlySeString EvaluateFromAddon(uint addonId, SeStringContext context)
    {
        context.Language ??= TranslationManager.ClientLanguage;

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
                return ResolveStringPayload(
                    ref ctx,
                    ReadOnlySePayloadSpan.FromText(MemoryMarshal.CreateReadOnlySpanFromNullTerminated(p.StringValue)));

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
                    if (!payload.TryGetExpression(out var eSheetName, out var eRowId, out var eColIndex)
                        || !TryResolveUInt(ref context, eRowId, out var eRowIdValue)
                        || !TryResolveUInt(ref context, eColIndex, out var eColIndexValue)
                        || !eSheetName.TryGetString(out var eSheetNameStr))
                    {
                        return false;
                    }

                    var resolvedSheetName = Evaluate(eSheetNameStr, new SeStringContext()
                    {
                        LocalParameters = context.LocalParameters,
                        Language = context.Language,
                        StripSoftHypen = context.StripSoftHypen
                    }).ExtractText();

                    var sheet = DataManager.Excel.GetSheetRaw(resolvedSheetName, (context.Language ?? TranslationManager.ClientLanguage).ToLumina());
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
                        case bool val:
                            context.Builder.Append(val ? "true"u8 : "false"u8);
                            return true;

                        case Lumina.Text.SeString val:
                            context.Builder.Append(Evaluate(val, new SeStringContext()
                            {
                                LocalParameters = context.LocalParameters,
                                Language = context.Language,
                                StripSoftHypen = context.StripSoftHypen
                            }));
                            return true;

                        case { } val:
                            context.Builder.Append(val.ToString());
                            return true;
                    }

                    return false;
                }

            case MacroCode.String:
                return payload.TryGetExpression(out var eStr) && ResolveStringExpression(ref context, eStr);

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
}
