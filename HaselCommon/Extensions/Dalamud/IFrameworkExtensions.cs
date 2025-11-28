namespace HaselCommon.Extensions;

public static class IFrameworkExtensions
{
    public static Debouncer CreateDebouncer(this IFramework framework, TimeSpan delay, Action action)
    {
        return new Debouncer(framework, delay, action);
    }
}
