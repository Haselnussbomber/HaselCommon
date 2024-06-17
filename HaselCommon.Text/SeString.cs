using System.Linq;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using HaselCommon.Services;
using Addon = Lumina.Excel.GeneratedSheets.Addon;

namespace HaselCommon.Text;

public class SeString
{
    public List<Payload> Payloads { get; set; } = [];

    public SeString()
    {
    }

    public SeString(List<Payload> payloads)
    {
        Payloads = payloads;
    }

    public byte[] Encode()
    {
        return Payloads.SelectMany(payload => payload.Encode()).ToArray();
    }

    public SeString Resolve(List<Expression>? localParameters = null)
    {
        return new(Payloads.SelectMany(payload => payload.Resolve(localParameters).Payloads).ToList());
    }

    public override string ToString()
    {
        return Payloads.Aggregate(new StringBuilder(), (sb, tp) => sb.Append(tp.ToString()), sb => sb.ToString());
    }

    public static unsafe SeString FromAddon(uint addonId)
    {
        var row = GetRow<Addon>(addonId, uint.MaxValue, Service.Get<TranslationManager>().ClientLanguage);
        return row == null
            ? new()
            : Parse(row.Text.RawData);
    }

    public static unsafe SeString FromMacro(string macro)
    {
        var input = Utf8String.FromString(macro);
        var output = Utf8String.CreateEmpty();
        RaptureTextModule.Instance()->TextModule.EncodeString(output, input);
        var str = Parse(output->StringPtr, output->Length);
        output->Dtor(true);
        input->Dtor(true);
        return str;
    }

    public static unsafe SeString Parse(ReadOnlySpan<byte> data)
    {
        fixed (byte* ptr = data)
        {
            return Parse(ptr, data.Length);
        }
    }

    public static unsafe SeString Parse(byte* ptr, int len)
    {
        var str = new SeString();

        if (ptr == null)
            return str;

        using var stream = new UnmanagedMemoryStream(ptr, len);
        using var reader = new BinaryReader(stream);

        while (stream.Position < len)
            str.Payloads.Add(Payload.From(reader));

        return str;
    }

    public static implicit operator SeString(Payload payload) => new([payload]);
    public static implicit operator SeString(string str) => new([new TextPayload(str)]);
    public static implicit operator SeString(uint value) => value.ToString(); // TODO: encode number properly? used for Resolve([...])
    public static implicit operator SeString(Lumina.Text.SeString str) => Parse(str.RawData);

    public static implicit operator byte[](SeString str) => str.Encode();
}
