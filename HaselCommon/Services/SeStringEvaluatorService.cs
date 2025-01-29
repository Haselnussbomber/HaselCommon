using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Dalamud.Game;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Component.Text;
using HaselCommon.Extensions.Dalamud;
using HaselCommon.Extensions.Strings;
using HaselCommon.Services.SeStringEvaluation;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Text;
using Lumina.Text.Expressions;
using Lumina.Text.Payloads;
using Lumina.Text.ReadOnly;
using Microsoft.Extensions.Logging;

namespace HaselCommon.Services;

/// <summary>Evaluator for SeStrings.</summary>
[RegisterSingleton]
public partial class SeStringEvaluatorService(
    ILogger<SeStringEvaluatorService> logger,
    LanguageProvider languageProvider,
    ExcelService excelService,
    NounProcessor nounProcessor)
{
    private readonly ExcelService _excelService = excelService;
    private readonly LanguageProvider _languageProvider = languageProvider;
    private readonly NounProcessor _nounProcessor = nounProcessor;

    public ReadOnlySeString Evaluate(byte[] str)
        => Evaluate(str, new());

    public ReadOnlySeString Evaluate(ReadOnlySeString str)
        => Evaluate(str, new());

    public ReadOnlySeString Evaluate(ReadOnlySeStringSpan str)
        => Evaluate(str, new());

    public ReadOnlySeString Evaluate(byte[] str, SeStringContext context)
        => Evaluate((ReadOnlySeStringSpan)str, context);

    public ReadOnlySeString Evaluate(ReadOnlySeString str, SeStringContext context)
        => Evaluate(str.AsSpan(), context);

    public ReadOnlySeString Evaluate(ReadOnlySeStringSpan str, SeStringContext context)
    {
        context.Language ??= _languageProvider.ClientLanguage;

        foreach (var payload in str)
            ResolveStringPayload(ref context, payload);

        return context.Builder.ToReadOnlySeString();
    }

    public ReadOnlySeString EvaluateFromAddon(uint addonId, SeStringContext context)
    {
        context.Language ??= _languageProvider.ClientLanguage;

        if (!_excelService.TryGetRow<Addon>(addonId, context.Language, out var addonRow))
            return new();

        return Evaluate(addonRow.Text, context);
    }

    public ReadOnlySeString EvaluateFromLobby(uint lobbyId, SeStringContext context)
    {
        context.Language ??= _languageProvider.ClientLanguage;

        if (!_excelService.TryGetRow<Lobby>(lobbyId, context.Language, out var lobbyRow))
            return new();

        return Evaluate(lobbyRow.Text, context);
    }

    public unsafe bool TryGetGNumDefault(uint parameterIndex, out uint value)
    {
        value = 0u;

        var rtm = RaptureTextModule.Instance();
        if (rtm is null)
            return false;

        if (!ThreadSafety.IsMainThread)
        {
            logger.LogError("Global parameters may only be used from the main thread.");
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
                logger.LogError("Requested a number; Utf8String global parameter at {parameterIndex}.", parameterIndex);
                return false;

            case TextParameterType.String:
                logger.LogError("Requested a number; string global parameter at {parameterIndex}.", parameterIndex);
                return false;

            case TextParameterType.Uninitialized:
                logger.LogError("Requested a number; uninitialized global parameter at {parameterIndex}.", parameterIndex);
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
            logger.LogError("Global parameters may only be used from the main thread.");
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
                return TryResolveSetResetTime(ref context, payload);

            case MacroCode.SetTime:
                return TryResolveSetTime(ref context, payload);

            case MacroCode.If:
                return TryResolveIf(ref context, payload);

            case MacroCode.Switch:
                return TryResolveSwitch(ref context, payload);

            case MacroCode.NewLine:
                context.Builder.AppendNewLine();
                return true;

            case MacroCode.Icon:
            case MacroCode.Icon2: // TODO: Icon2 handles controller icons based on button mapping - https://discord.com/channels/581875019861328007/653504487352303619/1235201758012112977
                return TryResolveIcon(ref context, payload);

            case MacroCode.Color:
                return TryResolveColor(ref context, payload);

            case MacroCode.EdgeColor:
                return TryResolveEdgeColor(ref context, payload);

            //case MacroCode.SoftHyphen:
            //    if (!context.StripSoftHypen)
            //        context.Builder.Append("\u00AD"u8);
            //    return true;

            case MacroCode.Bold:
                return TryResolveBold(ref context, payload);

            case MacroCode.Italic:
                return TryResolveItalic(ref context, payload);

            //case MacroCode.NonBreakingSpace:
            //    context.Builder.Append("\u00A0"u8);
            //    return true;

            //case MacroCode.Hyphen:
            //    context.Builder.Append('-');
            //    return true;

            case MacroCode.Num:
                return TryResolveNum(ref context, payload);

            case MacroCode.Hex:
                return TryResolveHex(ref context, payload);

            case MacroCode.Kilo:
                return TryResolveKilo(ref context, payload);

            case MacroCode.Digit:
                return TryResolveDigit(ref context, payload);

            case MacroCode.Sec:
                return TryResolveSec(ref context, payload);

            case MacroCode.Float:
                return TryResolveFloat(ref context, payload);

            case MacroCode.Sheet:
                return TryResolveSheet(ref context, payload);

            case MacroCode.String:
                return payload.TryGetExpression(out var eStr) && ResolveStringExpression(ref context, eStr);

            case MacroCode.Head:
                return TryResolveHead(ref context, payload);

            case MacroCode.HeadAll:
                return TryResolveHeadAll(ref context, payload);

            case MacroCode.ColorType:
                return TryResolveColorType(ref context, payload);

            case MacroCode.EdgeColorType:
                return TryResolveEdgeColorType(ref context, payload);

            case MacroCode.LevelPos:
                return TryResolveLevelPos(ref context, payload);

            case MacroCode.Fixed:
                // second function in Client::UI::Misc::PronounModule_ProcessString
                return TryResolveFixed(ref context, payload);

            case MacroCode.JaNoun:
                return TryResolveNoun(ClientLanguage.Japanese, ref context, payload);

            case MacroCode.EnNoun:
                return TryResolveNoun(ClientLanguage.English, ref context, payload);

            case MacroCode.DeNoun:
                return TryResolveNoun(ClientLanguage.German, ref context, payload);

            case MacroCode.FrNoun:
                return TryResolveNoun(ClientLanguage.French, ref context, payload);

            default:
                context.Builder.Append(payload);
                return false;
        }
    }

    private unsafe bool TryResolveSetResetTime(ref SeStringContext context, in ReadOnlySePayloadSpan payload)
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

    private unsafe bool TryResolveSetTime(ref SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eTime) || !TryResolveUInt(ref context, eTime, out var eTimeVal))
            return false;

        var date = DateTimeOffset.FromUnixTimeSeconds(eTimeVal).LocalDateTime;
        MacroDecoder.GetMacroTime()->SetTime(date);

        return true;
    }

    private bool TryResolveIf(ref SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        return
            payload.TryGetExpression(out var eCond, out var eTrue, out var eFalse)
            && ResolveStringExpression(
                ref context,
                TryResolveBool(ref context, eCond, out var eCondVal) && eCondVal
                    ? eTrue
                    : eFalse);
    }

    private bool TryResolveSwitch(ref SeStringContext context, in ReadOnlySePayloadSpan payload)
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

    private bool TryResolveIcon(ref SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eIcon) || !TryResolveUInt(ref context, eIcon, out var eIconValue))
            return false;

        context.Builder.AppendIcon(eIconValue);

        return true;
    }

    private bool TryResolveColor(ref SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eColor))
            return false;

        if (eColor.TryGetPlaceholderExpression(out var ph) && ph == (int)ExpressionType.StackColor)
            context.Builder.PopColor();
        else if (TryResolveUInt(ref context, eColor, out var eColorVal))
            context.Builder.PushColorBgra(eColorVal);

        return true;
    }

    private bool TryResolveEdgeColor(ref SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eColor))
            return false;

        if (eColor.TryGetPlaceholderExpression(out var ph) && ph == (int)ExpressionType.StackColor)
            context.Builder.PopEdgeColor();
        else if (TryResolveUInt(ref context, eColor, out var eColorVal))
            context.Builder.PushEdgeColorBgra(eColorVal);

        return true;
    }

    private bool TryResolveBold(ref SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eEnable) || !TryResolveBool(ref context, eEnable, out var eEnableVal))
            return false;

        context.Builder.AppendSetBold(eEnableVal);

        return true;
    }

    private bool TryResolveItalic(ref SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eEnable) || !TryResolveBool(ref context, eEnable, out var eEnableVal))
            return false;

        context.Builder.AppendSetItalic(eEnableVal);

        return true;
    }

    private bool TryResolveNum(ref SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eInt) || !TryResolveInt(ref context, eInt, out var eIntVal))
        {
            context.Builder.Append('0');
            return true;
        }

        context.Builder.Append(eIntVal.ToString());

        return true;
    }

    private bool TryResolveHex(ref SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eUInt) || !TryResolveUInt(ref context, eUInt, out var eUIntVal))
        {
            context.Builder.Append("0x00000000"u8);
            return true;
        }

        context.Builder.Append("0x{0:X08}".Format(eUIntVal));

        return true;
    }

    private bool TryResolveKilo(ref SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eInt, out var eSep) || !TryResolveInt(ref context, eInt, out var eIntVal))
        {
            context.Builder.Append('0');
            return true;
        }

        if (eIntVal == int.MinValue)
        {
            // -2147483648
            context.Builder.Append("-2"u8);
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

    private bool TryResolveDigit(ref SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eValue, out var eTargetLength))
            return false;

        if (!TryResolveInt(ref context, eValue, out var eValueVal))
            return false;

        if (!TryResolveInt(ref context, eTargetLength, out var eTargetLengthVal))
            return false;

        context.Builder.Append(eValueVal.ToString(new string('0', eTargetLengthVal)));

        return true;
    }

    private bool TryResolveSec(ref SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (payload.TryGetExpression(out var eInt) && TryResolveUInt(ref context, eInt, out var eIntVal))
        {
            context.Builder.Append("{0:00}".Format(eIntVal));
            return true;
        }

        context.Builder.Append("00"u8);

        return true;
    }

    private bool TryResolveFloat(ref SeStringContext context, in ReadOnlySePayloadSpan payload)
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

    private bool TryResolveSheet(ref SeStringContext context, in ReadOnlySePayloadSpan payload)
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

        ResolveSheetRedirect(ref resolvedSheetName, ref eRowIdValue);
        if (string.IsNullOrEmpty(resolvedSheetName))
            return false;

        if (!_excelService.HasSheet(resolvedSheetName))
            return false;

        if (!_excelService.TryGetRawRow(resolvedSheetName, eRowIdValue, context.Language ?? _languageProvider.ClientLanguage, out var row))
            return false;

        var column = row.ReadColumn((int)eColIndexValue);
        if (column == null)
            return false;

        // TODO: why was this here?
        // var expressions = payload.Body[(eSheetName.Body.Length + eRowId.Body.Length + eColIndex.Body.Length)..];

        switch (column)
        {
            case ReadOnlySeString val:
                context.Builder.Append(Evaluate(val, context with { Builder = new(), LocalParameters = [eColParamValue] }));
                return true;

            case bool val:
                context.Builder.Append((val ? 1u : 0).ToString("D", CultureInfo.InvariantCulture));
                return true;

            case sbyte val:
                context.Builder.Append(val.ToString("D", CultureInfo.InvariantCulture));
                return true;

            case byte val:
                context.Builder.Append(val.ToString("D", CultureInfo.InvariantCulture));
                return true;

            case short val:
                context.Builder.Append(val.ToString("D", CultureInfo.InvariantCulture));
                return true;

            case ushort val:
                context.Builder.Append(val.ToString("D", CultureInfo.InvariantCulture));
                return true;

            case int val:
                context.Builder.Append(val.ToString("D", CultureInfo.InvariantCulture));
                return true;

            case uint val:
                context.Builder.Append(val.ToString("D", CultureInfo.InvariantCulture));
                return true;

            case { } val:
                context.Builder.Append(val.ToString());
                return true;
        }

        return false;
    }

    private bool TryResolveHead(ref SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eStr))
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

    private bool TryResolveHeadAll(ref SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eStr))
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
                var cultureInfo = _languageProvider.ClientLanguage == context.Language
                    ? _languageProvider.CultureInfo
                    : LanguageProvider.GetCultureInfoFromLangCode((context.Language ?? ClientLanguage.English).ToCode());
                context.Builder.Append(cultureInfo.TextInfo.ToTitleCase(Encoding.UTF8.GetString(p.Body.ToArray())));
                continue;
            }

            context.Builder.Append(p);
        }

        return true;
    }

    private bool TryResolveColorType(ref SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eColorType) || !TryResolveUInt(ref context, eColorType, out var eColorTypeVal))
            return false;

        if (eColorTypeVal == 0)
            context.Builder.PopColor();
        else if (_excelService.TryGetRow<UIColor>(eColorTypeVal, out var row))
            context.Builder.PushColorBgra((row.UIForeground >> 8) | (row.UIForeground << 24));

        return true;
    }

    private bool TryResolveEdgeColorType(ref SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eColorType) || !TryResolveUInt(ref context, eColorType, out var eColorTypeVal))
            return false;

        if (eColorTypeVal == 0)
            context.Builder.PopEdgeColor();
        else if (_excelService.TryGetRow<UIColor>(eColorTypeVal, out var row))
            context.Builder.PushEdgeColorBgra((row.UIForeground >> 8) | (row.UIForeground << 24));

        return true;
    }

    private bool TryResolveLevelPos(ref SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eLevel) || !TryResolveUInt(ref context, eLevel, out var eLevelVal))
            goto invalidLevelPos;

        if (!_excelService.TryGetRow<Level>(eLevelVal, context.Language, out var level) || !level.Map.IsValid)
            goto invalidLevelPos;

        if (!_excelService.TryGetRow<PlaceName>(level.Map.Value.PlaceName.RowId, context.Language, out var placeName))
            goto invalidLevelPos;

        if (!_excelService.TryGetRow<Addon>(1637, context.Language, out var levelFormatRow))
            goto invalidLevelPos;

        var mapPosX = ConvertRawToMapPosX(level.Map.Value, level.X);
        var mapPosY = ConvertRawToMapPosY(level.Map.Value, level.Z); // Z is [sic]

        context.Builder.Append(
            Evaluate(
                levelFormatRow.Text,
                new SeStringContext() { LocalParameters = [placeName.Name, mapPosX, mapPosY] }));

        return true;

