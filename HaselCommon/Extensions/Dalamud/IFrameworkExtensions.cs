namespace HaselCommon.Extensions;

public static class IFrameworkExtensions
{
    extension(IFramework framework)
    {
        public Debouncer CreateDebouncer(TimeSpan delay, Action action)
        {
            return new Debouncer(framework, delay, action);
        }
    }
}
