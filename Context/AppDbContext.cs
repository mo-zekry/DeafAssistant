using DeafAssistant.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DeafAssistant.Context;

/// <summary>
/// Database context for the application
/// </summary>
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
  public AppDbContext(DbContextOptions<AppDbContext> options)
    : base(options) { }

  // DbSets for each model
  public required DbSet<Lesson> Lesson { get; set; }
  public required DbSet<Media> Media { get; set; }
  public required DbSet<Feedback> Feedback { get; set; }
  public required DbSet<Subscription> Subscription { get; set; }
  public required DbSet<UserRefreshToken> UserRefreshTokens { get; set; }

  /// <summary>
  /// Configure the model that was discovered by convention from the entity types
  /// </summary>
  /// <param name="builder">The ModelBuilder used to construct the model for this context</param>
  protected override void OnModelCreating(ModelBuilder builder)
  {
    base.OnModelCreating(builder);

    // Configure User's FirstName and LastName to be required
    builder.Entity<ApplicationUser>().Property(u => u.FirstName).IsRequired().HasMaxLength(50);

    builder.Entity<ApplicationUser>().Property(u => u.LastName).IsRequired().HasMaxLength(50);

    // Configure UserRefreshTokens
    builder
      .Entity<UserRefreshToken>()
      .HasOne(rt => rt.User)
      .WithMany()
      .HasForeignKey(rt => rt.UserId)
      .OnDelete(DeleteBehavior.Cascade);

    // Configure Subscription
    builder.Entity<Subscription>().Property(s => s.Price).HasColumnType("decimal(18,2)");

    // Configure Media with proper relationships
    builder
      .Entity<Media>()
      .HasOne(m => m.Lesson)
      .WithMany(l => l.MediaResources)
      .HasForeignKey(m => m.LessonId)
      .OnDelete(DeleteBehavior.SetNull)
      .IsRequired(false);

    builder
      .Entity<Media>()
      .HasOne(m => m.User)
      .WithMany()
      .HasForeignKey(m => m.UserId)
      .OnDelete(DeleteBehavior.SetNull)
      .IsRequired(false);

    // Configure Feedback with proper relationships
    builder
      .Entity<Feedback>()
      .HasOne(f => f.User)
      .WithMany()
      .HasForeignKey(f => f.UserId)
      .OnDelete(DeleteBehavior.Cascade);

    builder
      .Entity<Feedback>()
      .HasOne(f => f.Lesson)
      .WithMany()
      .HasForeignKey(f => f.LessonId)
      .OnDelete(DeleteBehavior.SetNull)
      .IsRequired(false);

    // Configure proper table names (singular form)
    builder.Entity<Lesson>().ToTable("Lesson");
    builder.Entity<Media>().ToTable("Media");
    builder.Entity<Feedback>().ToTable("Feedback");
    builder.Entity<Subscription>().ToTable("Subscription");
    builder.Entity<UserRefreshToken>().ToTable("UserRefreshToken");
  }

  /// <summary>
  /// Configures the database connection string
  /// </summary>
  /// <param name="optionsBuilder">The DbContextOptionsBuilder used to configure the context</param>
  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    var connectionString = new ConfigurationBuilder()
      .SetBasePath(AppContext.BaseDirectory)
      .AddJsonFile("appsettings.json")
      .Build()
      .GetConnectionString("DefaultConnection");

    if (!optionsBuilder.IsConfigured)
    {
      optionsBuilder.UseSqlServer(connectionString);
    }
  }
}
