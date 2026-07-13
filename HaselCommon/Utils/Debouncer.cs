using System.Threading;

namespace HaselCommon.Utils;

// Pending PR to Dalamud: https://github.com/goatcorp/Dalamud/pull/2867
/// <summary>
/// Provides a thread-safe mechanism to debounce actions, ensuring that a rapid succession 
/// of calls only triggers the action after a specified delay has elapsed since the last call.
/// </summary>
public class Debouncer : IDisposable
{
    private readonly IFramework _framework;
    private readonly TimeSpan _delay;
    private readonly Action _action;
    private readonly Lock _debouncerLock = new();
    private CancellationTokenSource? _cts;
    private DateTime _targetTime = DateTime.MinValue;
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="Debouncer"/> class.
    /// </summary>
    /// <param name="framework">Dalamuds <see cref="IFramework"/> service.</param>
    /// <param name="delay">The delay to wait after the last request before executing the action.</param>
    /// <param name="action">The delegate to execute when the debounce period elapses.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is <see langword="null"/>.</exception>
    public Debouncer(IFramework framework, TimeSpan delay, Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        _framework = framework;
        _delay = delay;
        _action = action;
    }

    /// <summary>
    /// Gets a value indicating whether the action is queued to be executed.
    /// </summary>
    public bool IsPending => _cts != null;

    /// <inheritdoc/>
    public void Dispose()
    {
        using var scope = _debouncerLock.EnterScope();

        if (_isDisposed)
            return;

        Cancel();

        _isDisposed = true;
    }

    /// <summary>
    /// Requests the execution of the action.
    /// </summary>
    public void Debounce()
    {
        using var scope = _debouncerLock.EnterScope();

        if (_isDisposed)
            return;

        _targetTime = DateTime.UtcNow + _delay;

        if (IsPending)
            return;

        _cts = new();
        _framework.RunOnTick(OnTick, _delay, cancellationToken: _cts.Token);
    }

    /// <summary>
    /// Cancels the pending execution of the action.
    /// </summary>
    public void Cancel()
    {
        using var scope = _debouncerLock.EnterScope();

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    private void OnTick()
    {
        using (_debouncerLock.EnterScope())
        {
            if (_isDisposed)
                return;

            var now = DateTime.UtcNow;
            if (now < _targetTime && _cts != null)
            {
                _framework.RunOnTick(OnTick, _targetTime - now, cancellationToken: _cts.Token);
                return;
            }

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        _action();
    }
}
