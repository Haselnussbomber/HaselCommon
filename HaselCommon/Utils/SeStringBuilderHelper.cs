using HaselCommon.Gui;
using Lumina.Text;

namespace HaselCommon.Utils;

public static class SeStringBuilderHelper
{
    public static IDisposable Rent(out SeStringBuilder seStringBuilder)
    {
        var sb = SeStringBuilder.SharedPool.Get();
        seStringBuilder = sb;
        return new EndUnconditionally(() => SeStringBuilder.SharedPool.Return(sb), true);
    }
}
