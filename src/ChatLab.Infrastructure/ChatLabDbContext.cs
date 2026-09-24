using ChatLab.Domain;
using Microsoft.EntityFrameworkCore;

namespace ChatLab.Infrastructure;

public sealed class ChatLabDbContext(DbContextOptions<ChatLabDbContext> options) : DbContext(options)
{
    public DbSet<ResearchSession> ResearchSessions => Set<ResearchSession>();
    public DbSet<WebRtcSample> WebRtcSamples => Set<WebRtcSample>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ResearchSession>(entity =>
        {
            entity.ToTable("ResearchSessions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Label).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(4_000);
            entity.HasMany(x => x.WebRtcSamples).WithOne().HasForeignKey(x => x.ResearchSessionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WebRtcSample>(entity =>
        {
            entity.ToTable("WebRtcSamples");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ConnectionState).HasMaxLength(50).IsRequired();
            entity.Property(x => x.IceConnectionState).HasMaxLength(50).IsRequired();
            entity.HasIndex(x => new { x.ResearchSessionId, x.CapturedAtUtc });
        });
    }
}
