using Microsoft.EntityFrameworkCore;
using VerdictApp.Data;
using VerdictApp.Models;

namespace VerdictApp.Services;

public class NotificationService(ApplicationDbContext db)
{
    public async Task NotifyVoteAsync(string voterName, Dilemma dilemma)
    {
        if (dilemma.UserId == null) return;

        // Upsert: update the existing unread vote notification for this dilemma
        // so the owner gets one ping that reflects the latest voter, not one per vote.
        var existing = await db.Notifications.FirstOrDefaultAsync(n =>
            n.RecipientUserId == dilemma.UserId &&
            n.Type == NotificationType.Vote &&
            n.DilemmaId == dilemma.Id &&
            !n.IsRead);

        if (existing != null)
        {
            existing.Message = $"{voterName} and others voted on your dilemma \"{dilemma.Title}\"";
            existing.CreatedAt = DateTime.UtcNow;
        }
        else
        {
            db.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                RecipientUserId = dilemma.UserId,
                Type = NotificationType.Vote,
                Message = $"{voterName} voted on your dilemma \"{dilemma.Title}\"",
                DilemmaId = dilemma.Id,
                CreatedAt = DateTime.UtcNow
            });
        }
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

    public async Task NotifyPostRemovedAsync(string userId, string postTitle)
    {
        db.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            RecipientUserId = userId,
            Type = NotificationType.ContentRemoved,
            Message = $"Your post \"{postTitle}\" was removed for violating community guidelines.",
            DilemmaId = null,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    public async Task NotifyCommentRemovedAsync(string userId, string postTitle, Guid dilemmaId)
    {
        db.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            RecipientUserId = userId,
            Type = NotificationType.ContentRemoved,
            Message = $"Your comment on \"{postTitle}\" was removed for violating community guidelines.",
            DilemmaId = dilemmaId,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    public async Task NotifyNewPostAsync(Dilemma dilemma, string posterUserId)
    {
        var subscriberIds = await db.CategorySubscriptions
            .Where(s => s.Category == dilemma.Category && s.UserId != posterUserId)
            .Select(s => s.UserId)
            .ToListAsync();

        var notifications = subscriberIds.Select(uid => new Notification
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
