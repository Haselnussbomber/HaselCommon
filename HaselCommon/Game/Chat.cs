using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;
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

    public static void Print(ReadOnlySeString message, ReadOnlySeString sender = default, ushort logKindId = 1)
    {
        var senderLength = sender.ByteLength;
        var senderBytes = senderLength < 512 ? stackalloc byte[senderLength + 1] : new byte[senderLength + 1];
        sender.Data.Span.CopyTo(senderBytes);
        senderBytes[senderLength] = 0;

        var messageLength = message.ByteLength;
        var messageBytes = messageLength < 512 ? stackalloc byte[messageLength + 1] : new byte[messageLength + 1];
        message.Data.Span.CopyTo(messageBytes);
        messageBytes[messageLength] = 0;

        using var _sender = new Utf8String(senderBytes);
        using var _message = new Utf8String(messageBytes);
        RaptureLogModule.Instance()->PrintMessage(logKindId, &_sender, &_message, 0);
    }

    public static void Print(ReadOnlySeStringSpan message, ReadOnlySeStringSpan sender = default, ushort logKindId = 1)
    {
        var senderLength = sender.ByteLength;
        var senderBytes = senderLength < 512 ? stackalloc byte[senderLength + 1] : new byte[senderLength + 1];
        sender.Data.CopyTo(senderBytes);
        senderBytes[senderLength] = 0;

        var messageLength = message.ByteLength;
        var messageBytes = messageLength < 512 ? stackalloc byte[messageLength + 1] : new byte[messageLength + 1];
        message.Data.CopyTo(messageBytes);
        messageBytes[messageLength] = 0;

        using var _sender = new Utf8String(senderBytes);
        using var _message = new Utf8String(messageBytes);
        RaptureLogModule.Instance()->PrintMessage(logKindId, &_sender, &_message, 0);
    }
}
