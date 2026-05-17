namespace VerdictApp.Models;

public class Report
{
    public Guid Id { get; set; }
    public string ReporterUserId { get; set; } = "";
    public Guid? DilemmaId { get; set; }
    public Guid? CommentId { get; set; }
    public string Reason { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
