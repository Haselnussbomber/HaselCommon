using System.Collections.Generic;
using System.IO;
using HaselCommon.Text.Attributes;
using HaselCommon.Text.Enums;
using HaselCommon.Text.Extensions;
using Lumina.Text.Expressions;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Color)] // n N x
public class ColorPayload : HaselMacroPayload
{
    public BaseExpression? Color { get; set; }

    public override byte[] Encode() => EncodeChunk(Color);

    public override void Decode(BinaryReader reader)
    {
        if (reader.ReadByte() != START_BYTE)
            throw new Exception("Expected START_BYTE");

        if (reader.ReadByte() != (byte)Code)
            throw new Exception($"Expected MacroCode {Code} (0x{(byte)Code:X})");

        reader.ReadIntegerExpression();

        Color = BaseExpression.Parse(reader.BaseStream);

        if (reader.ReadByte() != END_BYTE)
            throw new Exception("Expected END_BYTE");
    }

    public override HaselSeString Resolve(List<HaselSeString>? localParameterData = null)
    {
        if (Color == null)
            return new();

        // stackcolor is used to pop the color from the stack, think of ImGui.PushStyleColor/PopStyleColor
        // as it can't be resolved, we just copy the expression if present
        // the game does this too in MacroDecoder.vf9
        var isStackcolor = (UpdatedExpressionType)Color.ExpressionType == UpdatedExpressionType.StackColor;
        var payload = new ColorPayload
        {
            Color = isStackcolor
                ? Color
                : new IntegerExpression((uint)Color.ResolveNumber(localParameterData))
        };

        return payload;
    }
}
