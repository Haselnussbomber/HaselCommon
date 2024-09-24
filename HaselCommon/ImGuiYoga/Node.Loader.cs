using System.IO;
using System.Xml;

namespace HaselCommon.ImGuiYoga;

public partial class Node
{
    public void LoadXml(string xml)
    {
        var doc = new XmlDocument();
        doc.LoadXml(xml);
        LoadXmlDocument(doc);
    }

    public void LoadManifestResource(string path)
    {
        using var stream = Service.PluginAssembly.GetManifestResourceStream(path) ?? throw new FileNotFoundException();
        LoadStream(stream);
    }

    public void LoadStream(Stream stream)
    {
        var doc = new XmlDocument();
        doc.Load(stream);
        LoadXmlDocument(doc);
    }

    public void LoadXmlDocument(XmlDocument doc)
    {
        var document = GetDocument() ?? throw new Exception("Node must be assigned to a document.");

        var xmlNode = doc.FirstChild;
        while (xmlNode != null && xmlNode.NodeType != XmlNodeType.Element)
            xmlNode = xmlNode.NextSibling;

        if (xmlNode == null)
            throw new Exception("Cannot parse an empty document");

        if (xmlNode.NodeType != XmlNodeType.Element)
            throw new Exception("Could not find Element node.");

        if (xmlNode.Attributes != null)
        {
            foreach (XmlAttribute attr in xmlNode.Attributes)
            {
                Attributes[attr.Name] = attr.Value;
            }
        }

        foreach (XmlNode child in xmlNode.ChildNodes)
        {
            var childNode = NodeParser.ParseXmlNode(document, child, this);
            if (childNode != null)
                Add(childNode);
        }
    }
}
