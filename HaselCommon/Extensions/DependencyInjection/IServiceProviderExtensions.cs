using System.Diagnostics.CodeAnalysis;

namespace HaselCommon.Extensions;

public static class IServiceProviderExtensions
{
    extension(IServiceProvider provider)
    {
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

    /// <inheritdoc cref="ActivatorUtilities.CreateFactory{T}(Type[])" />
    public static T CreateInstance<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(this IServiceProvider provider, params object[] parameters)
    {
        return ActivatorUtilities.CreateInstance<T>(provider, parameters);
    }
}
