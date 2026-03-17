using VerdictApp.Data;
using VerdictApp.Models;
namespace VerdictApp.Models;

public class Comment {
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public Guid DilemmaId { get; set; }
    public string Body { get; set; }
    public DateTime CreatedAt { get; set; }
    public ApplicationUser User { get; set; }
    public Dilemma Dilemma { get; set; }
}