using System.Collections.Generic;
using System.IO;
using HaselCommon.Text.Attributes;
using HaselCommon.Text.Enums;
using HaselCommon.Text.Extensions;
using Lumina.Text.Expressions;

namespace HaselCommon.Text.Payloads.Macro;

[SeStringPayload(MacroCodes.Link)] // n n n n s
public class LinkPayload : HaselMacroPayload
{
    public LinkPayload()
    {
    }

    public LinkPayload(LinkType type, uint arg2 = 0, uint arg3 = 0, uint arg4 = 0, string arg5 = "")
    {
        Type = new IntegerExpression((uint)type);
        Arg2 = new IntegerExpression(arg2);
        Arg3 = new IntegerExpression(arg3);
        Arg4 = new IntegerExpression(arg4);
        Arg5 = new StringExpression(new(arg5));
    }

    public BaseExpression? Type { get; set; }
    public BaseExpression? Arg2 { get; set; }
    public BaseExpression? Arg3 { get; set; }
    public BaseExpression? Arg4 { get; set; }
    public BaseExpression? Arg5 { get; set; }

    public override byte[] Encode() => EncodeChunk(Type, Arg2, Arg3, Arg4, Arg5);

    public override void Decode(BinaryReader reader)
    {
        if (reader.ReadByte() != START_BYTE)
            throw new Exception("Expected START_BYTE");

        if (reader.ReadByte() != (byte)Code)
            throw new Exception($"Expected MacroCode {Code} (0x{(byte)Code:X})");

        reader.ReadIntegerExpression();

        Type = BaseExpression.Parse(reader.BaseStream);

        // not sure why, but sometimes links don't have all expressions set?

        if (!reader.IsEndOfChunk())
        {
            Arg2 = BaseExpression.Parse(reader.BaseStream);

            if (!reader.IsEndOfChunk())
            {
                Arg3 = BaseExpression.Parse(reader.BaseStream);

                if (!reader.IsEndOfChunk())
                {
                    Arg4 = BaseExpression.Parse(reader.BaseStream);

                    if (!reader.IsEndOfChunk())
                    {
                        Arg5 = BaseExpression.Parse(reader.BaseStream);
                    }
                }
            }
        }

        if (reader.ReadByte() != END_BYTE)
            throw new Exception("Expected END_BYTE");
    }

    public override HaselSeString Resolve(List<HaselSeString>? localParameterData = null)
        => this;
}
