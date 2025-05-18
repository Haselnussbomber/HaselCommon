namespace HaselCommon.Gui;

public struct EndUnconditionally(Action endAction, bool success) : ImRaii.IEndObject, IDisposable
{
    private Action EndAction { get; } = endAction;

    public bool Success { get; } = success;

    public bool Disposed { get; private set; } = false;

    public void Dispose()
    {
        if (!Disposed)
        {
            EndAction();
            Disposed = true;
        }
    }
}
