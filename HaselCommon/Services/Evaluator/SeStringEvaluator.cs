using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Dalamud;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Component.Text;
using HaselCommon.Extensions.Dalamud;
using HaselCommon.Extensions.Strings;
using HaselCommon.Services.Evaluator;
using HaselCommon.Services.Evaluator.Internal;
using HaselCommon.Services.Noun;
using HaselCommon.Services.Noun.Enums;
using Lumina.Data.Structs.Excel;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Text;
using Lumina.Text.Expressions;
using Lumina.Text.Payloads;
using Lumina.Text.ReadOnly;
using Microsoft.Extensions.Logging;
using Serilog;
using ActionKind = HaselCommon.Services.Evaluator.ActionKind;
using AddonSheet = Lumina.Excel.Sheets.Addon;

namespace HaselCommon.Services;

/// <summary> Evaluator for SeStrings. </summary>
[RegisterSingleton, AutoConstruct]
public partial class SeStringEvaluator
{
    private readonly ILogger<SeStringEvaluator> _logger;
    private readonly IDataManager _dataManager;
    private readonly ExcelService _excelService;
    private readonly LanguageProvider _languageProvider;
    private readonly NounProcessor _nounProcessor;
    private readonly IGameConfig _gameConfig;
    private readonly SheetRedirectResolver _sheetRedirectResolver;

    private readonly ConcurrentDictionary<StringCacheKey<ActionKind>, string> _actStrCache = [];
    private readonly ConcurrentDictionary<StringCacheKey<ObjectKind>, string> _objStrCache = [];

    /// <inheritdoc/>
    public ReadOnlySeString Evaluate(
        ReadOnlySeString str,
        Span<SeStringParameter> localParameters = default,
        ClientLanguage? language = null)
    {
        return Evaluate(str.AsSpan(), localParameters, language);
    }

    /// <inheritdoc/>
    public ReadOnlySeString Evaluate(
        ReadOnlySeStringSpan str,
        Span<SeStringParameter> localParameters = default,
        ClientLanguage? language = null)
    {
        if (str.IsTextOnly())
            return new(str);

        var lang = language ?? _languageProvider.ClientLanguage;

        // TODO: remove culture info toggling after supporting CultureInfo for SeStringBuilder.Append,
        //       and then remove try...finally block (discard builder from the pool on exception)
        var previousCulture = CultureInfo.CurrentCulture;
        var builder = SeStringBuilder.SharedPool.Get();
        try
        {
            CultureInfo.CurrentCulture = Localization.GetCultureInfoFromLangCode(lang.ToCode());
            return EvaluateAndAppendTo(builder, str, localParameters, lang).ToReadOnlySeString();
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            SeStringBuilder.SharedPool.Return(builder);
        }
    }

    /// <inheritdoc/>
    public ReadOnlySeString EvaluateFromAddon(
        uint addonId,
        Span<SeStringParameter> localParameters = default,
        ClientLanguage? language = null)
    {
        var lang = language ?? _languageProvider.ClientLanguage;

        if (!_excelService.TryGetRow<AddonSheet>(addonId, lang, out var addonRow))
            return default;

        return Evaluate(addonRow.Text.AsSpan(), localParameters, lang);
    }

    /// <inheritdoc/>
    public ReadOnlySeString EvaluateFromLobby(
        uint lobbyId,
        Span<SeStringParameter> localParameters = default,
        ClientLanguage? language = null)
    {
        var lang = language ?? _languageProvider.ClientLanguage;

        if (!_excelService.TryGetRow<Lobby>(lobbyId, lang, out var lobbyRow))
            return default;

        return Evaluate(lobbyRow.Text.AsSpan(), localParameters, lang);
    }

    /// <inheritdoc/>
    public ReadOnlySeString EvaluateFromLogMessage(
        uint logMessageId,
        Span<SeStringParameter> localParameters = default,
        ClientLanguage? language = null)
    {
        var lang = language ?? _languageProvider.ClientLanguage;

        if (!_excelService.TryGetRow<LogMessage>(logMessageId, lang, out var logMessageRow))
            return default;

        return Evaluate(logMessageRow.Text.AsSpan(), localParameters, lang);
    }

    /// <inheritdoc/>
    public string EvaluateActStr(ActionKind actionKind, uint id, ClientLanguage? language = null) =>
        _actStrCache.GetOrAdd(
            new(actionKind, id, language ?? _languageProvider.ClientLanguage),
            static (key, t) => t.EvaluateFromAddon(2026, [key.Kind.GetActStrId(key.Id)], key.Language)
                                .ExtractText()
                                .StripSoftHyphen(),
            this);

    /// <inheritdoc/>
    public string EvaluateObjStr(ObjectKind objectKind, uint id, ClientLanguage? language = null) =>
        _objStrCache.GetOrAdd(
            new(objectKind, id, language ?? _languageProvider.ClientLanguage),
            static (key, t) => t.EvaluateFromAddon(2025, [key.Kind.GetObjStrId(key.Id)], key.Language)
                                .ExtractText()
                                .StripSoftHyphen(),
            this);

    // TODO: move this to MapUtil?
    private static uint ConvertRawToMapPos(Lumina.Excel.Sheets.Map map, short offset, float value)
    {
        var scale = map.SizeFactor / 100.0f;
        return (uint)(10 - (int)(((((value + offset) * scale) + 1024f) * -0.2f) / scale));
    }

    private static uint ConvertRawToMapPosX(Lumina.Excel.Sheets.Map map, float x)
        => ConvertRawToMapPos(map, map.OffsetX, x);

    private static uint ConvertRawToMapPosY(Lumina.Excel.Sheets.Map map, float y)
        => ConvertRawToMapPos(map, map.OffsetY, y);

    private SeStringBuilder EvaluateAndAppendTo(
        SeStringBuilder builder,
        ReadOnlySeStringSpan str,
        Span<SeStringParameter> localParameters,
        ClientLanguage language)
    {
        var context = new SeStringContext(builder, localParameters, language);

        foreach (var payload in str)
        {
            if (!ResolvePayload(in context, payload))
            {
                context.Builder.Append(payload);
            }
        }

        return builder;
    }

