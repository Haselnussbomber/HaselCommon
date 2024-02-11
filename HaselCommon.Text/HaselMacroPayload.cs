using System.IO;
using System.Linq;
using System.Reflection;
using HaselCommon.Extensions;
using HaselCommon.Text.Attributes;
using HaselCommon.Text.Enums;
using Lumina.Text.Expressions;

namespace HaselCommon.Text;

public abstract class HaselMacroPayload : HaselPayload
{
    public MacroCodes Code { get; private set; }

    public HaselMacroPayload()
    {
        Code = GetType().GetCustomAttribute<SeStringPayloadAttribute>()?.Code ?? throw new Exception($"{GetType().FullName} is missing SeStringPayloadAttribute");
    }

    protected byte[] EncodeChunk(params BaseExpression?[] expressions)
    {
        if (expressions.Length != 0)
        {
            if (expressions.All(expr => expr == null))
                return [];

            var lastNonNull = false;
            for (var i = expressions.Length - 1; i >= 0; i--)
            {
                if (expressions[i] == null)
                {
                    if (lastNonNull)
                        throw new ArgumentException($"Can't encode optional null expression {i} in between non-null expressions");
                }
                else
                {
                    lastNonNull = true;
                }
            }
        }

        using var exprStream = new MemoryStream();

        foreach (var expr in expressions)
            expr?.Encode(exprStream);

        using var stream = new MemoryStream();

        stream.WriteByte(START_BYTE);
        stream.WriteByte((byte)Code);
        exprStream.CopyStreamWithLengthTo(stream);
        stream.WriteByte(END_BYTE);

        return stream.ToArray();
    }
}
