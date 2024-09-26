using Lumina.Text;
using Lumina.Text.ReadOnly;

namespace HaselCommon.ImGuiYoga;

public class Comment : CharacterData
{
    public Comment() : base()
    {
        Style["display"] = "none";
    }

    public Comment(ReadOnlySeString data) : this()
    {
        Data = data;
    }

    public Comment(string data) : this()
    {
        Data = new SeStringBuilder().Append(data).ToReadOnlySeString(); // TODO: use ReadOnlySeString.FromText() when https://github.com/goatcorp/Dalamud/pull/2033 is merged
    }

    public override string AsHtmlOpenTag => $"<!-- {Data.ExtractText().Replace('\n', ' ')} -->";
}
