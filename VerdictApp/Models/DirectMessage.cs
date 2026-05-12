using VerdictApp.Data;
namespace VerdictApp.Models;

public class DirectMessage
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public string SenderId { get; set; } = "";
    public string Body { get; set; } = "";
    public DateTime SentAt { get; set; }
    public Conversation Conversation { get; set; } = null!;
    public ApplicationUser Sender { get; set; } = null!;
}
