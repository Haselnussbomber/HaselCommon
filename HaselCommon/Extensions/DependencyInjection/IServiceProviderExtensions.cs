using System.Diagnostics.CodeAnalysis;

namespace HaselCommon.Extensions;

public static class IServiceProviderExtensions
{
    public static bool TryGetService<T>(this IServiceProvider serviceProvider, [NotNullWhen(returnValue: true)] out T? service)
    {
        try
        {
            service = serviceProvider.GetService<T>();
            return service != null;
        }
        catch // might catch ObjectDisposedException here
        {
            service = default;
            return false;
        }
    }
}
