using Lumina.Extensions;

namespace HaselCommon.Text.Payloads;

public class TextPayload : Payload
{
    public string? Text { get; set; }

    public TextPayload(string? text = null)
    {
        Text = text;
    }

    public override byte[] Encode()
        => string.IsNullOrEmpty(Text) ? [] : Encoding.UTF8.GetBytes(Text);

    public override void Decode(BinaryReader reader)
    {
        var textBytes = new List<byte>();

        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            var nextByte = reader.PeekByte();
            if (nextByte == START_BYTE)
                break;

            reader.BaseStream.Position++;
            textBytes.Add(nextByte);
        }

        if (textBytes.Count > 0)
            // TODO: handling of the game's assorted special unicode characters
            Text = Encoding.UTF8.GetString(textBytes.ToArray());
    }

    public override SeString Resolve(List<Expression>? localParameters = null)
        => this;

    public override string ToString()
        => Text ?? string.Empty;
}
