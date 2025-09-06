using System.IO;
using System.Reflection;
using HaselCommon.Utils.Internal;

namespace HaselCommon.Extensions;

public static class IDalamudPluginInterfaceExtensions
{
    /// <remarks> Use sparingly! </remarks>
    public static T GetService<T>(this IDalamudPluginInterface pluginInterface)
    {
        return new DalamudServiceWrapper<T>(pluginInterface).Service;
    }

    public static void InitializeCustomClientStructs(this IDalamudPluginInterface pluginInterface)
    {
        var sigScanner = pluginInterface.GetService<ISigScanner>();
        var dataManager = pluginInterface.GetService<IDataManager>();

        FFXIVClientStructs.Interop.Generated.Addresses.Register();

        Assembly.GetCallingAssembly()
            .GetType($"{pluginInterface.InternalName}.Addresses")
            ?.GetMethod("Register", BindingFlags.Static | BindingFlags.Public)
            ?.Invoke(null, null);

        Resolver.GetInstance.Setup(
            sigScanner.SearchBase,
            dataManager.GameData.Repositories["ffxiv"].Version,
            new FileInfo(Path.Join(pluginInterface.ConfigDirectory.FullName, "SigCache.json")));

        Resolver.GetInstance.Resolve();
    }
}
