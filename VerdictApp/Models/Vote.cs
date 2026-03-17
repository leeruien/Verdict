using VerdictApp.Data;
using VerdictApp.Models;
namespace VerdictApp.Models;
public class Vote {
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public Guid DilemmaOptionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public ApplicationUser User { get; set; }
    public DilemmaOption DilemmaOption { get; set; }
}