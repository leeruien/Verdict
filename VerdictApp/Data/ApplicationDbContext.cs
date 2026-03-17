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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Dilemma>().ToTable("Dilemmas");
        builder.Entity<DilemmaOption>().ToTable("DilemmaOptions");
        builder.Entity<Vote>().ToTable("Votes");
        builder.Entity<Comment>().ToTable("Comments");

        builder.Entity<Vote>()
            .HasIndex(v => new { v.UserId, v.DilemmaOptionId })
            .IsUnique();
    }
}