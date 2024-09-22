using System.Text;
using Lumina.Text.ReadOnly;

namespace HaselCommon.ImGuiYoga;

// https://dom.spec.whatwg.org/#interface-characterdata

public abstract class CharacterData : Node
{
    private ReadOnlySeString data;
    public ReadOnlySeString Data
    {
        get => data;
        set
        {
            if (data != value)
            {
                data = value;
                //IsDirty = true; // TODO: crashes
            }
        }
    }

    protected CharacterData() : base()
    {

    }

    protected CharacterData(ReadOnlySeString data) : base()
    {
        Data = data;
    }

    protected CharacterData(string data) : base()
    {
        Data = new ReadOnlySeString(Encoding.UTF8.GetBytes(data)); // TODO: use ReadOnlySeString.FromText() when https://github.com/goatcorp/Dalamud/pull/2033 is merged
    }
}
