namespace HaselCommon.Events;

public class ValueChangedEventArgs<TValue> : EventArgs where TValue : notnull
{
    public required TValue OldValue { get; init; }
    public required TValue NewValue { get; init; }
}

public class ValueChangedEventArgs<TSender, TValue> : EventArgs
    where TSender : notnull
    where TValue : notnull
{
    public required TSender Sender { get; init; }
    public required TValue OldValue { get; init; }
    public required TValue NewValue { get; init; }
}
