using VerdictApp.Data;
namespace VerdictApp.Models;

public class Notification
{
    public Guid Id { get; set; }
    public string RecipientUserId { get; set; } = "";
    public NotificationType Type { get; set; }
    public string Message { get; set; } = "";
    public Guid? DilemmaId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public ApplicationUser Recipient { get; set; } = null!;
    public Dilemma? Dilemma { get; set; }
}
