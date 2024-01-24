using System.Collections.Generic;
using System.Linq;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using HaselCommon.Extensions;
using HaselCommon.Services;

namespace HaselCommon;

public class Service
{
    private static readonly HashSet<object> Cache = [];
    private static readonly HashSet<object> DalamudCache = [];

    public static bool HasService<T>() where T : class
        => Cache.OfType<T>().FirstOrDefault() != null || DalamudCache.OfType<T>().FirstOrDefault() != null;

    public static T GetService<T>() where T : class, new()
    {
        lock (Cache)
        {
            var obj = Cache.OfType<T>().FirstOrDefault();
            if (obj == null)
            {
                if (HasService<IPluginLog>())
                    PluginLog.Verbose($"[Service] Creating {typeof(T).Name}");
                Cache.Add(obj = new T());
            }

            return obj;
        }
    }

    public static T GetDalamudService<T>()
    {
        lock (DalamudCache)
        {
            var obj = DalamudCache.OfType<T>().FirstOrDefault();
            if (obj == null)
            {
                if (HasService<IPluginLog>())
                    PluginLog.Verbose($"[Service] Injecting {typeof(T).Name}");

                obj = new DalamudServiceWrapper<T>(PluginInterface).Service;

                if (obj != null)
                    DalamudCache.Add(obj);
                else
                    throw new Exception($"DalamudService of type {typeof(T).Name} is null");
            }

            return obj;
        }
    }

    #region Dalamud Services
    [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] public static IAddonEventManager AddonEventManager => GetDalamudService<IAddonEventManager>();
    [PluginService] public static IAddonLifecycle AddonLifecycle => GetDalamudService<IAddonLifecycle>();
    [PluginService] public static IAetheryteList AetheryteList => GetDalamudService<IAetheryteList>();
    [PluginService] public static IBuddyList BuddyList => GetDalamudService<IBuddyList>();
    [PluginService] public static IChatGui ChatGui => GetDalamudService<IChatGui>();
    [PluginService] public static IClientState ClientState => GetDalamudService<IClientState>();
    [PluginService] public static ICommandManager CommandManager => GetDalamudService<ICommandManager>();
    [PluginService] public static ICondition Condition => GetDalamudService<ICondition>();
    [PluginService] public static IDataManager DataManager => GetDalamudService<IDataManager>();
    [PluginService] public static IDtrBar DtrBar => GetDalamudService<IDtrBar>();
    [PluginService] public static IDutyState DutyState => GetDalamudService<IDutyState>();
    [PluginService] public static IFateTable FateTable => GetDalamudService<IFateTable>();
    [PluginService] public static IFlyTextGui FlyTextGui => GetDalamudService<IFlyTextGui>();
    [PluginService] public static IFramework Framework => GetDalamudService<IFramework>();
    [PluginService] public static IGameConfig GameConfig => GetDalamudService<IGameConfig>();
    [PluginService] public static IGameGui GameGui => GetDalamudService<IGameGui>();
    [PluginService] public static IGameInteropProvider GameInteropProvider => GetDalamudService<IGameInteropProvider>();
    [PluginService] public static IGameInventory GameInventory => GetDalamudService<IGameInventory>();
    [PluginService] public static IGameLifecycle GameLifecycle => GetDalamudService<IGameLifecycle>();
    [PluginService] public static IGameNetwork GameNetwork => GetDalamudService<IGameNetwork>();
    [PluginService] public static IGamepadState GamepadState => GetDalamudService<IGamepadState>();
    [PluginService] public static IJobGauges JobGauges => GetDalamudService<IJobGauges>();
    [PluginService] public static IKeyState KeyState => GetDalamudService<IKeyState>();
    [PluginService] public static ILibcFunction LibcFunction => GetDalamudService<ILibcFunction>();
    [PluginService] public static IObjectTable ObjectTable => GetDalamudService<IObjectTable>();
    [PluginService] public static IPartyFinderGui PartyFinderGui => GetDalamudService<IPartyFinderGui>();
    [PluginService] public static IPartyList PartyList => GetDalamudService<IPartyList>();
    [PluginService] public static IPluginLog PluginLog => GetDalamudService<IPluginLog>();
    [PluginService] public static ISigScanner SigScanner => GetDalamudService<ISigScanner>();
    [PluginService] public static ITargetManager TargetManager => GetDalamudService<ITargetManager>();
    [PluginService] public static ITextureProvider TextureProvider => GetDalamudService<ITextureProvider>();
    [PluginService] public static ITextureSubstitutionProvider TextureSubstitutionProvider => GetDalamudService<ITextureSubstitutionProvider>();
    [PluginService] public static ITitleScreenMenu TitleScreenMenu => GetDalamudService<ITitleScreenMenu>();
    [PluginService] public static IToastGui ToastGui => GetDalamudService<IToastGui>();
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
    }

    public static void Dispose()
    {
        Cache.OfType<IDisposable>().ForEach(disposable => disposable.Dispose());
        Cache.Clear();
        DalamudCache.Clear();
    }
}
