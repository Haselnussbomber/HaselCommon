using System.IO;
using System.Reflection;

namespace HaselCommon.Utils;

public class PluginAssembly(Assembly assembly)
{
    public Assembly Assembly => assembly;

    public Stream? GetManifestResourceStream(string name)
    {
        return assembly.GetManifestResourceStream(name);
    }
}
