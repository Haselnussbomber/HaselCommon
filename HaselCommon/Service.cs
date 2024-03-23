using System.Collections.Concurrent;
using System.Reflection;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using HaselCommon.Extensions;
using HaselCommon.Services;

namespace HaselCommon;

public static class Service
{
    internal static Assembly PluginAssembly = null!;

    private static readonly ConcurrentDictionary<Type, object> Cache = [];

    public static bool HasService<T>() where T : class
        => Cache.ContainsKey(typeof(T));

    public static bool AddService<T>() where T : class, new()
        => GetService<T>() != null;

    public static bool AddService<T>(T obj) where T : class, new()
    {
        var type = typeof(T);

        if (!Cache.ContainsKey(type) && HasService<IPluginLog>())
            PluginLog.Verbose($"[Service] Adding {type.Name}");

        return Cache.TryAdd(type, obj);
    }

    public static T GetService<T>() where T : class, new()
        => (T)Cache.GetOrAdd(typeof(T), type =>
        {
            if (HasService<IPluginLog>())
                PluginLog.Verbose($"[Service] Creating {type.Name}");

            return new T();
        });

    public static T GetDalamudService<T>()
        => (T)Cache.GetOrAdd(typeof(T), type =>
        {
            if (HasService<IPluginLog>())
                PluginLog.Verbose($"[Service] Injecting {type.Name}");

            return new DalamudServiceWrapper<T>(PluginInterface).Service ??
                throw new Exception($"Could not inject DalamudService {type.Name}");
        });

    #region Dalamud Services
    public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
    public static IAddonEventManager AddonEventManager => GetDalamudService<IAddonEventManager>();
    public static IAddonLifecycle AddonLifecycle => GetDalamudService<IAddonLifecycle>();
    public static IAetheryteList AetheryteList => GetDalamudService<IAetheryteList>();
    public static IBuddyList BuddyList => GetDalamudService<IBuddyList>();
    public static IChatGui ChatGui => GetDalamudService<IChatGui>();
    public static IClientState ClientState => GetDalamudService<IClientState>();
    public static ICommandManager CommandManager => GetDalamudService<ICommandManager>();
    public static ICondition Condition => GetDalamudService<ICondition>();
    public static IContextMenu ContextMenu => GetDalamudService<IContextMenu>();
    public static IDataManager DataManager => GetDalamudService<IDataManager>();
    public static IDtrBar DtrBar => GetDalamudService<IDtrBar>();
    public static IDutyState DutyState => GetDalamudService<IDutyState>();
    public static IFateTable FateTable => GetDalamudService<IFateTable>();
    public static IFlyTextGui FlyTextGui => GetDalamudService<IFlyTextGui>();
    public static IFramework Framework => GetDalamudService<IFramework>();
    public static IGameConfig GameConfig => GetDalamudService<IGameConfig>();
    public static IGameGui GameGui => GetDalamudService<IGameGui>();
    public static IGameInteropProvider GameInteropProvider => GetDalamudService<IGameInteropProvider>();
    public static IGameInventory GameInventory => GetDalamudService<IGameInventory>();
    public static IGameLifecycle GameLifecycle => GetDalamudService<IGameLifecycle>();
    public static IGameNetwork GameNetwork => GetDalamudService<IGameNetwork>();
    public static IGamepadState GamepadState => GetDalamudService<IGamepadState>();
    public static IJobGauges JobGauges => GetDalamudService<IJobGauges>();
    public static IKeyState KeyState => GetDalamudService<IKeyState>();
    public static ILibcFunction LibcFunction => GetDalamudService<ILibcFunction>();
    public static INotificationManager NotificationManager => GetDalamudService<INotificationManager>();
    public static IObjectTable ObjectTable => GetDalamudService<IObjectTable>();
    public static IPartyFinderGui PartyFinderGui => GetDalamudService<IPartyFinderGui>();
    public static IPartyList PartyList => GetDalamudService<IPartyList>();
    public static IPluginLog PluginLog => GetDalamudService<IPluginLog>();
    public static ISigScanner SigScanner => GetDalamudService<ISigScanner>();
    public static ITargetManager TargetManager => GetDalamudService<ITargetManager>();
    public static ITextureProvider TextureProvider => GetDalamudService<ITextureProvider>();
    public static ITextureSubstitutionProvider TextureSubstitutionProvider => GetDalamudService<ITextureSubstitutionProvider>();
    public static ITitleScreenMenu TitleScreenMenu => GetDalamudService<ITitleScreenMenu>();
    public static IToastGui ToastGui => GetDalamudService<IToastGui>();
    #endregion

    #region HaselCommon Services
    internal static StringManager StringManager => GetService<StringManager>();

    public static AddonObserver AddonObserver => GetService<AddonObserver>();
    public static TranslationManager TranslationManager => GetService<TranslationManager>();
    public static TextureManager TextureManager => GetService<TextureManager>();
    public static WindowManager WindowManager => GetService<WindowManager>();
    #endregion

    public static void Initialize(DalamudPluginInterface pluginInterface)
    {
        PluginInterface = pluginInterface;
        PluginAssembly = Assembly.GetCallingAssembly();
        Interop.Resolver.GetInstance.Resolve();
    }

    public static void Dispose()
    {
        Cache.Dispose();
    }
}
