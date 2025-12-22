using System.Diagnostics.CodeAnalysis;

namespace HaselCommon.Extensions;

public static class IServiceProviderExtensions
{
    extension(IServiceProvider serviceProvider)
    {
        public bool TryGetService<T>([NotNullWhen(returnValue: true)] out T? service)
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
}
