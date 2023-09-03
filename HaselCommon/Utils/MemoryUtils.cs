using Dalamud.Memory;

namespace HaselCommon.Utils;

public static unsafe class MemoryUtils
{
    public static T GetDelegateForSignature<T>(string signature) where T : Delegate
    {
        var address = Service.SigScanner.ScanText(signature);
        return Marshal.GetDelegateForFunctionPointer<T>(address);
    }

    public static byte[] ReplaceRaw(nint address, byte[] data)
    {
        var originalBytes = MemoryHelper.ReadRaw(address, data.Length);

        MemoryHelper.ChangePermission(address, data.Length, MemoryProtection.ExecuteReadWrite, out var oldPermissions);
        MemoryHelper.WriteRaw(address, data);
        MemoryHelper.ChangePermission(address, data.Length, oldPermissions);

        return originalBytes;
    }
}
