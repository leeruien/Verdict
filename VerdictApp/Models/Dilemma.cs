using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
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
    public string? ImagePath { get; set; }
    public string? ImagePaths { get; set; }

    [NotMapped]
    public List<string> ImagePathList =>
        !string.IsNullOrEmpty(ImagePaths)
            ? JsonSerializer.Deserialize<List<string>>(ImagePaths) ?? new()
            : !string.IsNullOrEmpty(ImagePath)
                ? new List<string> { ImagePath }
                : new();

    public ApplicationUser User { get; set; }
    public List<DilemmaOption> Options { get; set; }
    public List<Comment> Comments { get; set; }
}