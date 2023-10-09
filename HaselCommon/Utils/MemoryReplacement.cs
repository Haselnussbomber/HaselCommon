namespace HaselCommon.Utils;

public class MemoryReplacement : IDisposable
{
    private readonly nint address;
    private readonly byte[] replacementBytes;
    private byte[]? originalBytes;

    public MemoryReplacement(nint address, byte[] replacementBytes)
    {
        this.address = address;
        this.replacementBytes = replacementBytes;
    }

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
    }
}
