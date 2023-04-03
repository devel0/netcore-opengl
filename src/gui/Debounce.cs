/// <summary>
/// Helper class to manage action debouncing.<br/>
/// Example follows:<br/>
/// \snippet{'trimleft'} examples/example-0019/Views/MainWindow.axaml.cs DebounceExample
/// </summary>
public class DebounceAction
{

    public TimeSpan TimeSpan { get; private set; }

    Action Action;

    CancellationTokenSource cts = new CancellationTokenSource();

    /// <summary>
    /// Create a debounce action helper.
    /// </summary>
    /// <param name="timeSpan">Timeout after which the given action executes.</param>
    /// <param name="action">Action to execute when time expired and no <see cref="Hit"/> called.<br/>
    /// If a hit happens before previous hit expired the previous will cancelled and start over.</param>
    public DebounceAction(TimeSpan timeSpan, Action action)
    {
        TimeSpan = timeSpan;
        Action = action;
    }

    /// <summary>
    /// Cancel scheduled action and starts a new one that will executes
    /// if no further hit happens before timeout.
    /// </summary>
    public void Hit()
    {
        cts.Cancel();

        var newCts = new CancellationTokenSource();
        cts = newCts;

        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(TimeSpan, newCts.Token);
            }
            catch (TaskCanceledException)
            {
            }
            if (newCts.Token.IsCancellationRequested) return;

            Action.Invoke();
        });
    }

}