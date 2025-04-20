using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;
using HaselCommon.Extensions.Memory;
using HaselCommon.Extensions.Strings;
using Lumina.Text.ReadOnly;

namespace HaselCommon.Game;

public static unsafe class Chat
{
    public static unsafe void ExecuteCommand(string command)
    {
        using var cmd = new Utf8String(command);
        RaptureShellModule.Instance()->ExecuteCommandInner(&cmd, UIModule.Instance());
    }

    public static void Print(string message, string? sender = null, ushort logKindId = 1)
    {
        using var _sender = new Utf8String(sender ?? string.Empty);
        using var _message = new Utf8String(message);
        RaptureLogModule.Instance()->PrintMessage(logKindId, &_sender, &_message, 0);
    }

    public static void Print(ReadOnlySpan<byte> message, ReadOnlySpan<byte> sender = default, ushort logKindId = 1)
    {
        using var utf8Sender = new Utf8String(sender.WithNullTerminator());
        using var utf8Message = new Utf8String(message.WithNullTerminator());
        RaptureLogModule.Instance()->PrintMessage(logKindId, &utf8Sender, &utf8Message, 0);
    }

    public static void Print(ReadOnlySeString message, ReadOnlySeString sender = default, ushort logKindId = 1)
    {
        using var utf8Sender = new Utf8String(sender.Data.Span.WithNullTerminator());
        using var utf8Message = new Utf8String(message.Data.Span.WithNullTerminator());
        RaptureLogModule.Instance()->PrintMessage(logKindId, &utf8Sender, &utf8Message, 0);
    }

    public static void Print(ReadOnlySeStringSpan message, ReadOnlySeStringSpan sender = default, ushort logKindId = 1)
    {
        using var utf8Sender = new Utf8String(sender.Data.WithNullTerminator());
        using var utf8Message = new Utf8String(message.Data.WithNullTerminator());
        RaptureLogModule.Instance()->PrintMessage(logKindId, &utf8Sender, &utf8Message, 0);
    }

    public static void PrintError(string message, string? sender = null)
        => Print(message, sender, 2);

    public static void PrintError(ReadOnlySpan<byte> message, ReadOnlySpan<byte> sender = default)
        => Print(message, sender, 2);

    public static void PrintError(ReadOnlySeString message, ReadOnlySeString sender = default)
        => Print(message, sender, 2);

    public static void PrintError(ReadOnlySeStringSpan message, ReadOnlySeStringSpan sender = default)
        => Print(message, sender, 2);
}
