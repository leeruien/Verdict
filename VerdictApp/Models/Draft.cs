namespace VerdictApp.Models;

public class Draft
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Category { get; set; } = "";
    public int ExpiresAtHours { get; set; } = 24;
    public string OptionsJson { get; set; } = "[]";
    public DateTime SavedAt { get; set; }
}