    private bool ResolvePayload(in SeStringContext context, ReadOnlySePayloadSpan payload)
    {
        if (payload.Type != ReadOnlySePayloadType.Macro)
            return false;

        // if (context.HandlePayload(payload, in context))
        //    return true;

        switch (payload.MacroCode)
        {
            case MacroCode.SetResetTime:
                return TryResolveSetResetTime(in context, payload);

            case MacroCode.SetTime:
                return TryResolveSetTime(in context, payload);

            case MacroCode.If:
                return TryResolveIf(in context, payload);

            case MacroCode.Switch:
                return TryResolveSwitch(in context, payload);

            case MacroCode.PcName:
                return TryResolvePcName(in context, payload);

            case MacroCode.IfPcGender:
                return TryResolveIfPcGender(in context, payload);

            case MacroCode.IfPcName:
                return TryResolveIfPcName(in context, payload);

            // case MacroCode.Josa:
            // case MacroCode.Josaro:

            case MacroCode.IfSelf:
                return TryResolveIfSelf(in context, payload);

            // case MacroCode.NewLine: // pass through
            // case MacroCode.Wait: // pass through
            // case MacroCode.Icon: // pass through

            case MacroCode.Color:
                return TryResolveColor(in context, payload);

            case MacroCode.EdgeColor:
                return TryResolveEdgeColor(in context, payload);

            case MacroCode.ShadowColor:
                return TryResolveShadowColor(in context, payload);

            // case MacroCode.SoftHyphen: // pass through
            // case MacroCode.Key:
            // case MacroCode.Scale:

            case MacroCode.Bold:
                return TryResolveBold(in context, payload);

            case MacroCode.Italic:
                return TryResolveItalic(in context, payload);

            // case MacroCode.Edge:
            // case MacroCode.Shadow:
            // case MacroCode.NonBreakingSpace: // pass through
            // case MacroCode.Icon2: // pass through
            // case MacroCode.Hyphen: // pass through

            case MacroCode.Num:
                return TryResolveNum(in context, payload);

            case MacroCode.Hex:
                return TryResolveHex(in context, payload);

            case MacroCode.Kilo:
                return TryResolveKilo(in context, payload);

            // case MacroCode.Byte:

            case MacroCode.Sec:
                return TryResolveSec(in context, payload);

            // case MacroCode.Time:

            case MacroCode.Float:
                return TryResolveFloat(in context, payload);

            // case MacroCode.Link: // pass through

            case MacroCode.Sheet:
                return TryResolveSheet(in context, payload);

            case MacroCode.String:
                return TryResolveString(in context, payload);

            case MacroCode.Caps:
                return TryResolveCaps(in context, payload);

            case MacroCode.Head:
                return TryResolveHead(in context, payload);

            case MacroCode.Split:
                return TryResolveSplit(in context, payload);

            case MacroCode.HeadAll:
                return TryResolveHeadAll(in context, payload);

            case MacroCode.Fixed:
                return TryResolveFixed(in context, payload);

            case MacroCode.Lower:
                return TryResolveLower(in context, payload);

            case MacroCode.JaNoun:
                return TryResolveNoun(ClientLanguage.Japanese, in context, payload);

            case MacroCode.EnNoun:
                return TryResolveNoun(ClientLanguage.English, in context, payload);

            case MacroCode.DeNoun:
                return TryResolveNoun(ClientLanguage.German, in context, payload);

            case MacroCode.FrNoun:
                return TryResolveNoun(ClientLanguage.French, in context, payload);

            // case MacroCode.ChNoun:

            case MacroCode.LowerHead:
                return TryResolveLowerHead(in context, payload);

            case MacroCode.ColorType:
                return TryResolveColorType(in context, payload);

            case MacroCode.EdgeColorType:
                return TryResolveEdgeColorType(in context, payload);

            // case MacroCode.Ruby:

            case MacroCode.Digit:
                return TryResolveDigit(in context, payload);

            case MacroCode.Ordinal:
                return TryResolveOrdinal(in context, payload);

            // case MacroCode.Sound: // pass through

            case MacroCode.LevelPos:
                return TryResolveLevelPos(in context, payload);

            default:
                return false;
        }
    }

    private unsafe bool TryResolveSetResetTime(in SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        DateTime date;

        if (payload.TryGetExpression(out var eHour, out var eWeekday)
            && TryResolveInt(in context, eHour, out var eHourVal)
            && TryResolveInt(in context, eWeekday, out var eWeekdayVal))
        {
            var t = DateTime.UtcNow.AddDays(((eWeekdayVal - (int)DateTime.UtcNow.DayOfWeek) + 7) % 7);
            date = new DateTime(t.Year, t.Month, t.Day, eHourVal, 0, 0, DateTimeKind.Utc).ToLocalTime();
        }
        else if (payload.TryGetExpression(out eHour)
                 && TryResolveInt(in context, eHour, out eHourVal))
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

    private unsafe bool TryResolveSetTime(in SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eTime) || !TryResolveUInt(in context, eTime, out var eTimeVal))
            return false;

        var date = DateTimeOffset.FromUnixTimeSeconds(eTimeVal).LocalDateTime;
        MacroDecoder.GetMacroTime()->SetTime(date);

