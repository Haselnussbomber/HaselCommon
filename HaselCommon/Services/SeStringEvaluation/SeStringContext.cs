using Lumina.Text;
using Lumina.Text.ReadOnly;

namespace HaselCommon.Services.SeStringEvaluation;

public struct SeStringContext(SeStringParameter[]? localParameters = null, SeStringBuilder? builder = null)
{
    public SeStringParameter[] LocalParameters = localParameters ?? [];
    public SeStringBuilder Builder = builder ?? new();

    public bool TryGetLNum(int index, out uint value)
    {
        if (LocalParameters.Length > index && LocalParameters[index] is SeStringParameter { } val)
        {
            value = val.AsUInt();
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetLStr(int index, out ReadOnlySeString value)
    {
        if (LocalParameters.Length > index && LocalParameters[index] is SeStringParameter { } val)
        {
            value = val.AsString();
            return true;
        }

        value = new();
        return false;
    }

    public static implicit operator SeStringContext(SeStringParameter[] localParameters)
        => new(localParameters);

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
}
