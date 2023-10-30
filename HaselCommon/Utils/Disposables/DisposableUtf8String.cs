using System.Text;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.System.Memory;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace HaselCommon.Utils;

public unsafe class DisposableUtf8String : DisposableCreatable<Utf8String>, IDisposable
{
    public DisposableUtf8String() : base()
    {
    }

    public DisposableUtf8String(byte* text) : base()
    {
        SetString(text);
    }

    public DisposableUtf8String(byte[] text) : base()
    {
        SetString(text);
    }

    public DisposableUtf8String(string text) : base()
    {
        SetString(text);
    }

    public DisposableUtf8String(SeString text) : base()
    {
        SetString(text);
    }

    public void SetString(byte* text)
        => Ptr->SetString(text);

    public void SetString(byte[] text)
        => Ptr->SetString(text);

    public void SetString(string text)
        => Ptr->SetString(text);

    public void SetString(SeString text)
        => SetString(text.Encode());

    public void AppendMacro(string macroCode)
    {
        using var input = new DisposableUtf8String(macroCode);
        using var output = new DisposableUtf8String();

        RaptureTextModule.Instance()->TextModule.ProcessMacroCode(output, input.Ptr->StringPtr);
        PronounModule.Instance()->ProcessString(output, false);

        Append(output);
    }

    public void AppendFixed(string fixedString)
    {
        using var input = new DisposableUtf8String(fixedString);
        using var output = new DisposableUtf8String();

        RaptureTextModule.Instance()->TextModule.ProcessMacroCode(output, input.Ptr->StringPtr);
        var out1 = PronounModule.Instance()->ProcessString(output, true);
        var out2 = PronounModule.Instance()->ProcessString(out1, false);

        Append(out2);
    }

    public void Prepend(Utf8String* other)
        => Ptr->Prepend(other);

    public void Prepend(string text)
        => Prepend(Encoding.UTF8.GetBytes(text));

    public void Prepend(byte[] text)
        => Prepend(text.AsSpan());

    public void Prepend(SeString text)
        => Prepend(text.Encode());

    public void Prepend(ReadOnlySpan<byte> text)
    {
        var temp = Utf8String.FromSequence(text);
        Ptr->Prepend(temp);
        temp->Dtor();
        IMemorySpace.Free(temp);
    }

    public void Prepend(byte* text)
    {
        var temp = Utf8String.FromSequence(text);
        Ptr->Prepend(temp);
        temp->Dtor();
        IMemorySpace.Free(temp);
    }

    public void Append(Utf8String* other)
        => Ptr->Append(other);

    public void Append(string text)
        => Append(Encoding.UTF8.GetBytes(text));

    public void Append(byte[] text)
        => Append(text.AsSpan());

    public void Append(SeString text)
        => Append(text.Encode());

    public void Append(ReadOnlySpan<byte> text)
    {
        var temp = Utf8String.FromSequence(text);
        Ptr->Append(temp);
        temp->Dtor();
        IMemorySpace.Free(temp);
    }

    public void Append(byte* text)
    {
        var temp = Utf8String.FromSequence(text);
        Ptr->Append(temp);
        temp->Dtor();
        IMemorySpace.Free(temp);
    }

    public new string ToString()
        => Ptr->ToString();

    public SeString ToSeString()
        => SeString.Parse(Ptr->StringPtr, (int)Ptr->BufSize);

    public new void Dispose()
    {
        if (Ptr == null)
            return;

        Ptr->Dtor();
        base.Dispose();

        GC.SuppressFinalize(this);
    }
}
