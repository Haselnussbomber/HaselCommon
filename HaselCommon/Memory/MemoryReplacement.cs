namespace HaselCommon.Memory;

public class MemoryReplacement(nint address, byte[] replacementBytes) : IDisposable
{
    private byte[]? _originalBytes;

    public void Enable()
    {
        if (_originalBytes != null)
            return;

        _originalBytes = MemoryUtils.ReplaceRaw(address, replacementBytes);
    }

    public void Disable()
    {
        if (_originalBytes == null)
            return;

        MemoryUtils.ReplaceRaw(address, _originalBytes);
        _originalBytes = null;
    }

    public void Dispose()
    {
        Disable();
        GC.SuppressFinalize(this);
    }
}
