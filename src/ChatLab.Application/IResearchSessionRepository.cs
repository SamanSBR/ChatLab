using ChatLab.Domain;

namespace ChatLab.Application;

public interface IResearchSessionRepository
{
    Task<IReadOnlyList<ResearchSession>> GetRecentAsync(int take, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task AddAsync(ResearchSession session, CancellationToken cancellationToken = default);
    Task<ResearchSession?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddSampleAsync(WebRtcSample sample, CancellationToken cancellationToken = default);
    Task<WebRtcSample?> GetLatestSampleAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WebRtcSample>> GetSamplesAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task AddAnomaliesAsync(IEnumerable<AnomalyEvent> anomalies, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AnomalyEvent>> GetAnomaliesAsync(Guid sessionId, CancellationToken cancellationToken = default);
}
