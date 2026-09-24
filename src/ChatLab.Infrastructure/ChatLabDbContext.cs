using ChatLab.Domain;
using Microsoft.EntityFrameworkCore;

namespace ChatLab.Infrastructure;

public sealed class ChatLabDbContext(DbContextOptions<ChatLabDbContext> options) : DbContext(options)
{
    public DbSet<ResearchSession> ResearchSessions => Set<ResearchSession>();
    public DbSet<WebRtcSample> WebRtcSamples => Set<WebRtcSample>();
    public DbSet<AnomalyEvent> AnomalyEvents => Set<AnomalyEvent>();

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
            entity.Property(x => x.SignalingState).HasMaxLength(50).IsRequired();
            entity.Property(x => x.UiState).HasMaxLength(50).IsRequired();
            entity.Property(x => x.InboundCodec).HasMaxLength(200);
            entity.Property(x => x.OutboundCodec).HasMaxLength(200);
            entity.Property(x => x.LocalCandidateType).HasMaxLength(50);
            entity.Property(x => x.RemoteCandidateType).HasMaxLength(50);
            entity.Property(x => x.SelectedCandidatePairId).HasMaxLength(200);
            entity.HasIndex(x => new { x.ResearchSessionId, x.CapturedAtUtc });
        });

        modelBuilder.Entity<AnomalyEvent>(entity =>
        {
            entity.ToTable("AnomalyEvents");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Type).HasConversion<string>().HasMaxLength(100).IsRequired();
            entity.Property(x => x.Severity).HasConversion<string>().HasMaxLength(50).IsRequired();
            entity.Property(x => x.Evidence).HasMaxLength(2_000).IsRequired();
            entity.HasIndex(x => new { x.ResearchSessionId, x.OccurredAtUtc });
            entity.HasIndex(x => new { x.WebRtcSampleId, x.Type }).IsUnique();
        });
    }

    /// <summary>Safely extends databases created by the Phase 1 EnsureCreated baseline.</summary>
    public async Task EnsureTelemetrySchemaAsync(CancellationToken cancellationToken = default)
    {
        var connection = Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);
        try
        {
            await using var columnsCommand = connection.CreateCommand();
            columnsCommand.CommandText = "PRAGMA table_info(\"WebRtcSamples\");";
            var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            await using var reader = await columnsCommand.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken)) columns.Add(reader.GetString(1));

            var additions = new Dictionary<string, string>
            {
                ["SignalingState"] = "TEXT NOT NULL DEFAULT 'unknown'",
                ["PacketsReceived"] = "INTEGER NULL",
                ["FramesReceived"] = "INTEGER NULL",
                ["FramesDecoded"] = "INTEGER NULL",
                ["PacketsSent"] = "INTEGER NULL",
                ["FramesEncoded"] = "INTEGER NULL",
                ["FramesSent"] = "INTEGER NULL",
                ["AvailableIncomingBitrateKbps"] = "REAL NULL",
                ["AvailableOutgoingBitrateKbps"] = "REAL NULL",
                ["SelectedCandidatePairId"] = "TEXT NULL"
                ,["UiState"] = "TEXT NOT NULL DEFAULT 'unknown'"
            };

            foreach (var (column, definition) in additions.Where(x => !columns.Contains(x.Key)))
            {
                await using var alterCommand = connection.CreateCommand();
                alterCommand.CommandText = $"ALTER TABLE \"WebRtcSamples\" ADD COLUMN \"{column}\" {definition};";
                await alterCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            await using var anomalyCommand = connection.CreateCommand();
            anomalyCommand.CommandText = """
                CREATE TABLE IF NOT EXISTS \"AnomalyEvents\" (
                    \"Id\" TEXT NOT NULL CONSTRAINT \"PK_AnomalyEvents\" PRIMARY KEY,
                    \"ResearchSessionId\" TEXT NOT NULL,
                    \"WebRtcSampleId\" TEXT NOT NULL,
                    \"OccurredAtUtc\" TEXT NOT NULL,
                    \"Type\" TEXT NOT NULL,
                    \"Severity\" TEXT NOT NULL,
                    \"Evidence\" TEXT NOT NULL);
                CREATE UNIQUE INDEX IF NOT EXISTS \"IX_AnomalyEvents_WebRtcSampleId_Type\" ON \"AnomalyEvents\" (\"WebRtcSampleId\", \"Type\");
                CREATE INDEX IF NOT EXISTS \"IX_AnomalyEvents_ResearchSessionId_OccurredAtUtc\" ON \"AnomalyEvents\" (\"ResearchSessionId\", \"OccurredAtUtc\");
                """;
            await anomalyCommand.ExecuteNonQueryAsync(cancellationToken);
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
}
