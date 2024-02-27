namespace HaselCommon.Text.Payloads;

public class RawPayload : Payload
{
    public RawPayload() : base()
    {
    }

    public RawPayload(byte[] data) : base()
    {
        Data = data;
    }

    public byte[]? Data { get; set; }

    public override byte[] Encode()
    {
        return Data ?? [];
    }

    public override void Decode(BinaryReader reader)
    {
        Data = reader.ReadBytes((int)reader.BaseStream.Length);
    }

    public override SeString Resolve(List<Expression>? localParameters = null)
        => this;
}
