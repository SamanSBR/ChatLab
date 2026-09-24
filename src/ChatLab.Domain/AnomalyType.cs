namespace ChatLab.Domain;

/// <summary>Deterministic technical observations that may warrant session review.</summary>
public enum AnomalyType
{
    MediaWhileUiSearching,
    MediaWhileUiDisconnected,
    IceRestart,
    EarlyMedia
}
