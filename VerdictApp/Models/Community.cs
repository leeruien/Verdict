using VerdictApp.Data;
namespace VerdictApp.Models;
public class Community
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public string Description { get; set; } = "";
    public string Icon { get; set; } = "🌐";
    public string CreatedByUserId { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public ApplicationUser CreatedBy { get; set; } = null!;
}
