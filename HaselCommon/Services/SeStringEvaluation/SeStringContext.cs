using Dalamud.Game;
using Lumina.Text;
using Lumina.Text.ReadOnly;

namespace HaselCommon.Services.SeStringEvaluation;

public struct SeStringContext
{
    public SeStringParameter[] LocalParameters;
    public SeStringBuilder Builder;
    public ClientLanguage? Language;

    public SeStringContext()
    {
        LocalParameters = [];
        Builder = new();
    }

    public bool TryGetLNum(int index, out uint value)
    {
        if (LocalParameters.Length > index && LocalParameters[index] is SeStringParameter { } val)
        {
            value = val.UIntValue;
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetLStr(int index, out ReadOnlySeString value)
    {
        if (LocalParameters.Length > index && LocalParameters[index] is SeStringParameter { } val)
        {
            value = val.StringValue;
            return true;
        }

        value = new();
        return false;
    }
}
