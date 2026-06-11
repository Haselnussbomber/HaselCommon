using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;

namespace HaselCommon.Game;

public static unsafe class Chat
{
    public static void ExecuteCommand(string command)
    {
        if (!command.StartsWith('/'))
            return;

        using var cmd = new Utf8String(command);
        RaptureShellModule.Instance()->ExecuteCommandInner(&cmd, UIModule.Instance());
    }

    public static void Print(string message, string? sender = null, ushort logKindId = 1)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        using var utf8Sender = new Utf8String(sender ?? string.Empty);
        using var utf8Message = new Utf8String(message);
        var logInfo = new LogInfo { LogKind = logKindId };
        RaptureLogModule.Instance()->PrintMessage(logInfo, &utf8Sender, &utf8Message, 0);
    }

    public static void Print(ReadOnlySpan<byte> message, ReadOnlySpan<byte> sender = default, ushort logKindId = 1)
    {
        if (message.IsEmpty)
            return;

        using var rssb = new RentedSeStringBuilder();
        using var utf8Sender = sender.IsEmpty ? new Utf8String() : new Utf8String(rssb.Builder.Append(sender).GetViewAsSpan());
        using var utf8Message = new Utf8String(rssb.Builder.Clear().Append(message).GetViewAsSpan());
        var logInfo = new LogInfo { LogKind = logKindId };
        RaptureLogModule.Instance()->PrintMessage(logInfo, &utf8Sender, &utf8Message, 0);
    }

    public static void Print(ReadOnlySeString message, ReadOnlySeString sender = default, ushort logKindId = 1)
    {
        if (message.IsEmpty)
            return;

        using var rssb = new RentedSeStringBuilder();
        using var utf8Sender = sender.IsEmpty ? new Utf8String() : new Utf8String(rssb.Builder.Append(sender).GetViewAsSpan());
        using var utf8Message = new Utf8String(rssb.Builder.Clear().Append(message).GetViewAsSpan());
        var logInfo = new LogInfo { LogKind = logKindId };
        RaptureLogModule.Instance()->PrintMessage(logInfo, &utf8Sender, &utf8Message, 0);
    }

    public static void Print(ReadOnlySeStringSpan message, ReadOnlySeStringSpan sender = default, ushort logKindId = 1)
    {
        if (message.IsEmpty)
            return;

        using var rssb = new RentedSeStringBuilder();
        using var utf8Sender = sender.IsEmpty ? new Utf8String() : new Utf8String(rssb.Builder.Append(sender).GetViewAsSpan());
        using var utf8Message = new Utf8String(rssb.Builder.Clear().Append(message).GetViewAsSpan());
        var logInfo = new LogInfo { LogKind = logKindId };
        RaptureLogModule.Instance()->PrintMessage(logInfo, &utf8Sender, &utf8Message, 0);
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
