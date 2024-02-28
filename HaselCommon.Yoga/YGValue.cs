using System.Runtime.InteropServices;

namespace HaselCommon.Yoga;

[StructLayout(LayoutKind.Sequential)]
public struct YGValue
{
    public float value;
    public Unit unit;
}
