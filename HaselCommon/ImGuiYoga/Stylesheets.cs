using System.Collections.Generic;
using System.IO;
using ExCSS;

namespace HaselCommon.ImGuiYoga;

public class Stylesheets : List<Stylesheet>
{
    public void Add(string css)
    {
        Add(NodeParser.StylesheetParser.Parse(css));
    }

    public void AddEmbeddedResource(string filename)
    {
        using var stream = Service.PluginAssembly.GetManifestResourceStream(filename)
            ?? throw new FileNotFoundException("EmbeddedResource not found", filename);

        Add(NodeParser.StylesheetParser.Parse(stream));
    }
}
