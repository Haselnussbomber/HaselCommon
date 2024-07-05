namespace HaselCommon.Utils;

public class MemoryReplacement(nint address, byte[] replacementBytes) : IDisposable
{
    private byte[]? originalBytes;

    public void Enable()
    {
        if (originalBytes != null)
            return;

        originalBytes = MemoryUtils.ReplaceRaw(address, replacementBytes);
    }

    public void Disable()
    {
        if (originalBytes == null)
            return;

        MemoryUtils.ReplaceRaw(address, originalBytes);
        originalBytes = null;
    }

    public void Dispose()
    {
        Disable();
        GC.SuppressFinalize(this);
    }
}
