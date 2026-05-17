using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VerdictApp.Models;

namespace VerdictApp.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Dilemma> Dilemmas { get; set; }
    public DbSet<DilemmaOption> DilemmaOptions { get; set; }
    public DbSet<Vote> Votes { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<CategorySubscription> CategorySubscriptions { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<DirectMessage> DirectMessages { get; set; }
    public DbSet<Community> Communities { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<Draft> Drafts { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Dilemma>().ToTable("Dilemmas");
        builder.Entity<DilemmaOption>().ToTable("DilemmaOptions");
        builder.Entity<Vote>().ToTable("Votes");
        builder.Entity<Comment>().ToTable("Comments");
        builder.Entity<Notification>().ToTable("Notifications");
        builder.Entity<CategorySubscription>().ToTable("CategorySubscriptions")
            .HasIndex(s => new { s.UserId, s.Category }).IsUnique();
        builder.Entity<Conversation>().ToTable("Conversations");
        builder.Entity<DirectMessage>().ToTable("DirectMessages");
        builder.Entity<Community>().ToTable("Communities")
            .HasIndex(c => c.Slug).IsUnique();

        builder.Entity<Vote>()
            .HasIndex(v => new { v.UserId, v.DilemmaOptionId })
            .IsUnique();

        builder.Entity<Report>().ToTable("Reports");
        builder.Entity<Draft>().ToTable("Drafts");
        builder.Entity<Report>()
            .HasIndex(r => new { r.ReporterUserId, r.DilemmaId });
        builder.Entity<Report>()
            .HasIndex(r => new { r.ReporterUserId, r.CommentId });
    }
}