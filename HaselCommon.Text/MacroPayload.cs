using System.Linq;
using System.Reflection;

namespace HaselCommon.Text;

public abstract class MacroPayload : Payload
{
    public MacroCodes Code { get; private set; }

    private readonly PropertyInfo[] PropertyInfos;

    public MacroPayload()
    {
        Code = GetType().GetCustomAttribute<SeStringPayloadAttribute>()?.Code ?? throw new Exception($"{GetType().FullName} is missing SeStringPayloadAttribute");

        // TODO: support arrays? see SwitchPayload
        PropertyInfos = GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(propInfo => propInfo.PropertyType.IsAssignableTo(typeof(Expression)))
            .ToArray();
    }

    public override byte[] Encode()
    {
        using var exprStream = new MemoryStream();

        foreach (var propInfo in PropertyInfos)
        {
            if (propInfo.GetCustomAttribute<TerminatorExpressionAttribute>() != null)
                break; // not needed, omit

            var expr = (Expression?)propInfo.GetValue(this);
            expr?.Encode(exprStream);
        }

        using var stream = new MemoryStream();

        stream.WriteByte(START_BYTE);
        stream.WriteByte((byte)Code);
        exprStream.CopyStreamWithLengthTo(stream);
        stream.WriteByte(END_BYTE);

        return stream.ToArray();
    }

    public override void Decode(BinaryReader reader)
    {
        if (reader.ReadByte() != START_BYTE)
            throw new Exception("Expected START_BYTE");

        if (reader.ReadByte() != (byte)Code)
            throw new Exception($"Expected MacroCode {Code} (0x{(byte)Code:X})");

        reader.ReadIntegerExpression(); // length

        foreach (var propInfo in PropertyInfos)
        {
            if (reader.IsEndOfChunk())
                break;

            propInfo.SetValue(this, Expression.Parse(reader.BaseStream));
        }

        if (reader.ReadByte() != END_BYTE)
            throw new Exception("Expected END_BYTE");
    }

    public override SeString Resolve(List<Expression>? localParameters = null)
        => this; // passthrough

    public override string ToString()
    {
        switch (Code)
        {
            case MacroCodes.NewLine:
                return "\n";
            case MacroCodes.NonBreakingSpace:
                return "\u00A0";
            case MacroCodes.Hyphen:
                return "-";
            case MacroCodes.SoftHyphen:
                return "\u00AD";
        }

        var sb = new StringBuilder();

        sb.Append('<');
        sb.Append(Code.ToString().ToLower());

        if (PropertyInfos.Any())
        {
            sb.Append('(');

            var isFirst = true;
            foreach (var propInfo in PropertyInfos)
            {
                if (propInfo.GetCustomAttribute<TerminatorExpressionAttribute>() != null)
                    break;

                var expr = (Expression?)propInfo.GetValue(this);
                if (expr == null)
                    break;

                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    sb.Append(',');
                }

                sb.Append(expr.ToString());
            }

            sb.Append(')');
        }

        sb.Append('>');

        return sb.ToString();
    }

    protected byte[] EncodeChunk(params Expression?[] expressions)
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
