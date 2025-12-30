using System.Diagnostics.CodeAnalysis;

namespace HaselCommon.Extensions;

public static class IServiceProviderExtensions
{
    extension(IServiceProvider provider)
    {
        public T CreateInstance<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(params object[] parameters)
        {
            return ActivatorUtilities.CreateInstance<T>(provider, parameters);
        }

        public bool TryGetService<T>([NotNullWhen(returnValue: true)] out T? service)
        {
            try
            {
                service = provider.GetService<T>();
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
