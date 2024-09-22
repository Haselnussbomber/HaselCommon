using Lumina.Text;
using Lumina.Text.ReadOnly;
using YogaSharp;

namespace HaselCommon.ImGuiYoga;

public class Comment : CharacterData
{
    public Comment() : base()
    {
        Style.Display = YGDisplay.None;
    }

    public Comment(ReadOnlySeString data) : this()
    {
        Data = data;
    }

    public Comment(string data) : this()
    {
        Data = new SeStringBuilder().Append(data).ToReadOnlySeString(); // TODO: use ReadOnlySeString.FromText() when https://github.com/goatcorp/Dalamud/pull/2033 is merged
    }
}
