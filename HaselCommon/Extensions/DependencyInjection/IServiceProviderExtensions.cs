using System.Diagnostics.CodeAnalysis;

namespace HaselCommon.Extensions;

public static class IServiceProviderExtensions
{
    public static bool TryGetService<T>(this IServiceProvider serviceProvider, [NotNullWhen(returnValue: true)] out T? service)
    {
        return (service = serviceProvider.GetService<T>()) != null;
    }
}