invalidLevelPos:
        context.Builder.Append("??? ( ???  , ??? )"); // TODO: missing new line?
        return false;
    }

    private bool TryResolveFixed(ref SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        var enu = payload.GetEnumerator();

        if (!enu.MoveNext() || !TryResolveInt(ref context, enu.Current, out var e0Val))
            return false;

        if (!enu.MoveNext() || !TryResolveInt(ref context, enu.Current, out var e1Val))
            return false;

        return e0Val switch
        {
            100 or 200 => e1Val switch
            {
                1 => TryResolveFixedPlayerLink(ref context, ref enu),
                2 => TryResolveFixedClassJobLevel(ref context, ref enu),
                3 => TryResolveFixedMapLink(ref context, ref enu),
                4 => TryResolveFixedItemLink(ref context, ref enu),
                5 => TryResolveFixedChatSoundEffect(ref context, ref enu),
                6 => TryResolveFixedObjStr(ref context, ref enu),
                7 => TryResolveFixedString(ref context, ref enu),
                8 => TryResolveFixedTimeRemaining(ref context, ref enu),
                // Reads a uint and saves it to PronounModule+0x3AC
                // TODO: handle this? looks like it's for the mentor/beginner icon of the player link in novice network
                // see "FF 50 50 8B B0"
                9 => true,
                10 => TryResolveFixedStatusLink(ref context, ref enu),
                11 => TryResolveFixedPartyFinderLink(ref context, ref enu),
                12 => TryResolveFixedQuestLink(ref context, ref enu),
                _ => false,
            },
            _ => TryResolveFixedAutoTranslation(ref context, payload, e0Val, e1Val),
        };
    }

    private unsafe bool TryResolveFixedPlayerLink(ref SeStringContext context, ref ReadOnlySePayloadSpan.Enumerator enu)
    {
        if (!enu.MoveNext() || !TryResolveUInt(ref context, enu.Current, out var worldId))
            return false;

        if (!enu.MoveNext() || !enu.Current.TryGetString(out var playerName))
            return false;

        if (UIGlobals.IsValidPlayerCharacterName(playerName.ExtractText()))
        {
            var flags = 0u;
            if (InfoModule.Instance()->IsInCrossWorldDuty())
                flags |= 0x10;

            context.Builder.PushLink(LinkMacroPayloadType.Character, flags, worldId, 0u, playerName);
            context.Builder.Append(playerName);
            context.Builder.PopLink();
        }
        else
        {
            context.Builder.Append(playerName);
        }

        if (worldId == AgentLobby.Instance()->LobbyData.HomeWorldId)
            return true;

        if (!_excelService.TryGetRow<World>(worldId, context.Language, out var worldRow))
            return false;

        context.Builder.AppendIcon(88);
        context.Builder.Append(worldRow.Name);

        return true;
    }

    private bool TryResolveFixedClassJobLevel(ref SeStringContext context, ref ReadOnlySePayloadSpan.Enumerator enu)
    {
        if (!enu.MoveNext() || !TryResolveInt(ref context, enu.Current, out var classJobId) || classJobId <= 0)
            return false;

        if (!enu.MoveNext() || !TryResolveInt(ref context, enu.Current, out var level))
            return false;

        if (!_excelService.TryGetRow<ClassJob>((uint)classJobId, context.Language, out var classJobRow))
            return false;

        context.Builder.Append(classJobRow.Name);

        if (level != 0)
        {
            context.Builder.Append('(');
            context.Builder.Append(level.ToString("D", CultureInfo.InvariantCulture));
            context.Builder.Append(')');
        }

        return true;
    }

    private bool TryResolveFixedMapLink(ref SeStringContext context, ref ReadOnlySePayloadSpan.Enumerator enu)
    {
        if (!enu.MoveNext() || !TryResolveUInt(ref context, enu.Current, out var territoryTypeId))
            return false;

        if (!enu.MoveNext() || !TryResolveUInt(ref context, enu.Current, out var packedIds))
            return false;

        if (!enu.MoveNext() || !TryResolveInt(ref context, enu.Current, out var rawX))
            return false;

        if (!enu.MoveNext() || !TryResolveInt(ref context, enu.Current, out var rawY))
            return false;

        if (!enu.MoveNext() || !TryResolveInt(ref context, enu.Current, out var rawZ))
            return false;

        if (!enu.MoveNext() || !TryResolveUInt(ref context, enu.Current, out var placeNameIdInt))
            return false;

        var instance = packedIds >> 0x10;
        var mapId = packedIds & 0xFF;

        if (_excelService.TryGetRow<TerritoryType>(territoryTypeId, context.Language, out var territoryTypeRow))
        {
            if (!_excelService.TryGetRow<PlaceName>(placeNameIdInt == 0 ? territoryTypeRow.PlaceName.RowId : placeNameIdInt, context.Language, out var placeNameRow))
                return false;

            if (!_excelService.TryGetRow<Map>(mapId, out var mapRow))
                return false;

            var sb = SeStringBuilder.SharedPool.Get();

            sb.Append(placeNameRow.Name);
            if (instance > 0 && instance <= 9)
                sb.Append((char)((char)0xE0B0 + (char)instance));

            var placeNameWithInstance = sb.ToReadOnlySeString();
            SeStringBuilder.SharedPool.Return(sb);

            var mapPosX = ConvertRawToMapPosX(mapRow, rawX / 1000f);
            var mapPosY = ConvertRawToMapPosY(mapRow, rawY / 1000f);

            ReadOnlySeString linkText;
            if (rawZ == -30000)
            {
                linkText = EvaluateFromAddon(1635, new SeStringContext()
                {
                    Language = context.Language,
                    LocalParameters = [placeNameWithInstance, mapPosX, mapPosY]
                });
            }
            else
            {
                linkText = EvaluateFromAddon(1636, new SeStringContext()
                {
                    Language = context.Language,
                    LocalParameters = [placeNameWithInstance, mapPosX, mapPosY, rawZ / (rawZ >= 0 ? 10 : -10), rawZ]
                });
            }

            context.Builder.PushLinkMapPosition(territoryTypeId, mapId, rawX, rawY);
            context.Builder.Append(EvaluateFromAddon(371, new SeStringContext()
            {
                Language = context.Language,
                LocalParameters = [linkText]
            }));
            context.Builder.PopLink();

            return true;
        }
        else if (mapId == 0)
        {
            if (_excelService.TryGetRow<Addon>(875, out var addonRow)) // "(No location set for map link)"
                context.Builder.Append(addonRow.Text);

            return true;
        }
        else if (mapId == 1)
        {
            if (_excelService.TryGetRow<Addon>(874, out var addonRow)) // "(Map link unavailable in this area)"
                context.Builder.Append(addonRow.Text);

            return true;
        }
        else if (mapId == 2)
        {
            if (_excelService.TryGetRow<Addon>(13743, out var addonRow)) // "(Unable to set map link)"
                context.Builder.Append(addonRow.Text);

            return true;
        }

        return false;
    }

    private bool TryResolveFixedItemLink(ref SeStringContext context, ref ReadOnlySePayloadSpan.Enumerator enu)
    {
        if (!enu.MoveNext() || !TryResolveUInt(ref context, enu.Current, out var itemId))
            return false;

        if (!enu.MoveNext() || !TryResolveUInt(ref context, enu.Current, out var rarity))
            return false;

        if (!enu.MoveNext() || !TryResolveInt(ref context, enu.Current, out var unk2))
            return false;

        if (!enu.MoveNext() || !TryResolveInt(ref context, enu.Current, out var unk3))
            return false;

        if (!enu.MoveNext() || !enu.Current.TryGetString(out var itemName)) // TODO: unescape??
            return false;

        // rarity color start
        context.Builder.Append(EvaluateFromAddon(6, new SeStringContext()
        {
            Language = context.Language,
            LocalParameters = [rarity]
        }));

        var v2 = (ushort)((unk2 & 0xFF) + (unk3 << 0x10)); // TODO: find out what this does

        context.Builder.PushLink(LinkMacroPayloadType.Item, itemId, rarity, v2);

        // arrow and item name
        context.Builder.Append(EvaluateFromAddon(371, new SeStringContext()
        {
            Language = context.Language,
            LocalParameters = [itemName]
        }));

        context.Builder.PopLink();
        context.Builder.PopColor();

        return true;
    }

    private bool TryResolveFixedChatSoundEffect(ref SeStringContext context, ref ReadOnlySePayloadSpan.Enumerator enu)
    {
        if (!enu.MoveNext() || !TryResolveUInt(ref context, enu.Current, out var soundEffectId))
            return false;

        context.Builder.Append($"<se.{soundEffectId + 1}>");

        // the game would play it here

        return true;
    }

    private bool TryResolveFixedObjStr(ref SeStringContext context, ref ReadOnlySePayloadSpan.Enumerator enu)
    {
        if (!enu.MoveNext() || !TryResolveUInt(ref context, enu.Current, out var objStrId))
            return false;

        var name = EvaluateFromAddon(2025, new SeStringContext() // "<head(<denoun(ObjStr,5,lnum1,1,1,1)>)>"
        {
            Language = context.Language,
            LocalParameters = [objStrId]
        });

        // TODO: for some reason the game doesn't do that
        // i think it checks if it could get row 2025 instead of that the text is empty
        if (name.IsEmpty)
        {
            if (_excelService.TryGetRow<Addon>(3619, out var addonRow)) // "???"
                context.Builder.Append(addonRow.Text);
        }
        else
        {
            context.Builder.Append(name);
        }

        return true;
    }

    private bool TryResolveFixedString(ref SeStringContext context, ref ReadOnlySePayloadSpan.Enumerator enu)
    {
        if (!enu.MoveNext() || !enu.Current.TryGetString(out var text))
            return false;

        // formats it through vsprintf using "%s"??
        context.Builder.Append(text.ExtractText());

        return true;
    }

    private bool TryResolveFixedTimeRemaining(ref SeStringContext context, ref ReadOnlySePayloadSpan.Enumerator enu)
    {
        if (!enu.MoveNext() || !TryResolveUInt(ref context, enu.Current, out var seconds))
            return false;

        if (seconds != 0)
        {
            context.Builder.Append(EvaluateFromAddon(33, new SeStringContext() // "<num(lnum1)>:<sec(lnum2)>"
            {
                Language = context.Language,
                LocalParameters = [seconds / 60, seconds % 60]
            }));
        }
        else
        {
            if (_excelService.TryGetRow<Addon>(48, out var addonRow)) // "--:--"
                context.Builder.Append(addonRow.Text);
        }

        return true;
    }

    private bool TryResolveFixedStatusLink(ref SeStringContext context, ref ReadOnlySePayloadSpan.Enumerator enu)
    {
        if (!enu.MoveNext() || !TryResolveUInt(ref context, enu.Current, out var statusId))
            return false;

        if (!enu.MoveNext() || !TryResolveBool(ref context, enu.Current, out var hasOverride))
            return false;

        if (!_excelService.TryGetRow<Status>(statusId, out var statusRow))
            return false;

        ReadOnlySeStringSpan statusName;
        ReadOnlySeStringSpan statusDescription;

        if (hasOverride)
        {
            if (!enu.MoveNext() || !enu.Current.TryGetString(out statusName))
                return false;

            if (!enu.MoveNext() || !enu.Current.TryGetString(out statusDescription))
                return false;
        }
        else
        {
            statusName = statusRow.Name.AsSpan();
            statusDescription = statusRow.Description.AsSpan();
        }

        var sb = SeStringBuilder.SharedPool.Get();

        if (statusRow.StatusCategory == 1)
        {
            sb.Append(EvaluateFromAddon(376, new SeStringContext() // buff icon
            {
                Language = context.Language,
            }));
        }
        else if (statusRow.StatusCategory == 2)
        {
            sb.Append(EvaluateFromAddon(377, new SeStringContext() // debuff icon
            {
                Language = context.Language,
            }));
        }

        sb.Append(statusName);

        var linkText = sb.ToReadOnlySeString();
        SeStringBuilder.SharedPool.Return(sb);

        context.Builder
           .BeginMacro(MacroCode.Link)
            .AppendUIntExpression((uint)LinkMacroPayloadType.Status)
            .AppendUIntExpression(statusId)
            .AppendUIntExpression(0)
            .AppendUIntExpression(0)
            .AppendStringExpression(statusName)
            .AppendStringExpression(statusDescription)
            .EndMacro();

        context.Builder.Append(EvaluateFromAddon(371, new SeStringContext()
        {
            Language = context.Language,
            LocalParameters = [linkText]
        }));

        context.Builder.PopLink();

        return true;
    }

    private bool TryResolveFixedPartyFinderLink(ref SeStringContext context, ref ReadOnlySePayloadSpan.Enumerator enu)
    {
        if (!enu.MoveNext() || !TryResolveUInt(ref context, enu.Current, out var listingId))
            return false;

        if (!enu.MoveNext() || !TryResolveUInt(ref context, enu.Current, out var unk1))
            return false;

        if (!enu.MoveNext() || !TryResolveUInt(ref context, enu.Current, out var worldId))
            return false;

        if (!enu.MoveNext() || !TryResolveInt(ref context, enu.Current, out var crossWorldFlag)) // 0 = cross world, 1 = not cross world
            return false;

        if (!enu.MoveNext() || !enu.Current.TryGetString(out var playerName))
            return false;

        context.Builder
           .BeginMacro(MacroCode.Link)
            .AppendUIntExpression((uint)LinkMacroPayloadType.PartyFinder)
            .AppendUIntExpression(listingId)
            .AppendUIntExpression(unk1)
            .AppendUIntExpression((uint)(crossWorldFlag << 0x10) + worldId)
            .EndMacro();

        context.Builder.Append(EvaluateFromAddon(371, new SeStringContext()
        {
            Language = context.Language,
            LocalParameters = [
                EvaluateFromAddon(2265, new SeStringContext()
                {
                    Language = context.Language,
                    LocalParameters = [playerName, crossWorldFlag]
                })
            ]
        }));

        context.Builder.PopLink();

        return true;
    }

    private bool TryResolveFixedQuestLink(ref SeStringContext context, ref ReadOnlySePayloadSpan.Enumerator enu)
    {
        if (!enu.MoveNext() || !TryResolveUInt(ref context, enu.Current, out var questId))
            return false;

        if (!enu.MoveNext() || !enu.MoveNext() || !enu.MoveNext()) // unused
            return false;

        if (!enu.MoveNext() || !enu.Current.TryGetString(out var questName))
            return false;

        /* TODO some day
        if (!QuestManager.IsQuestComplete(questId) && !QuestManager.Instance()->IsQuestAccepted(questId))
        {
            var questRecompleteManager = QuestRecompleteManager.Instance();
            if (questRecompleteManager == null || !questRecompleteManager->"E8 ?? ?? ?? ?? 0F B6 57 FF"(questId)) {
                if (_excelService.TryGetRow<Addon>(5497, out var addonRow))
                    questName = addonRow.Text.AsSpan();
            }
        }
        */

        context.Builder
           .BeginMacro(MacroCode.Link)
            .AppendUIntExpression((uint)LinkMacroPayloadType.Quest)
            .AppendUIntExpression(questId)
            .AppendUIntExpression(0)
            .AppendUIntExpression(0)
            .EndMacro();

        context.Builder.Append(EvaluateFromAddon(371, new SeStringContext()
        {
            Language = context.Language,
            LocalParameters = [questName]
        }));

        context.Builder.PopLink();

        return true;
    }

    private bool TryResolveFixedAutoTranslation(ref SeStringContext context, in ReadOnlySePayloadSpan payload, int e0Val, int e1Val)
    {
        // Auto-Translation / Completion
        var group = (uint)(e0Val + 1);
        var rowId = (uint)e1Val;

        using var icons = new IconWrap(ref context, 54, 55);

        if (!_excelService.TryFindRow<Completion>(row => row.Group == group && !row.LookupTable.IsEmpty, context.Language, out var groupRow))
            return false;

        var lookupTable = (
            groupRow.LookupTable.IsTextOnly()
                ? groupRow.LookupTable
                : Evaluate(groupRow.LookupTable.AsSpan(), context with { Builder = new() })
            ).ExtractText();

        // Completion sheet
        if (lookupTable.Equals('@'))
        {
            if (_excelService.TryGetRow<Completion>(rowId, context.Language, out var completionRow))
            {
                context.Builder.Append(completionRow.Text);
            }
            return true;
        }
        // CategoryDataCache
        else if (lookupTable.Equals('#'))
        {
            // couldn't find any, so we don't handle them :p
            context.Builder.Append(payload);
            return false;
        }

        // All other sheets
        var rangesStart = lookupTable.IndexOf('[');
        RawRow row = default;
        if (rangesStart == -1) // Sheet without ranges
        {
            if (_excelService.GetSheet<RawRow>(lookupTable, context.Language).TryGetRow(rowId, out row))
            {
                context.Builder.Append(row.ReadStringColumn(0));
                return true;
            }
        }

        var sheetName = lookupTable[..rangesStart];
        var ranges = lookupTable[(rangesStart + 1)..(lookupTable.Length - 1)];
        if (ranges.Length == 0)
            return true;

        var isNoun = false;
        var col = 0;

        if (ranges.StartsWith("noun"))
        {
            isNoun = true;
        }
        else if (ranges.StartsWith("col"))
        {
            var colRangeEnd = ranges.IndexOf(',');
            if (colRangeEnd == -1)
                colRangeEnd = ranges.Length;

            col = int.Parse(ranges[4..colRangeEnd]);
        }
        else if (ranges.StartsWith("tail"))
        {
            // couldn't find any, so we don't handle them :p
            context.Builder.Append(payload);
            return false;
        }

        if (isNoun && context.Language == ClientLanguage.German && sheetName == "Companion")
        {
            context.Builder.Append(_nounProcessor.ProcessRow(sheetName, rowId, Lumina.Data.Language.German, 1, 5));
        }
        else if (_excelService.GetSheet<RawRow>(sheetName, context.Language).TryGetRow(rowId, out row))
        {
            context.Builder.Append(row.ReadStringColumn(col));
        }

        return true;
    }

    public unsafe bool TryResolveUInt(ref SeStringContext context, in ReadOnlySeExpressionSpan expression, out uint value)
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
            return (ExpressionType)exprType switch
            {
                ExpressionType.LocalNumber => context.TryGetLNum((int)paramIndex, out value), // lnum
                ExpressionType.GlobalNumber => TryGetGNumDefault(paramIndex, out value), // gnum
                _ => false, // gstr, lstr
            };
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
                        var resolvedStr1 = Evaluate(strval1, context);
                        var resolvedStr2 = Evaluate(strval2, context);
                        var equals = resolvedStr1.Equals(resolvedStr2);

                        if ((ExpressionType)exprType == ExpressionType.Equal)
                            value = equals ? 1u : 0u;
                        else
                            value = equals ? 0u : 1u;
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
    public bool TryResolveInt(ref SeStringContext ctx, in ReadOnlySeExpressionSpan expression, out int value)
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
    public bool TryResolveBool(ref SeStringContext ctx, in ReadOnlySeExpressionSpan expression, out bool value)
    {
        if (TryResolveUInt(ref ctx, expression, out var u32))
        {
            value = u32 != 0;
            return true;
        }

        value = false;
        return false;
    }

    public bool ResolveStringExpression(ref SeStringContext ctx, in ReadOnlySeExpressionSpan expression)
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

    private bool TryResolveNoun(ClientLanguage language, ref SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        var eAmountVal = 1;
        var eCaseVal = 1;

        var enu = payload.GetEnumerator();
        if (!enu.MoveNext())
            return false;

        if (!enu.Current.TryGetString(out var eSheetNameStr))
            return false;

        var sheetName = Evaluate(eSheetNameStr, context with { Builder = new() }).ExtractText();

        if (!enu.MoveNext())
            return false;

        if (!TryResolveInt(ref context, enu.Current, out var eArticleTypeVal))
            return false;

        if (!enu.MoveNext())
            return false;

        if (!TryResolveUInt(ref context, enu.Current, out var eRowIdVal))
            return false;

        ResolveSheetRedirect(ref sheetName, ref eRowIdVal);
        if (string.IsNullOrEmpty(sheetName))
            return false;

        if (!enu.MoveNext())
            goto decode;

        if (!TryResolveInt(ref context, enu.Current, out eAmountVal))
            return false;

        if (!enu.MoveNext())
            goto decode;

        if (!TryResolveInt(ref context, enu.Current, out eCaseVal))
            return false;

        if (!enu.MoveNext())
            goto decode;

/* For Chinese texts?
var eUnkInt5 = enu.Current;
if (!TryResolveInt(ref context, eUnkInt5, out eUnkInt5Val))
    return false;
*/

decode:
        context.Builder.Append(_nounProcessor.ProcessRow(sheetName, eRowIdVal, language.ToLumina(), eAmountVal, eArticleTypeVal, eCaseVal - 1));

        return true;
    }

    // "41 0F BF C0 66 0F 6E D0 B8"
    private static uint ConvertRawToMapPos(Map map, short offset, float value)
    {
        var scale = map.SizeFactor / 100.0f;
        return (uint)(10 - (int)(((value + offset) * scale + 1024f) * -0.2f / scale));
    }

    private static uint ConvertRawToMapPosX(Map map, float x)
        => ConvertRawToMapPos(map, map.OffsetX, x);

    private static uint ConvertRawToMapPosY(Map map, float y)
        => ConvertRawToMapPos(map, map.OffsetY, y);

    private static readonly string[] ActStrSheetNames = [
        "Trait",
        "Action",
        "Item",
        "EventItem",
        "EventAction",
        "GeneralAction",
        "BuddyAction",
        "MainCommand",
        "Companion",
        "CraftAction",
        "Action",
        "PetAction",
        "CompanyAction",
        "Mount",
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        "BgcArmyAction",
        "Ornament",
    ];

    private static readonly string[] ObjStrSheetNames = [
        "BNpcName",
        "ENpcResident",
        "Treasure",
        "Aetheryte",
        "GatheringPointName",
        "EObjName",
        "Mount",
        "Companion",
        string.Empty,
        string.Empty,
        "Item",
    ];

    public void ResolveSheetRedirect(ref string sheetName, ref uint rowId, ushort flags = 0xFFFF)
    {
        if (sheetName is "Item" or "ItemHQ" or "ItemMP") // MP means Masterpiece
        {
            if (rowId is > 500_000 and < 1_000_000) // Collectible
            {
                sheetName = "Item";
                rowId -= 500_000;
            }
            else if (rowId - 2_000_000 < _excelService.GetRowCount<EventItem>()) // EventItem
            {
                sheetName = "EventItem";
            }
            else if (rowId >= 1_000_000) // HighQuality
            {
                rowId -= 1_000_000;
            }
            else
            {
                sheetName = "Item";
            }
        }
        else if (sheetName == "ActStr")
        {
            var index = rowId / 1000000;

            if (index >= 0 && index < ActStrSheetNames.Length)
                sheetName = ActStrSheetNames[index];

            rowId %= 1000000;
        }
        else if (sheetName == "ObjStr")
        {
            var index = rowId / 1000000;

            if (index >= 0 && index < ObjStrSheetNames.Length)
                sheetName = ObjStrSheetNames[index];

            rowId %= 1000000;

            if (index == 0) // BNpcName
            {
                if (rowId >= 100000)
                    rowId += 900000;
            }
            else if (index == 1) // ENpcResident
            {
                rowId += 1000000;
            }
            else if (index == 2) // Treasure
            {
                if (_excelService.TryGetRow<Treasure>(rowId, out var treasureRow) && treasureRow.Unknown0.IsEmpty)
                    rowId = 0; // defaulting to "Treasure Coffer"
            }
            else if (index == 3) // Aetheryte
            {
                rowId = _excelService.TryGetRow<Aetheryte>(rowId, out var aetheryteRow) && aetheryteRow.IsAetheryte
                    ? 0u // "Aetheryte"
                    : 1; // "Aethernet Shard"
            }
            else if (index == 5) // EObjName
            {
                rowId += 2000000;
            }
        }
        else if (sheetName == "EObj" && (flags <= 7 || flags == 0xFFFF))
        {
            sheetName = "EObjName";
        }
        else if (sheetName == "Treasure")
        {
            if (_excelService.TryGetRow<Treasure>(rowId, out var treasureRow) && treasureRow.Unknown0.IsEmpty)
                rowId = 0; // defaulting to "Treasure Coffer"
        }
        else if (sheetName == "WeatherPlaceName")
        {
            sheetName = "PlaceName";

            var _rowId = rowId;
            if (_excelService.TryFindRow<WeatherReportReplace>(row => row.PlaceNameSub.RowId == _rowId, out var row))
                rowId = row.PlaceNameParent.RowId;
        }
        else if (sheetName == "InstanceContent" && flags == 3)
        {
            sheetName = "ContentFinderCondition";

            if (_excelService.TryGetRow<InstanceContent>(rowId, out var row))
                rowId = row.Order;
        }
        else if (sheetName == "PartyContent" && flags == 2)
        {
            sheetName = "ContentFinderCondition";

            if (_excelService.TryGetRow<PartyContent>(rowId, out var row))
                rowId = row.ContentFinderCondition.RowId;
        }
        else if (sheetName == "PublicContent" && flags == 3)
        {
            sheetName = "ContentFinderCondition";

            if (_excelService.TryGetRow<PublicContent>(rowId, out var row))
                rowId = row.ContentFinderCondition.RowId;
        }
        else if (sheetName == "AkatsukiNote")
        {
            sheetName = "AkatsukiNoteString";

            if (_excelService.GetSubrowSheet<AkatsukiNote>().TryGetRow(rowId, out var row))
                rowId = (uint)row[0].Unknown2;
        }
    }

    private ref struct IconWrap : IDisposable
    {
        private ref SeStringContext _context;
        private readonly uint _iconClose;

        public IconWrap(ref SeStringContext context, uint iconOpen, uint iconClose)
        {
            _context = ref context;
            _iconClose = iconClose;
            _context.Builder.AppendIcon(iconOpen);
        }

        public void Dispose()
        {
            _context.Builder.AppendIcon(_iconClose);
        }
    }
}
