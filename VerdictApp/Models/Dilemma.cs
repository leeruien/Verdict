using VerdictApp.Data;
using VerdictApp.Models;
namespace VerdictApp.Models;
public class Dilemma {
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public ApplicationUser User { get; set; }
    public List<DilemmaOption> Options { get; set; }
    public List<Comment> Comments { get; set; }
}