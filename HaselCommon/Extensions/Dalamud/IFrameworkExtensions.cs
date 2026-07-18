namespace HaselCommon.Extensions;

public static class IFrameworkExtensions
{
    extension(IFramework framework)
    {
        public Debouncer CreateHaselDebouncer(TimeSpan delay, Action action)
        {
            return new Debouncer(framework, delay, action);
        }
    }
}
