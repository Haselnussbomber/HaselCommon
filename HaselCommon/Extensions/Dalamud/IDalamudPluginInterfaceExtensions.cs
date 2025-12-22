using System.IO;
using System.Reflection;

namespace HaselCommon.Extensions;

public static class IDalamudPluginInterfaceExtensions
{
    extension(IDalamudPluginInterface pluginInterface)
    {
        public void InitializeCustomClientStructs()
        {
            var sigScanner = pluginInterface.GetRequiredService<ISigScanner>();
            var dataManager = pluginInterface.GetRequiredService<IDataManager>();

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
}
