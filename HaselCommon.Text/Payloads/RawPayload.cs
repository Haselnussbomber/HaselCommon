namespace HaselCommon.Text.Payloads;

public class RawPayload : HaselPayload
{
    public byte[]? Data { get; set; }

    public override byte[] Encode()
        => Data ?? [];

    public override void Decode(BinaryReader reader)
    {
        Data = reader.ReadBytes((int)reader.BaseStream.Length);
    }

    public override HaselSeString Resolve(List<HaselSeString>? localParameterData = null)
        => this;
}
