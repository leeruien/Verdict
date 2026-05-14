namespace VerdictApp.Services;

public class RecentGroupsNotifier
{
    public event Func<Task>? OnGroupVisited;

    public async Task NotifyVisitedAsync()
    {
        if (OnGroupVisited != null)
            await OnGroupVisited.Invoke();
    }
}
