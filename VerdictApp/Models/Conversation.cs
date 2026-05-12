using VerdictApp.Data;
namespace VerdictApp.Models;

public class Conversation
{
    public Guid Id { get; set; }
    public string InitiatorId { get; set; } = "";
    public string RecipientId { get; set; } = "";
    public ConversationStatus Status { get; set; } = ConversationStatus.Pending;
    public DateTime CreatedAt { get; set; }
    public ApplicationUser Initiator { get; set; } = null!;
    public ApplicationUser Recipient { get; set; } = null!;
    public List<DirectMessage> Messages { get; set; } = new();
}
