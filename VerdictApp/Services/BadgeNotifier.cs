namespace VerdictApp.Services;

public class BadgeNotifier
{
    public event Func<Task>? OnCountsChanged;

    public async Task NotifyAsync()
    {
        if (OnCountsChanged != null)
            await OnCountsChanged.Invoke();
    }
}
