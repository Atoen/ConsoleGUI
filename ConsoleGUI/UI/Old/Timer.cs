namespace ConsoleGUI.UI.Old;

public class Timer
{
    private DateTime _startTime;
    private readonly TimeSpan _timeSpan;
    private readonly bool _countingDown;
    private readonly CancellationTokenSource _tokenSource = new();

    private readonly string _format;

    public int Seconds => (int) Math.Ceiling(_countingDown
        ? (_startTime - DateTime.Now).TotalSeconds
        : (DateTime.Now - _startTime).TotalSeconds);

    public Timer()
    {
        _startTime = DateTime.Now;
        _timeSpan = TimeSpan.Zero;

        _format = string.Empty;
    }

    public Timer(TimeSpan timeSpan, bool countDown = true)
    {
        _countingDown = countDown;
        _startTime = DateTime.Now.Add(countDown ? timeSpan : -timeSpan);
        _timeSpan = timeSpan;

        if (countDown)
        {
            SetTimeout(timeSpan);
        }

        _format = $"D{(int) Math.Ceiling(Math.Log10(Seconds))}";
    }

    public void StartNow()
    {
        _startTime = DateTime.Now.Add(_countingDown ? _timeSpan : -_timeSpan);
    }

    public string String => Seconds.ToString(_format);

    public event Action? Elapsed;

    public void Cancel()
    {
        if (!_countingDown) return;

        _tokenSource.Cancel();
    }

    private async void SetTimeout(TimeSpan timeSpan)
    {
        try
        {
            await Task.Delay(timeSpan, _tokenSource.Token);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        Elapsed?.Invoke();
    }
}