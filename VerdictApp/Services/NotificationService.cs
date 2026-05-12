using Microsoft.EntityFrameworkCore;
using VerdictApp.Data;
using VerdictApp.Models;

namespace VerdictApp.Services;

public class NotificationService(ApplicationDbContext db)
{
    public async Task NotifyVoteAsync(string voterName, Dilemma dilemma)
    {
        if (dilemma.UserId == null) return;
        db.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            RecipientUserId = dilemma.UserId,
            Type = NotificationType.Vote,
            Message = $"{voterName} voted on your dilemma \"{dilemma.Title}\"",
            DilemmaId = dilemma.Id,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    public async Task NotifyCommentAsync(string commenterName, Dilemma dilemma, string currentUserId)
    {
        if (dilemma.UserId == null || dilemma.UserId == currentUserId) return;
        db.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            RecipientUserId = dilemma.UserId,
            Type = NotificationType.Comment,
            Message = $"{commenterName} commented on your dilemma \"{dilemma.Title}\"",
            DilemmaId = dilemma.Id,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    public async Task NotifyNewPostAsync(Dilemma dilemma, string posterUserId)
    {
        // Notify users who have previously voted on a dilemma in the same category
        var interestedUserIds = await db.Votes
            .Include(v => v.DilemmaOption).ThenInclude(o => o.Dilemma)
            .Where(v => v.DilemmaOption.Dilemma.Category == dilemma.Category
                     && v.UserId != posterUserId)
            .Select(v => v.UserId)
            .Distinct()
            .ToListAsync();

        var notifications = interestedUserIds.Select(uid => new Notification
        {
            Id = Guid.NewGuid(),
            RecipientUserId = uid,
            Type = NotificationType.NewPost,
            Message = $"New {dilemma.Category} dilemma: \"{dilemma.Title}\"",
            DilemmaId = dilemma.Id,
            CreatedAt = DateTime.UtcNow
        });

        db.Notifications.AddRange(notifications);
        await db.SaveChangesAsync();
    }
}
