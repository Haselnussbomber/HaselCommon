using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using HaselCommon.Services;

namespace HaselCommon;

public class Service
{
    #region Dalamud Services
    [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] public static IAddonEventManager AddonEventManager { get; private set; } = null!;
    [PluginService] public static IAddonLifecycle AddonLifecycle { get; private set; } = null!;
    [PluginService] public static IAetheryteList AetheryteList { get; private set; } = null!;
    [PluginService] public static IBuddyList BuddyList { get; private set; } = null!;
    [PluginService] public static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] public static IClientState ClientState { get; private set; } = null!;
    [PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] public static ICondition Condition { get; private set; } = null!;
    [PluginService] public static IDataManager DataManager { get; private set; } = null!;
    [PluginService] public static IDtrBar DtrBar { get; private set; } = null!;
    [PluginService] public static IDutyState DutyState { get; private set; } = null!;
    [PluginService] public static IFateTable FateTable { get; private set; } = null!;
    [PluginService] public static IFlyTextGui FlyTextGui { get; private set; } = null!;
    [PluginService] public static IFramework Framework { get; private set; } = null!;
    [PluginService] public static IGameConfig GameConfig { get; private set; } = null!;
    [PluginService] public static IGameGui GameGui { get; private set; } = null!;
    [PluginService] public static IGameInteropProvider GameInteropProvider { get; private set; } = null!;
    [PluginService] public static IGameLifecycle GameLifecycle { get; private set; } = null!;
    [PluginService] public static IGameNetwork GameNetwork { get; private set; } = null!;
    [PluginService] public static IGamepadState GamepadState { get; private set; } = null!;
    [PluginService] public static IJobGauges JobGauges { get; private set; } = null!;
    [PluginService] public static IKeyState KeyState { get; private set; } = null!;
    [PluginService] public static ILibcFunction LibcFunction { get; private set; } = null!;
    [PluginService] public static IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] public static IPartyFinderGui PartyFinderGui { get; private set; } = null!;
    [PluginService] public static IPartyList PartyList { get; private set; } = null!;
    [PluginService] public static IPluginLog PluginLog { get; private set; } = null!;
    [PluginService] public static ISigScanner SigScanner { get; private set; } = null!;
    [PluginService] public static ITargetManager TargetManager { get; private set; } = null!;
    [PluginService] public static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] public static ITextureSubstitutionProvider TextureSubstitutionProvider { get; private set; } = null!;
    [PluginService] public static ITitleScreenMenu TitleScreenMenu { get; private set; } = null!;
    [PluginService] public static IToastGui ToastGui { get; private set; } = null!;
    #endregion

    #region HaselCommon Services
    public static AddonObserver AddonObserver { get; private set; } = null!;
    public static AgentUpdateObserver AgentUpdateObserver { get; private set; } = null!;
    public static TranslationManager TranslationManager { get; private set; } = null!;
    public static StringManager StringManager { get; private set; } = null!;
    public static TextureManager TextureManager { get; private set; } = null!;
    public static WindowManager WindowManager { get; private set; } = null!;
    #endregion

    public static void Initialize(DalamudPluginInterface pluginInterface)
    {
        PluginInterface = pluginInterface;
        pluginInterface.Create<Service>();

        AddonObserver = new();
        AgentUpdateObserver = new();
        TranslationManager = new(pluginInterface, ClientState);
        StringManager = new();
        TextureManager = new(Framework);
        WindowManager = new(pluginInterface);
    }

    public static void Dispose()
    {
        AddonObserver.Dispose();
        AgentUpdateObserver.Dispose();
        TranslationManager.Dispose();
        TextureManager.Dispose();
        WindowManager.Dispose();

        AddonObserver = null!;
        AgentUpdateObserver = null!;
        TranslationManager = null!;
        TextureManager = null!;
        WindowManager = null!;
    }
}
