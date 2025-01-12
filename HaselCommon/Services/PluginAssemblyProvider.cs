using System.Reflection;

namespace HaselCommon.Services;

public class PluginAssemblyProvider(Assembly assembly)
{
    public Assembly Assembly { get; } = assembly;
}
