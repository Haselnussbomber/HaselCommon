using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.Text.Evaluator;
using Dalamud.Plugin.Services;
using Lumina.Text.ReadOnly;

using CSObjectKind = FFXIVClientStructs.FFXIV.Client.Game.Object.ObjectKind;

namespace HaselCommon.Services;

#pragma warning disable SeStringEvaluator

[RegisterSingleton]
public partial class SeStringEvaluator
{
    private readonly LanguageProvider _languageProvider;
    private readonly ISeStringEvaluator _seStringEvaluator;

    public SeStringEvaluator(LanguageProvider languageProvider, ISeStringEvaluator seStringEvaluator)
    {
        _languageProvider = languageProvider;
        _seStringEvaluator = seStringEvaluator;
    }

    public ReadOnlySeString Evaluate(ReadOnlySeString str, Span<SeStringParameter> localParameters = default, ClientLanguage? language = null)
    {
        return _seStringEvaluator.Evaluate(str, localParameters, language ?? _languageProvider.ClientLanguage);
    }

    public ReadOnlySeString Evaluate(ReadOnlySeStringSpan str, Span<SeStringParameter> localParameters = default, ClientLanguage? language = null)
    {
        return _seStringEvaluator.Evaluate(str, localParameters, language ?? _languageProvider.ClientLanguage);
    }

    public string EvaluateActStr(ActionKind actionKind, uint id, ClientLanguage? language = null)
    {
        return _seStringEvaluator.EvaluateActStr(actionKind, id, language ?? _languageProvider.ClientLanguage);
    }

    public ReadOnlySeString EvaluateFromAddon(uint addonId, Span<SeStringParameter> localParameters = default, ClientLanguage? language = null)
    {
        return _seStringEvaluator.EvaluateFromAddon(addonId, localParameters, language ?? _languageProvider.ClientLanguage);
    }

    public ReadOnlySeString EvaluateFromLobby(uint lobbyId, Span<SeStringParameter> localParameters = default, ClientLanguage? language = null)
    {
        return _seStringEvaluator.EvaluateFromLobby(lobbyId, localParameters, language ?? _languageProvider.ClientLanguage);
    }

    public ReadOnlySeString EvaluateFromLogMessage(uint logMessageId, Span<SeStringParameter> localParameters = default, ClientLanguage? language = null)
    {
        return _seStringEvaluator.EvaluateFromLobby(logMessageId, localParameters, language ?? _languageProvider.ClientLanguage);
    }

    public string EvaluateObjStr(ObjectKind objectKind, uint id, ClientLanguage? language = null)
    {
        return _seStringEvaluator.EvaluateObjStr(objectKind, id, language ?? _languageProvider.ClientLanguage);
    }

    public string EvaluateObjStr(CSObjectKind objectKind, uint id, ClientLanguage? language = null)
    {
        return _seStringEvaluator.EvaluateObjStr((ObjectKind)objectKind, id, language ?? _languageProvider.ClientLanguage);
    }
}
