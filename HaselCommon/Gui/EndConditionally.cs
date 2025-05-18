namespace HaselCommon.Gui;

public struct EndConditionally(Action endAction, bool success) : ImRaii.IEndObject, IDisposable
{
    public bool Success { get; } = success;

    public bool Disposed { get; private set; } = false;

    private Action EndAction { get; } = endAction;

    public void Dispose()
    {
        if (!Disposed)
        {
            if (Success)
                EndAction();

            Disposed = true;
        }
    }
}
