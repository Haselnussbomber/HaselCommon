using System.Linq;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Addon = Lumina.Excel.GeneratedSheets.Addon;

namespace HaselCommon.Text;

public class HaselSeString
{
    public List<HaselPayload> Payloads { get; set; } = [];

    public HaselSeString()
    {
    }

    public HaselSeString(List<HaselPayload> payloads)
    {
        Payloads = payloads;
    }

    public byte[] Encode()
    {
        return Payloads.SelectMany(payload => payload.Encode()).ToArray();
    }

    public HaselSeString Resolve(List<ExpressionWrapper>? localParameters = null)
    {
        return new(Payloads.SelectMany(payload => payload.Resolve(localParameters).Payloads).ToList());
    }

    public override string ToString()
    {
        return Payloads.Aggregate(new StringBuilder(), (sb, tp) => sb.Append(tp.ToString()), sb => sb.ToString());
    }

    public static unsafe HaselSeString FromAddon(uint addonId)
    {
        var row = Service.DataManager.GetExcelSheet<Addon>(Service.TranslationManager.ClientLanguage)?.GetRow(addonId);
        return row == null
            ? new()
            : Parse(row.Text.RawData);
    }

    public static unsafe HaselSeString FromMacro(string macro)
    {
        var input = Utf8String.FromString(macro);
        var output = Utf8String.CreateEmpty();
        RaptureTextModule.Instance()->TextModule.EncodeString(output, input);
        var str = Parse(output->StringPtr, output->Length);
        output->Dtor(true);
        input->Dtor(true);
        return str;
    }

    public static unsafe HaselSeString Parse(ReadOnlySpan<byte> data)
    {
        fixed (byte* ptr = data)
        {
            return Parse(ptr, data.Length);
        }
    }

    public static unsafe HaselSeString Parse(byte* ptr, int len)
    {
        var str = new HaselSeString();

        if (ptr == null)
            return str;

        using var stream = new UnmanagedMemoryStream(ptr, len);
        using var reader = new BinaryReader(stream);

        while (stream.Position < len)
            str.Payloads.Add(HaselPayload.From(reader));

        return str;
    }

    public static implicit operator HaselSeString(HaselPayload payload) => new([payload]);
    public static implicit operator HaselSeString(string str) => new([new TextPayload(str)]);
    public static implicit operator HaselSeString(uint value) => value.ToString(); // TODO: encode number properly? used for Resolve([...])
    public static implicit operator HaselSeString(Lumina.Text.SeString str) => Parse(str.RawData);

    public static implicit operator byte[](HaselSeString str) => str.Encode();
}
