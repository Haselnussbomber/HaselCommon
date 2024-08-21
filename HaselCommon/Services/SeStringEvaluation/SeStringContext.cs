using Dalamud.Game;
using Lumina.Text;
using Lumina.Text.ReadOnly;

namespace HaselCommon.Services.SeStringEvaluation;

public struct SeStringContext
{
    public SeStringParameter[] LocalParameters;
    public SeStringBuilder Builder;
    public ClientLanguage? Language;
    public bool StripSoftHypen;

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

    /*
    public static implicit operator SeStringContext(SeStringParameter[] localParameters)
        => new() { LocalParameters = localParameters };

    public static implicit operator SeStringContext(uint[] uintParameters)
    {
        var parameters = new SeStringParameter[uintParameters.Length];
        for (var i = 0; i < uintParameters.Length; i++)
            parameters[i] = uintParameters[i];
        return parameters;
    }

    public static implicit operator SeStringContext(string[] stringParameters)
    {
        var parameters = new SeStringParameter[stringParameters.Length];
        for (var i = 0; i < stringParameters.Length; i++)
            parameters[i] = stringParameters[i];
        return parameters;
    }

    public static implicit operator SeStringContext(ReadOnlySeString[] stringParameters)
    {
        var parameters = new SeStringParameter[stringParameters.Length];
        for (var i = 0; i < stringParameters.Length; i++)
            parameters[i] = stringParameters[i];
        return parameters;
    }

    public static implicit operator SeStringContext(SeString[] stringParameters)
    {
        var parameters = new SeStringParameter[stringParameters.Length];
        for (var i = 0; i < stringParameters.Length; i++)
            parameters[i] = stringParameters[i];
        return parameters;
    }
    */
}
