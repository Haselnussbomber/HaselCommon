namespace HaselCommon.Text.Payloads.Macro.Abstracts;

/// <summary>
/// A very simple payload that is replaced by a character.
/// </summary>
public abstract class CharacterPayload : MacroPayload
{
    public override byte[] Encode()
        => EncodeChunk();

    public override void Decode(BinaryReader reader)
    {
        if (reader.ReadByte() != START_BYTE)
            throw new Exception("Expected START_BYTE");

        if (reader.ReadByte() != (byte)Code)
            throw new Exception($"Expected MacroCode {Code} (0x{(byte)Code:X})");

        if (reader.ReadIntegerExpression() != 0)
            throw new Exception("Expected Length of 0");

        if (reader.ReadByte() != END_BYTE)
            throw new Exception("Expected END_BYTE");
    }
}
