using System.Diagnostics.CodeAnalysis;

namespace HaselCommon.Extensions;

public static class IServiceProviderExtensions
{
    extension(IServiceProvider serviceProvider)
    {
        public bool TryGetService<T>([NotNullWhen(returnValue: true)] out T? service)
        {
            return (service = serviceProvider.GetService<T>()) != null;
        }
    }
}