        return true;
    }

    private bool TryResolveIf(in SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        return
            payload.TryGetExpression(out var eCond, out var eTrue, out var eFalse)
            && ResolveStringExpression(
                context,
                TryResolveBool(in context, eCond, out var eCondVal) && eCondVal
                    ? eTrue
                    : eFalse);
    }

    private bool TryResolveSwitch(in SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        var cond = -1;
        foreach (var e in payload)
        {
            switch (cond)
            {
                case -1:
                    cond = TryResolveUInt(in context, e, out var eVal) ? (int)eVal : 0;
                    break;
                case > 1:
                    cond--;
                    break;
                default:
                    return ResolveStringExpression(in context, e);
            }
        }

        return false;
    }

    private unsafe bool TryResolvePcName(in SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eEntityId))
            return false;

        if (!TryResolveUInt(in context, eEntityId, out var entityId))
            return false;

        // TODO: handle LogNameType

        NameCache.CharacterInfo characterInfo = default;
        if (NameCache.Instance()->TryGetCharacterInfoByEntityId(entityId, &characterInfo))
        {
            context.Builder.Append((ReadOnlySeStringSpan)characterInfo.Name.AsSpan());

            if (characterInfo.HomeWorldId != AgentLobby.Instance()->LobbyData.HomeWorldId &&
                WorldHelper.Instance()->AllWorlds.TryGetValue((ushort)characterInfo.HomeWorldId, out var world, false))
            {
                context.Builder.AppendIcon(88);

                if (_gameConfig.UiConfig.TryGetUInt("LogCrossWorldName", out var logCrossWorldName) &&
                    logCrossWorldName == 1)
                {
                    context.Builder.Append((ReadOnlySeStringSpan)world.Name);
                }
            }

            return true;
        }

        // TODO: lookup via InstanceContentCrystallineConflictDirector
        // TODO: lookup via MJIManager

        return false;
    }

    private unsafe bool TryResolveIfPcGender(in SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eEntityId, out var eMale, out var eFemale))
            return false;

        if (!TryResolveUInt(in context, eEntityId, out var entityId))
            return false;

        NameCache.CharacterInfo characterInfo = default;
        if (NameCache.Instance()->TryGetCharacterInfoByEntityId(entityId, &characterInfo))
            return ResolveStringExpression(in context, characterInfo.Sex == 0 ? eMale : eFemale);

        // TODO: lookup via InstanceContentCrystallineConflictDirector

        return false;
    }

    private unsafe bool TryResolveIfPcName(in SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eEntityId, out var eName, out var eTrue, out var eFalse))
            return false;

        if (!TryResolveUInt(in context, eEntityId, out var entityId) || !eName.TryGetString(out var name))
            return false;

        name = Evaluate(name, context.LocalParameters, context.Language).AsSpan();

        NameCache.CharacterInfo characterInfo = default;
        return NameCache.Instance()->TryGetCharacterInfoByEntityId(entityId, &characterInfo) &&
               ResolveStringExpression(
                   context,
                   name.Equals(characterInfo.Name.AsSpan())
                       ? eTrue
                       : eFalse);
    }

    private unsafe bool TryResolveIfSelf(in SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eEntityId, out var eTrue, out var eFalse))
            return false;

        if (!TryResolveUInt(in context, eEntityId, out var entityId))
            return false;

        // the game uses LocalPlayer here, but using PlayerState seems more safe.
        return ResolveStringExpression(in context, PlayerState.Instance()->EntityId == entityId ? eTrue : eFalse);
    }

    private bool TryResolveColor(in SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eColor))
            return false;

        if (eColor.TryGetPlaceholderExpression(out var ph) && ph == (int)ExpressionType.StackColor)
            context.Builder.PopColor();
        else if (TryResolveUInt(in context, eColor, out var eColorVal))
            context.Builder.PushColorBgra(eColorVal);

        return true;
    }

    private bool TryResolveEdgeColor(in SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eColor))
            return false;

        if (eColor.TryGetPlaceholderExpression(out var ph) && ph == (int)ExpressionType.StackColor)
            context.Builder.PopEdgeColor();
        else if (TryResolveUInt(in context, eColor, out var eColorVal))
            context.Builder.PushEdgeColorBgra(eColorVal);

        return true;
    }

    private bool TryResolveShadowColor(in SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eColor))
            return false;

        if (eColor.TryGetPlaceholderExpression(out var ph) && ph == (int)ExpressionType.StackColor)
            context.Builder.PopShadowColor();
        else if (TryResolveUInt(in context, eColor, out var eColorVal))
            context.Builder.PushShadowColorBgra(eColorVal);

        return true;
    }

    private bool TryResolveBold(in SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eEnable) ||
            !TryResolveBool(in context, eEnable, out var eEnableVal))
        {
            return false;
        }

        context.Builder.AppendSetBold(eEnableVal);

        return true;
    }

    private bool TryResolveItalic(in SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eEnable) ||
            !TryResolveBool(in context, eEnable, out var eEnableVal))
        {
            return false;
        }

        context.Builder.AppendSetItalic(eEnableVal);

        return true;
    }

    private bool TryResolveNum(in SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eInt) || !TryResolveInt(in context, eInt, out var eIntVal))
        {
            context.Builder.Append('0');
            return true;
        }

        context.Builder.Append(eIntVal.ToString());

        return true;
    }

    private bool TryResolveHex(in SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eUInt) || !TryResolveUInt(in context, eUInt, out var eUIntVal))
        {
            // TODO: throw?
            // ERROR: mismatch parameter type ('' is not numeric)
            return false;
        }

        context.Builder.Append("0x{0:X08}".Format(eUIntVal));

        return true;
    }

    private bool TryResolveKilo(in SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eInt, out var eSep) ||
            !TryResolveInt(in context, eInt, out var eIntVal))
        {
            context.Builder.Append('0');
            return true;
        }

        if (eIntVal == int.MinValue)
        {
            // -2147483648
            context.Builder.Append("-2"u8);
            ResolveStringExpression(in context, eSep);
            context.Builder.Append("147"u8);
            ResolveStringExpression(in context, eSep);
            context.Builder.Append("483"u8);
            ResolveStringExpression(in context, eSep);
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
            var digit = (eIntVal / i) % 10;
            switch (anyDigitPrinted)
            {
                case false when digit == 0:
                    continue;
                case true when i % 3 == 0:
                    ResolveStringExpression(in context, eSep);
                    break;
            }

            anyDigitPrinted = true;
            context.Builder.Append((char)('0' + digit));
        }

        return true;
    }

    private bool TryResolveSec(in SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eInt) || !TryResolveUInt(in context, eInt, out var eIntVal))
        {
            // TODO: throw?
            // ERROR: mismatch parameter type ('' is not numeric)
            return false;
        }

        context.Builder.Append("{0:00}".Format(eIntVal));
        return true;
    }

    private bool TryResolveFloat(in SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eValue, out var eRadix, out var eSeparator)
            || !TryResolveInt(in context, eValue, out var eValueVal)
            || !TryResolveInt(in context, eRadix, out var eRadixVal))
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
        ResolveStringExpression(in context, eSeparator);

        // brain fried code
        Span<byte> fractionalDigits = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
        var pos = fractionalDigits.Length - 1;
        for (var r = eRadixVal; r > 1; r /= 10)
        {
            fractionalDigits[pos--] = (byte)('0' + (fractionalPart % 10));
            fractionalPart /= 10;
        }

        context.Builder.Append(fractionalDigits[(pos + 1)..]);

        return true;
    }

    private bool TryResolveSheet(in SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        var enu = payload.GetEnumerator();

        if (!enu.MoveNext() || !enu.Current.TryGetString(out var eSheetNameStr))
            return false;

        if (!enu.MoveNext() || !TryResolveUInt(in context, enu.Current, out var eRowIdValue))
            return false;

        if (!enu.MoveNext() || !TryResolveUInt(in context, enu.Current, out var eColIndexValue))
            return false;

        var eColParamValue = 0u;
        if (enu.MoveNext())
            TryResolveUInt(in context, enu.Current, out eColParamValue);

        var resolvedSheetName = Evaluate(eSheetNameStr, context.LocalParameters, context.Language).ExtractText();

        _sheetRedirectResolver.Resolve(ref resolvedSheetName, ref eRowIdValue, ref eColIndexValue);

        if (string.IsNullOrEmpty(resolvedSheetName))
            return false;

        if (!_dataManager.Excel.SheetNames.Contains(resolvedSheetName))
            return false;

        if (!_excelService.TryGetRow<RawRow>(resolvedSheetName, eRowIdValue, context.Language, out var row))
            return false;

        if (eColIndexValue >= row.Columns.Count)
            return false;

        var column = row.Columns[(int)eColIndexValue];
        switch (column.Type)
        {
            case ExcelColumnDataType.String:
                context.Builder.Append(Evaluate(row.ReadString(column.Offset), [eColParamValue], context.Language));
                return true;
            case ExcelColumnDataType.Bool:
                context.Builder.Append((row.ReadBool(column.Offset) ? 1u : 0).ToString("D", CultureInfo.InvariantCulture));
                return true;
            case ExcelColumnDataType.Int8:
                context.Builder.Append(row.ReadInt8(column.Offset).ToString("D", CultureInfo.InvariantCulture));
                return true;
            case ExcelColumnDataType.UInt8:
                context.Builder.Append(row.ReadUInt8(column.Offset).ToString("D", CultureInfo.InvariantCulture));
                return true;
            case ExcelColumnDataType.Int16:
                context.Builder.Append(row.ReadInt16(column.Offset).ToString("D", CultureInfo.InvariantCulture));
                return true;
            case ExcelColumnDataType.UInt16:
                context.Builder.Append(row.ReadUInt16(column.Offset).ToString("D", CultureInfo.InvariantCulture));
                return true;
            case ExcelColumnDataType.Int32:
                context.Builder.Append(row.ReadInt32(column.Offset).ToString("D", CultureInfo.InvariantCulture));
                return true;
            case ExcelColumnDataType.UInt32:
                context.Builder.Append(row.ReadUInt32(column.Offset).ToString("D", CultureInfo.InvariantCulture));
                return true;
            case ExcelColumnDataType.Float32:
                context.Builder.Append(row.ReadFloat32(column.Offset).ToString("D", CultureInfo.InvariantCulture));
                return true;
            case ExcelColumnDataType.Int64:
                context.Builder.Append(row.ReadInt64(column.Offset).ToString("D", CultureInfo.InvariantCulture));
                return true;
            case ExcelColumnDataType.UInt64:
                context.Builder.Append(row.ReadUInt64(column.Offset).ToString("D", CultureInfo.InvariantCulture));
                return true;
            case ExcelColumnDataType.PackedBool0:
                context.Builder.Append((row.ReadPackedBool(column.Offset, 0) ? 1u : 0).ToString("D", CultureInfo.InvariantCulture));
                return true;
            case ExcelColumnDataType.PackedBool1:
                context.Builder.Append((row.ReadPackedBool(column.Offset, 1) ? 1u : 0).ToString("D", CultureInfo.InvariantCulture));
                return true;
            case ExcelColumnDataType.PackedBool2:
                context.Builder.Append((row.ReadPackedBool(column.Offset, 2) ? 1u : 0).ToString("D", CultureInfo.InvariantCulture));
                return true;
            case ExcelColumnDataType.PackedBool3:
                context.Builder.Append((row.ReadPackedBool(column.Offset, 3) ? 1u : 0).ToString("D", CultureInfo.InvariantCulture));
                return true;
            case ExcelColumnDataType.PackedBool4:
                context.Builder.Append((row.ReadPackedBool(column.Offset, 4) ? 1u : 0).ToString("D", CultureInfo.InvariantCulture));
                return true;
            case ExcelColumnDataType.PackedBool5:
                context.Builder.Append((row.ReadPackedBool(column.Offset, 5) ? 1u : 0).ToString("D", CultureInfo.InvariantCulture));
                return true;
            case ExcelColumnDataType.PackedBool6:
                context.Builder.Append((row.ReadPackedBool(column.Offset, 6) ? 1u : 0).ToString("D", CultureInfo.InvariantCulture));
                return true;
            case ExcelColumnDataType.PackedBool7:
                context.Builder.Append((row.ReadPackedBool(column.Offset, 7) ? 1u : 0).ToString("D", CultureInfo.InvariantCulture));
                return true;
            default:
                return false;
        }
    }

    private bool TryResolveString(in SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        return payload.TryGetExpression(out var eStr) && ResolveStringExpression(in context, eStr);
    }

    private bool TryResolveCaps(in SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eStr))
            return false;

        var builder = SeStringBuilder.SharedPool.Get();

        try
        {
            var headContext = new SeStringContext(builder, context.LocalParameters, context.Language);

            if (!ResolveStringExpression(headContext, eStr))
                return false;

            var str = builder.ToReadOnlySeString();
            var pIdx = 0;

            foreach (var p in str)
            {
                pIdx++;

                if (p.Type == ReadOnlySePayloadType.Invalid)
                    continue;

                if (pIdx == 1 && p.Type == ReadOnlySePayloadType.Text)
                {
                    context.Builder.Append(Encoding.UTF8.GetString(p.Body.ToArray()).ToUpper(context.CultureInfo));
                    continue;
                }

                context.Builder.Append(p);
            }

            return true;
        }
        finally
        {
            SeStringBuilder.SharedPool.Return(builder);
        }
    }

    private bool TryResolveHead(in SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eStr))
            return false;

        var builder = SeStringBuilder.SharedPool.Get();

        try
        {
            var headContext = new SeStringContext(builder, context.LocalParameters, context.Language);

            if (!ResolveStringExpression(headContext, eStr))
                return false;

            var str = builder.ToReadOnlySeString();
            var pIdx = 0;

            foreach (var p in str)
            {
                pIdx++;

                if (p.Type == ReadOnlySePayloadType.Invalid)
                    continue;

                if (pIdx == 1 && p.Type == ReadOnlySePayloadType.Text)
                {
                    context.Builder.Append(Encoding.UTF8.GetString(p.Body.Span).FirstCharToUpper(context.CultureInfo));
                    continue;
                }

                context.Builder.Append(p);
            }

            return true;
        }
        finally
        {
            SeStringBuilder.SharedPool.Return(builder);
        }
    }

    private bool TryResolveSplit(in SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eText, out var eSeparator, out var eIndex))
            return false;

        if (!eSeparator.TryGetString(out var eSeparatorVal) || !eIndex.TryGetUInt(out var eIndexVal) || eIndexVal <= 0)
            return false;

        var builder = SeStringBuilder.SharedPool.Get();

        try
        {
            var headContext = new SeStringContext(builder, context.LocalParameters, context.Language);

            if (!ResolveStringExpression(headContext, eText))
                return false;

            var separator = eSeparatorVal.ExtractText();
            if (separator.Length < 1)
                return false;

            var splitted = builder.ToReadOnlySeString().ExtractText().Split(separator[0]);
            if (eIndexVal <= splitted.Length)
            {
                context.Builder.Append(splitted[eIndexVal - 1]);
                return true;
            }

            return false;
        }
        finally
        {
            SeStringBuilder.SharedPool.Return(builder);
        }
    }

    private bool TryResolveHeadAll(in SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eStr))
            return false;

        var builder = SeStringBuilder.SharedPool.Get();

        try
        {
            var headContext = new SeStringContext(builder, context.LocalParameters, context.Language);

            if (!ResolveStringExpression(headContext, eStr))
                return false;

            var str = builder.ToReadOnlySeString();

            foreach (var p in str)
            {
                if (p.Type == ReadOnlySePayloadType.Invalid)
                    continue;

                if (p.Type == ReadOnlySePayloadType.Text)
                {
                    context.Builder.Append(
                        context.CultureInfo.TextInfo.ToTitleCase(Encoding.UTF8.GetString(p.Body.Span)));

                    continue;
                }

                context.Builder.Append(p);
            }

            return true;
        }
        finally
        {
            SeStringBuilder.SharedPool.Return(builder);
        }
    }

    private bool TryResolveFixed(in SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        // This is handled by the second function in Client::UI::Misc::PronounModule_ProcessString

        var enu = payload.GetEnumerator();

        if (!enu.MoveNext() || !TryResolveInt(in context, enu.Current, out var e0Val))
            return false;

        if (!enu.MoveNext() || !TryResolveInt(in context, enu.Current, out var e1Val))
            return false;

        return e0Val switch
        {
            100 or 200 => e1Val switch
            {
                1 => TryResolveFixedPlayerLink(in context, ref enu),
                2 => TryResolveFixedClassJobLevel(in context, ref enu),
                3 => TryResolveFixedMapLink(in context, ref enu),
                4 => TryResolveFixedItemLink(in context, ref enu),
                5 => TryResolveFixedChatSoundEffect(in context, ref enu),
                6 => TryResolveFixedObjStr(in context, ref enu),
                7 => TryResolveFixedString(in context, ref enu),
                8 => TryResolveFixedTimeRemaining(in context, ref enu),
                // Reads a uint and saves it to PronounModule+0x3AC
                // TODO: handle this? looks like it's for the mentor/beginner icon of the player link in novice network
                // see "FF 50 50 8B B0"
                9 => true,
                10 => TryResolveFixedStatusLink(in context, ref enu),
                11 => TryResolveFixedPartyFinderLink(in context, ref enu),
                12 => TryResolveFixedQuestLink(in context, ref enu),
                _ => false,
            },
            _ => TryResolveFixedAutoTranslation(in context, payload, e0Val, e1Val),
        };
    }

    private unsafe bool TryResolveFixedPlayerLink(in SeStringContext context, ref ReadOnlySePayloadSpan.Enumerator enu)
    {
        if (!enu.MoveNext() || !TryResolveUInt(in context, enu.Current, out var worldId))
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

    private bool TryResolveFixedClassJobLevel(in SeStringContext context, ref ReadOnlySePayloadSpan.Enumerator enu)
    {
        if (!enu.MoveNext() || !TryResolveInt(in context, enu.Current, out var classJobId) || classJobId <= 0)
            return false;

        if (!enu.MoveNext() || !TryResolveInt(in context, enu.Current, out var level))
            return false;

        if (!_excelService.TryGetRow<ClassJob>((uint)classJobId, context.Language, out var classJobRow))
            return false;

        context.Builder.Append(classJobRow.Name);

        if (level != 0)
            context.Builder.Append(context.CultureInfo, $"({level:D})");

        return true;
    }

    private bool TryResolveFixedMapLink(in SeStringContext context, ref ReadOnlySePayloadSpan.Enumerator enu)
    {
        if (!enu.MoveNext() || !TryResolveUInt(in context, enu.Current, out var territoryTypeId))
            return false;

        if (!enu.MoveNext() || !TryResolveUInt(in context, enu.Current, out var packedIds))
            return false;

        if (!enu.MoveNext() || !TryResolveInt(in context, enu.Current, out var rawX))
            return false;

        if (!enu.MoveNext() || !TryResolveInt(in context, enu.Current, out var rawY))
            return false;

        if (!enu.MoveNext() || !TryResolveInt(in context, enu.Current, out var rawZ))
            return false;

        if (!enu.MoveNext() || !TryResolveUInt(in context, enu.Current, out var placeNameIdInt))
            return false;

        var instance = packedIds >> 0x10;
        var mapId = packedIds & 0xFF;

        if (_excelService.TryGetRow<TerritoryType>(territoryTypeId, context.Language, out var territoryTypeRow))
        {
            if (!_excelService.TryGetRow<PlaceName>(placeNameIdInt == 0 ? territoryTypeRow.PlaceName.RowId : placeNameIdInt, context.Language, out var placeNameRow))
                return false;

            if (!_excelService.TryGetRow<Lumina.Excel.Sheets.Map>(mapId, context.Language, out var mapRow))
                return false;

            var sb = SeStringBuilder.SharedPool.Get();

            sb.Append(placeNameRow.Name);
            if (instance is > 0 and <= 9)
                sb.Append((char)((char)0xE0B0 + (char)instance));

            var placeNameWithInstance = sb.ToReadOnlySeString();
            SeStringBuilder.SharedPool.Return(sb);

            var mapPosX = ConvertRawToMapPosX(mapRow, rawX / 1000f);
            var mapPosY = ConvertRawToMapPosY(mapRow, rawY / 1000f);

            var linkText = rawZ == -30000
                               ? EvaluateFromAddon(
                                   1635,
                                   [placeNameWithInstance, mapPosX, mapPosY],
                                   context.Language)
                               : EvaluateFromAddon(
                                   1636,
                                   [placeNameWithInstance, mapPosX, mapPosY, rawZ / (rawZ >= 0 ? 10 : -10), rawZ],
                                   context.Language);

            context.Builder.PushLinkMapPosition(territoryTypeId, mapId, rawX, rawY);
            context.Builder.Append(EvaluateFromAddon(371, [linkText], context.Language));
            context.Builder.PopLink();

            return true;
        }

        var rowId = mapId switch
        {
            0 => 875u, // "(No location set for map link)"
            1 => 874u, // "(Map link unavailable in this area)"
            2 => 13743u, // "(Unable to set map link)"
            _ => 0u,
        };
        if (rowId == 0u)
            return false;
        if (_excelService.TryGetRow<AddonSheet>(rowId, context.Language, out var addonRow))
            context.Builder.Append(addonRow.Text);
        return true;
    }

    private bool TryResolveFixedItemLink(in SeStringContext context, ref ReadOnlySePayloadSpan.Enumerator enu)
    {
        if (!enu.MoveNext() || !TryResolveUInt(in context, enu.Current, out var itemId))
            return false;

        if (!enu.MoveNext() || !TryResolveUInt(in context, enu.Current, out var rarity))
            return false;

        if (!enu.MoveNext() || !TryResolveInt(in context, enu.Current, out var unk2))
            return false;

        if (!enu.MoveNext() || !TryResolveInt(in context, enu.Current, out var unk3))
            return false;

        if (!enu.MoveNext() || !enu.Current.TryGetString(out var itemName)) // TODO: unescape??
            return false;

        // rarity color start
        context.Builder.Append(EvaluateFromAddon(6, [rarity], context.Language));

        var v2 = (ushort)((unk2 & 0xFF) + (unk3 << 0x10)); // TODO: find out what this does

        context.Builder.PushLink(LinkMacroPayloadType.Item, itemId, rarity, v2);

        // arrow and item name
        context.Builder.Append(EvaluateFromAddon(371, [itemName], context.Language));

        context.Builder.PopLink();
        context.Builder.PopColor();

        return true;
    }

    private bool TryResolveFixedChatSoundEffect(in SeStringContext context, ref ReadOnlySePayloadSpan.Enumerator enu)
    {
        if (!enu.MoveNext() || !TryResolveUInt(in context, enu.Current, out var soundEffectId))
            return false;

        context.Builder.Append($"<se.{soundEffectId + 1}>");

        // the game would play it here

        return true;
    }

    private bool TryResolveFixedObjStr(in SeStringContext context, ref ReadOnlySePayloadSpan.Enumerator enu)
    {
        if (!enu.MoveNext() || !TryResolveUInt(in context, enu.Current, out var objStrId))
            return false;

        context.Builder.Append(EvaluateFromAddon(2025, [objStrId], context.Language));

        return true;
    }

    private bool TryResolveFixedString(in SeStringContext context, ref ReadOnlySePayloadSpan.Enumerator enu)
    {
        if (!enu.MoveNext() || !enu.Current.TryGetString(out var text))
            return false;

        // formats it through vsprintf using "%s"??
        context.Builder.Append(text.ExtractText());

        return true;
    }

    private bool TryResolveFixedTimeRemaining(in SeStringContext context, ref ReadOnlySePayloadSpan.Enumerator enu)
    {
        if (!enu.MoveNext() || !TryResolveUInt(in context, enu.Current, out var seconds))
            return false;

        if (seconds != 0)
        {
            context.Builder.Append(EvaluateFromAddon(33, [seconds / 60, seconds % 60], context.Language));
        }
        else
        {
            if (_excelService.TryGetRow<AddonSheet>(48, context.Language, out var addonRow))
                context.Builder.Append(addonRow.Text);
        }

        return true;
    }

    private bool TryResolveFixedStatusLink(in SeStringContext context, ref ReadOnlySePayloadSpan.Enumerator enu)
    {
        if (!enu.MoveNext() || !TryResolveUInt(in context, enu.Current, out var statusId))
            return false;

        if (!enu.MoveNext() || !TryResolveBool(in context, enu.Current, out var hasOverride))
            return false;

        if (!_excelService.TryGetRow<Lumina.Excel.Sheets.Status>(statusId, context.Language, out var statusRow))
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

        switch (statusRow.StatusCategory)
        {
            case 1:
                sb.Append(EvaluateFromAddon(376, default, context.Language));
                break;

            case 2:
                sb.Append(EvaluateFromAddon(377, default, context.Language));
                break;
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

        context.Builder.Append(EvaluateFromAddon(371, [linkText], context.Language));

        context.Builder.PopLink();

        return true;
    }

    private bool TryResolveFixedPartyFinderLink(in SeStringContext context, ref ReadOnlySePayloadSpan.Enumerator enu)
    {
        if (!enu.MoveNext() || !TryResolveUInt(in context, enu.Current, out var listingId))
            return false;

        if (!enu.MoveNext() || !TryResolveUInt(in context, enu.Current, out var unk1))
            return false;

        if (!enu.MoveNext() || !TryResolveUInt(in context, enu.Current, out var worldId))
            return false;

        if (!enu.MoveNext() || !TryResolveInt(
                context,
                enu.Current,
                out var crossWorldFlag)) // 0 = cross world, 1 = not cross world
        {
            return false;
        }

        if (!enu.MoveNext() || !enu.Current.TryGetString(out var playerName))
            return false;

        context.Builder
               .BeginMacro(MacroCode.Link)
               .AppendUIntExpression((uint)LinkMacroPayloadType.PartyFinder)
               .AppendUIntExpression(listingId)
               .AppendUIntExpression(unk1)
               .AppendUIntExpression((uint)(crossWorldFlag << 0x10) + worldId)
               .EndMacro();

        context.Builder.Append(
            EvaluateFromAddon(
                371,
                [EvaluateFromAddon(2265, [playerName, crossWorldFlag], context.Language)],
                context.Language));

        context.Builder.PopLink();

        return true;
    }

    private bool TryResolveFixedQuestLink(in SeStringContext context, ref ReadOnlySePayloadSpan.Enumerator enu)
    {
        if (!enu.MoveNext() || !TryResolveUInt(in context, enu.Current, out var questId))
            return false;

        if (!enu.MoveNext() || !enu.MoveNext() || !enu.MoveNext()) // unused
            return false;

        if (!enu.MoveNext() || !enu.Current.TryGetString(out var questName))
            return false;

        /* TODO: hide incomplete, repeatable special event quest names
        if (!QuestManager.IsQuestComplete(questId) && !QuestManager.Instance()->IsQuestAccepted(questId))
        {
            var questRecompleteManager = QuestRecompleteManager.Instance();
            if (questRecompleteManager == null || !questRecompleteManager->"E8 ?? ?? ?? ?? 0F B6 57 FF"(questId)) {
                if (_excelService.TryGetRow<AddonSheet>(5497, context.Language, out var addonRow))
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

        context.Builder.Append(EvaluateFromAddon(371, [questName], context.Language));

        context.Builder.PopLink();

        return true;
    }

    private bool TryResolveFixedAutoTranslation(
        in SeStringContext context, in ReadOnlySePayloadSpan payload, int e0Val, int e1Val)
    {
        // Auto-Translation / Completion
        var group = (uint)(e0Val + 1);
        var rowId = (uint)e1Val;

        using var icons = new SeStringBuilderIconWrap(context.Builder, 54, 55);

        if (!_excelService.TryFindRow<Completion>(row => row.Group == group && !row.LookupTable.IsEmpty, context.Language, out var groupRow))
            return false;

        var lookupTable = (
                              groupRow.LookupTable.IsTextOnly()
                                  ? groupRow.LookupTable
                                  : Evaluate(
                                      groupRow.LookupTable.AsSpan(),
                                      context.LocalParameters,
                                      context.Language)).ExtractText();

        // Completion sheet
        if (lookupTable.Equals("@"))
        {
            if (_excelService.TryGetRow<Completion>(rowId, context.Language, out var completionRow))
            {
                context.Builder.Append(completionRow.Text);
            }

            return true;
        }

        // CategoryDataCache
        if (lookupTable.Equals("#"))
        {
            // couldn't find any, so we don't handle them :p
            context.Builder.Append(payload);
            return false;
        }

        // All other sheets
        var rangesStart = lookupTable.IndexOf('[');
        // Sheet without ranges
        if (rangesStart == -1)
        {
            if (_excelService.TryGetRow<RawRow>(lookupTable, rowId, context.Language, out var row))
            {
                context.Builder.Append(row.ReadStringColumn(0));
                return true;
            }
        }

        var sheetName = lookupTable[..rangesStart];
        var ranges = lookupTable[(rangesStart + 1)..^1];
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
            context.Builder.Append(_nounProcessor.ProcessNoun(new NounParams()
            {
                Language = ClientLanguage.German,
                SheetName = sheetName,
                RowId = rowId,
                Quantity = 1,
                ArticleType = (int)GermanArticleType.ZeroArticle,
            }));
        }
        else if (_excelService.TryGetRow<RawRow>(sheetName, rowId, context.Language, out var row))
        {
            context.Builder.Append(row.ReadStringColumn(col));
        }

        return true;
    }

    private bool TryResolveLower(in SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eStr))
            return false;

        var builder = SeStringBuilder.SharedPool.Get();

        try
        {
            var headContext = new SeStringContext(builder, context.LocalParameters, context.Language);

            if (!ResolveStringExpression(headContext, eStr))
                return false;

            var str = builder.ToReadOnlySeString();

            foreach (var p in str)
            {
                if (p.Type == ReadOnlySePayloadType.Invalid)
                    continue;

                if (p.Type == ReadOnlySePayloadType.Text)
                {
                    context.Builder.Append(Encoding.UTF8.GetString(p.Body.ToArray()).ToLower(context.CultureInfo));

                    continue;
                }

                context.Builder.Append(p);
            }

            return true;
        }
        finally
        {
            SeStringBuilder.SharedPool.Return(builder);
        }
    }

    private bool TryResolveNoun(ClientLanguage language, in SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        var eAmountVal = 1;
        var eCaseVal = 1;

        var enu = payload.GetEnumerator();

        if (!enu.MoveNext() || !enu.Current.TryGetString(out var eSheetNameStr))
            return false;

        var sheetName = Evaluate(eSheetNameStr, context.LocalParameters, context.Language).ExtractText();

        if (!enu.MoveNext() || !TryResolveInt(in context, enu.Current, out var eArticleTypeVal))
            return false;

        if (!enu.MoveNext() || !TryResolveUInt(in context, enu.Current, out var eRowIdVal))
            return false;

        uint colIndex = ushort.MaxValue;
        var flags = _sheetRedirectResolver.Resolve(ref sheetName, ref eRowIdVal, ref colIndex);

        if (string.IsNullOrEmpty(sheetName))
            return false;

        // optional arguments
        if (enu.MoveNext())
        {
            if (!TryResolveInt(in context, enu.Current, out eAmountVal))
                return false;

            if (enu.MoveNext())
            {
                if (!TryResolveInt(in context, enu.Current, out eCaseVal))
                    return false;

                // For Chinese texts?
                /*
                if (enu.MoveNext())
                {
                    var eUnkInt5 = enu.Current;
                    if (!TryResolveInt(context,eUnkInt5, out eUnkInt5Val))
                        return false;
                }
                */
            }
        }

        context.Builder.Append(
            _nounProcessor.ProcessNoun(new NounParams()
            {
                Language = language,
                SheetName = sheetName,
                RowId = eRowIdVal,
                Quantity = eAmountVal,
                ArticleType = eArticleTypeVal,
                GrammaticalCase = eCaseVal - 1,
                IsActionSheet = flags.HasFlag(SheetRedirectFlags.Action),
            }));

        return true;
    }

    private bool TryResolveLowerHead(in SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eStr))
            return false;

        var builder = SeStringBuilder.SharedPool.Get();

        try
        {
            var headContext = new SeStringContext(builder, context.LocalParameters, context.Language);

            if (!ResolveStringExpression(headContext, eStr))
                return false;

            var str = builder.ToReadOnlySeString();
            var pIdx = 0;

            foreach (var p in str)
            {
                pIdx++;

                if (p.Type == ReadOnlySePayloadType.Invalid)
                    continue;

                if (pIdx == 1 && p.Type == ReadOnlySePayloadType.Text)
                {
                    context.Builder.Append(Encoding.UTF8.GetString(p.Body.Span).FirstCharToLower(context.CultureInfo));
                    continue;
                }

                context.Builder.Append(p);
            }

            return true;
        }
        finally
        {
            SeStringBuilder.SharedPool.Return(builder);
        }
    }

    private bool TryResolveColorType(in SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eColorType) ||
            !TryResolveUInt(in context, eColorType, out var eColorTypeVal))
        {
            return false;
        }

        if (eColorTypeVal == 0)
            context.Builder.PopColor();
        else if (_excelService.TryGetRow<UIColor>(eColorTypeVal, out var row))
            context.Builder.PushColorBgra((row.UIForeground >> 8) | (row.UIForeground << 24));

        return true;
    }

    private bool TryResolveEdgeColorType(in SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eColorType) ||
            !TryResolveUInt(in context, eColorType, out var eColorTypeVal))
        {
            return false;
        }

        if (eColorTypeVal == 0)
            context.Builder.PopEdgeColor();
        else if (_excelService.TryGetRow<UIColor>(eColorTypeVal, out var row))
            context.Builder.PushEdgeColorBgra((row.UIForeground >> 8) | (row.UIForeground << 24));

        return true;
    }

    private bool TryResolveDigit(in SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eValue, out var eTargetLength))
            return false;

        if (!TryResolveInt(in context, eValue, out var eValueVal))
            return false;

        if (!TryResolveInt(in context, eTargetLength, out var eTargetLengthVal))
            return false;

        context.Builder.Append(eValueVal.ToString(new string('0', eTargetLengthVal)));

        return true;
    }

    private bool TryResolveOrdinal(in SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eValue) || !TryResolveUInt(in context, eValue, out var eValueVal))
            return false;

        // TODO: Culture support?
        context.Builder.Append(
            $"{eValueVal}{(eValueVal % 10) switch
            {
                _ when eValueVal is >= 10 and <= 19 => "th",
                1 => "st",
                2 => "nd",
                3 => "rd",
                _ => "th",
            }}");
        return true;
    }

    private bool TryResolveLevelPos(in SeStringContext context, in ReadOnlySePayloadSpan payload)
    {
        if (!payload.TryGetExpression(out var eLevel) || !TryResolveUInt(in context, eLevel, out var eLevelVal))
            return false;

        if (!_excelService.TryGetRow<Level>(eLevelVal, context.Language, out var level) ||
            !level.Map.IsValid)
        {
            return false;
        }

        if (!_excelService.TryGetRow<PlaceName>(level.Map.Value.PlaceName.RowId, context.Language, out var placeName))
            return false;

        var mapPosX = ConvertRawToMapPosX(level.Map.Value, level.X);
        var mapPosY = ConvertRawToMapPosY(level.Map.Value, level.Z); // Z is [sic]

        context.Builder.Append(
            EvaluateFromAddon(
                1637,
                [placeName.Name, mapPosX, mapPosY],
                context.Language));

        return true;
    }

    private unsafe bool TryGetGNumDefault(uint parameterIndex, out uint value)
    {
        value = 0u;

        var rtm = RaptureTextModule.Instance();
        if (rtm is null)
            return false;

        if (!ThreadSafety.IsMainThread)
        {
            Log.Error("Global parameters may only be used from the main thread.");
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
                Log.Error("Requested a number; Utf8String global parameter at {parameterIndex}.", parameterIndex);
                return false;

            case TextParameterType.String:
                Log.Error("Requested a number; string global parameter at {parameterIndex}.", parameterIndex);
                return false;

            case TextParameterType.Uninitialized:
                Log.Error("Requested a number; uninitialized global parameter at {parameterIndex}.", parameterIndex);
                return false;

            default:
                return false;
        }
    }

    private unsafe bool TryProduceGStrDefault(SeStringBuilder builder, ClientLanguage language, uint parameterIndex)
    {
        var rtm = RaptureTextModule.Instance();
        if (rtm is null)
            return false;

        ref var gp = ref rtm->TextModule.MacroDecoder.GlobalParameters;
        if (parameterIndex >= gp.MySize)
            return false;

        if (!ThreadSafety.IsMainThread)
        {
            Log.Error("Global parameters may only be used from the main thread.");
            return false;
        }

        var p = rtm->TextModule.MacroDecoder.GlobalParameters[parameterIndex];
        switch (p.Type)
        {
            case TextParameterType.Integer:
                builder.Append($"{p.IntValue:D}");
                return true;

            case TextParameterType.ReferencedUtf8String:
                EvaluateAndAppendTo(
                    builder,
                    p.ReferencedUtf8StringValue->Utf8String.AsSpan(),
                    null,
                    language);
                return false;

            case TextParameterType.String:
                EvaluateAndAppendTo(builder, p.StringValue.ToReadOnlySeStringSpan(), null, language);
                return false;

            case TextParameterType.Uninitialized:
            default:
                return false;
        }
    }

    private unsafe bool TryResolveUInt(
        in SeStringContext context, in ReadOnlySeExpressionSpan expression, out uint value)
    {
        if (expression.TryGetUInt(out value))
            return true;

        if (expression.TryGetPlaceholderExpression(out var exprType))
        {
            // if (context.TryGetPlaceholderNum(exprType, out value))
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
            if (!TryResolveUInt(in context, operand1, out var paramIndex))
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
                    if (!TryResolveInt(in context, operand1, out var value1)
                        || !TryResolveInt(in context, operand2, out var value2))
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
                    if (TryResolveInt(in context, operand1, out value1) &&
                        TryResolveInt(in context, operand2, out value2))
                    {
                        if ((ExpressionType)exprType == ExpressionType.Equal)
                            value = value1 == value2 ? 1u : 0u;
                        else
                            value = value1 == value2 ? 0u : 1u;
                        return true;
                    }

                    if (operand1.TryGetString(out var strval1) && operand2.TryGetString(out var strval2))
                    {
                        var resolvedStr1 = EvaluateAndAppendTo(
                            SeStringBuilder.SharedPool.Get(),
                            strval1,
                            context.LocalParameters,
                            context.Language);
                        var resolvedStr2 = EvaluateAndAppendTo(
                            SeStringBuilder.SharedPool.Get(),
                            strval2,
                            context.LocalParameters,
                            context.Language);
                        var equals = resolvedStr1.GetViewAsSpan().SequenceEqual(resolvedStr2.GetViewAsSpan());
                        SeStringBuilder.SharedPool.Return(resolvedStr1);
                        SeStringBuilder.SharedPool.Return(resolvedStr2);

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
            var evaluatedStr = Evaluate(str, context.LocalParameters, context.Language);

            foreach (var payload in evaluatedStr)
            {
                if (!payload.TryGetExpression(out var expr))
                    return false;

                return TryResolveUInt(in context, expr, out value);
            }

            return false;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryResolveInt(in SeStringContext context, in ReadOnlySeExpressionSpan expression, out int value)
    {
        if (TryResolveUInt(in context, expression, out var u32))
        {
            value = (int)u32;
            return true;
        }

        value = 0;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryResolveBool(in SeStringContext context, in ReadOnlySeExpressionSpan expression, out bool value)
    {
        if (TryResolveUInt(in context, expression, out var u32))
        {
            value = u32 != 0;
            return true;
        }

        value = false;
        return false;
    }

    private bool ResolveStringExpression(in SeStringContext context, in ReadOnlySeExpressionSpan expression)
    {
        uint u32;

        if (expression.TryGetString(out var innerString))
        {
            context.Builder.Append(Evaluate(innerString, context.LocalParameters, context.Language));
            return true;
        }

        /*
        if (expression.TryGetPlaceholderExpression(out var exprType))
        {
            if (context.TryProducePlaceholder(context,exprType))
                return true;
        }
        */

        if (expression.TryGetParameterExpression(out var exprType, out var operand1))
        {
            if (!TryResolveUInt(in context, operand1, out var paramIndex))
                return false;
            if (paramIndex == 0)
                return false;
            paramIndex--;
            switch ((ExpressionType)exprType)
            {
                case ExpressionType.LocalNumber: // lnum
                    if (!context.TryGetLNum((int)paramIndex, out u32))
                        return false;

                    context.Builder.Append(unchecked((int)u32).ToString());
                    return true;

                case ExpressionType.LocalString: // lstr
                    if (!context.TryGetLStr((int)paramIndex, out var str))
                        return false;

                    context.Builder.Append(str);
                    return true;

                case ExpressionType.GlobalNumber: // gnum
                    if (!TryGetGNumDefault(paramIndex, out u32))
                        return false;

                    context.Builder.Append(unchecked((int)u32).ToString());
                    return true;

                case ExpressionType.GlobalString: // gstr
                    return TryProduceGStrDefault(context.Builder, context.Language, paramIndex);

                default:
                    return false;
            }
        }

        // Handles UInt and Binary expressions
        if (!TryResolveUInt(in context, expression, out u32))
            return false;

        context.Builder.Append(((int)u32).ToString());
        return true;
    }

    private readonly record struct StringCacheKey<TK>(TK Kind, uint Id, ClientLanguage Language)
        where TK : struct, Enum;
}
