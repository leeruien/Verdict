using Microsoft.EntityFrameworkCore;
using VerdictApp.Data;
using VerdictApp.Models;

namespace VerdictApp.Services;

public class ExpiryNotificationService(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckExpiringDilemmasAsync();
            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }

    private async Task CheckExpiringDilemmasAsync()
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var soon = DateTime.UtcNow.AddHours(24);

        // Dilemmas expiring within 24 hours that haven't had an ExpiringSoon notification yet
        var alreadyNotified = await db.Notifications
            .Where(n => n.Type == NotificationType.ExpiringSoon)
            .Select(n => n.DilemmaId)
            .ToListAsync();

        var expiring = await db.Dilemmas
            .Where(d => d.ExpiresAt.HasValue
                     && d.ExpiresAt.Value > DateTime.UtcNow
                     && d.ExpiresAt.Value <= soon
                     && !alreadyNotified.Contains(d.Id))
            .ToListAsync();

        foreach (var dilemma in expiring)
        {
            db.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                RecipientUserId = dilemma.UserId,
                Type = NotificationType.ExpiringSoon,
                Message = $"Your dilemma \"{dilemma.Title}\" is expiring within 24 hours.",
                DilemmaId = dilemma.Id,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (expiring.Any())
            await db.SaveChangesAsync();
    }
}
