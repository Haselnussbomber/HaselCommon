namespace HaselCommon.Events;

#pragma warning disable CS8618

public class ValueChangedEventArgs<TValue> : EventArgs where TValue : notnull
{
    private static readonly ValueChangedEventArgs<TValue> Instance = new();
    private ValueChangedEventArgs() { }

    public TValue OldValue { get; private set; }
    public TValue NewValue { get; private set; }

    public static ValueChangedEventArgs<TValue> With(TValue oldValue, TValue newValue)
    {
        Instance.OldValue = oldValue;
        Instance.NewValue = newValue;
        return Instance;
    }
}

public class ValueChangedEventArgs<TSender, TValue> : EventArgs
    where TSender : notnull
    where TValue : notnull
{
    private static readonly ValueChangedEventArgs<TSender, TValue> Instance = new();
    private ValueChangedEventArgs() { }

    public TSender Sender { get; private set; }
    public TValue OldValue { get; private set; }
    public TValue NewValue { get; private set; }

    public static ValueChangedEventArgs<TSender, TValue> With(TSender sender, TValue oldValue, TValue newValue)
    {
        Instance.Sender = sender;
        Instance.OldValue = oldValue;
        Instance.NewValue = newValue;
        return Instance;
    }
}
