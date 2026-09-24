using ChatLab.Domain;

namespace ChatLab.Application;

public interface IAnomalyDetectionService
{
    IReadOnlyList<AnomalyEvent> Detect(WebRtcSample current, IReadOnlyList<WebRtcSample> precedingSamples);
}
