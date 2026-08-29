using Dalamud.Game.Chat;

namespace HaselCommon.Extensions;

public static partial class IChatGuiExtensions
{
    public delegate void HandleableChatMessageDelegate(IHandleableChatMessage message);
    public delegate void ChatMessageDelegate(IChatMessage message);
    public delegate void LogMessageDelegate(ILogMessage message);

    extension(IChatGui chatGui)
    {
        public IDisposable OnChatMessage(HandleableChatMessageDelegate handler)
        {
            return EventExtensions.Subscribe(
                handler => chatGui.ChatMessage += handler,
                handler => chatGui.ChatMessage -= handler,
                (IChatGui.OnHandleableChatMessageDelegate)handler.Invoke
            );
        }

        public IDisposable OnCheckMessageHandled(HandleableChatMessageDelegate handler)
        {
            return EventExtensions.Subscribe(
                handler => chatGui.CheckMessageHandled += handler,
                handler => chatGui.CheckMessageHandled -= handler,
                (IChatGui.OnHandleableChatMessageDelegate)handler.Invoke
            );
        }

        public IDisposable OnChatMessageHandled(ChatMessageDelegate handler)
        {
            return EventExtensions.Subscribe(
                handler => chatGui.ChatMessageHandled += handler,
                handler => chatGui.ChatMessageHandled -= handler,
                (IChatGui.OnChatMessageDelegate)handler.Invoke
            );
        }

        public IDisposable OnChatMessageUnhandled(ChatMessageDelegate handler)
        {
            return EventExtensions.Subscribe(
                handler => chatGui.ChatMessageUnhandled += handler,
                handler => chatGui.ChatMessageUnhandled -= handler,
                (IChatGui.OnChatMessageDelegate)handler.Invoke
            );
        }

        public IDisposable OnLogMessage(LogMessageDelegate handler)
        {
            return EventExtensions.Subscribe(
                handler => chatGui.LogMessage += handler,
                handler => chatGui.LogMessage -= handler,
                (IChatGui.OnLogMessageDelegate)handler.Invoke
            );
        }
    }
}
