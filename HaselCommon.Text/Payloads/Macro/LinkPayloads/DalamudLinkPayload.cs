namespace HaselCommon.Text.Payloads.Macro;

// does not follow LinkPayload structure, nor does it use StringExpression to encode the plugin name...
public class DalamudLinkPayload : LinkPayload
{
    public DalamudLinkPayload() : base(LinkType.Dalamud)
    {
    }

    public DalamudLinkPayload(string pluginName, uint commandId) : base(LinkType.Dalamud)
    {
        PluginName = pluginName;
        CommandId = commandId;
    }

    public string PluginName { get; set; } = string.Empty;
    public uint CommandId { get; set; } = 0;

    public override byte[] Encode()
    {
        using var stream = new MemoryStream();

        stream.WriteByte(START_BYTE);
        stream.WriteByte((byte)Code);

        var pluginBytes = Encoding.UTF8.GetBytes(PluginName ?? string.Empty);
        stream.WriteByte((byte)pluginBytes.Length);
        stream.Write(pluginBytes);

        IntegerExpression.EncodeStatic(stream, CommandId);

        stream.WriteByte(END_BYTE);

        return stream.ToArray();
    }

    public override void Decode(BinaryReader reader)
    {
        if (reader.ReadByte() != START_BYTE)
            throw new Exception("Expected START_BYTE");

        if (reader.ReadByte() != (byte)Code)
            throw new Exception($"Expected MacroCode {Code} (0x{(byte)Code:X})");

        reader.ReadIntegerExpression();

        Type = Expression.Parse(reader.BaseStream);

        PluginName = Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadByte()));
        CommandId = IntegerExpression.Parse(reader.ReadByte(), reader.BaseStream).Value;

        if (reader.ReadByte() != END_BYTE)
            throw new Exception("Expected END_BYTE");
    }
}
