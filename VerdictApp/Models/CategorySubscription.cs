using VerdictApp.Data;
namespace VerdictApp.Models;

public class CategorySubscription
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = "";
    public string Category { get; set; } = "";
    public DateTime SubscribedAt { get; set; }
    public ApplicationUser User { get; set; } = null!;
}
