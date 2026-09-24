// Generic, opt-in browser collector. It only observes an RTCPeerConnection supplied by the host page.
// It never reads, changes, replays, or creates signaling messages.
window.ChatLabWebRtcTelemetry = (() => {
  const sum = (reports, field) => reports.reduce((total, report) => total + (Number(report[field]) || 0), 0);
  const nullableSum = (reports, field) => reports.some(report => report[field] != null) ? sum(reports, field) : null;
  const average = (reports, field) => {
    const values = reports.map(report => Number(report[field])).filter(Number.isFinite);
    return values.length ? values.reduce((a, b) => a + b, 0) / values.length : null;
  };
  const codecName = (report, byId) => {
    const codec = byId.get(report.codecId);
    return codec ? [codec.mimeType, codec.clockRate && `${codec.clockRate}Hz`, codec.channels && `${codec.channels}ch`].filter(Boolean).join(' ') : null;
  };

  async function snapshot(peerConnection, uiState = 'unknown') {
    const reports = Array.from((await peerConnection.getStats()).values());
    const byId = new Map(reports.map(report => [report.id, report]));
    const inbound = reports.filter(report => report.type === 'inbound-rtp' && !report.isRemote);
    const outbound = reports.filter(report => report.type === 'outbound-rtp' && !report.isRemote);
    const transport = reports.find(report => report.type === 'transport' && report.selectedCandidatePairId);
    const pair = byId.get(transport?.selectedCandidatePairId) || reports.find(report => report.type === 'candidate-pair' && report.nominated && report.state === 'succeeded');
    const local = pair && byId.get(pair.localCandidateId);
    const remote = pair && byId.get(pair.remoteCandidateId);
    const firstCodec = items => items.map(item => codecName(item, byId)).find(Boolean) || null;
    return {
      capturedAtUtc: new Date().toISOString(), connectionState: peerConnection.connectionState ?? 'unknown', iceConnectionState: peerConnection.iceConnectionState ?? 'unknown', signalingState: peerConnection.signalingState ?? 'unknown', uiState,
      bytesReceived: nullableSum(inbound, 'bytesReceived'), packetsReceived: nullableSum(inbound, 'packetsReceived'), packetsLost: nullableSum(inbound, 'packetsLost'), framesReceived: nullableSum(inbound, 'framesReceived'), framesDecoded: nullableSum(inbound, 'framesDecoded'), jitterSeconds: average(inbound, 'jitter'),
      bytesSent: nullableSum(outbound, 'bytesSent'), packetsSent: nullableSum(outbound, 'packetsSent'), framesEncoded: nullableSum(outbound, 'framesEncoded'), framesSent: nullableSum(outbound, 'framesSent'),
      currentRoundTripTimeSeconds: pair?.currentRoundTripTime ?? null, availableIncomingBitrateBps: pair?.availableIncomingBitrate ?? null, availableOutgoingBitrateBps: pair?.availableOutgoingBitrate ?? null,
      localCandidateType: local?.candidateType ?? null, remoteCandidateType: remote?.candidateType ?? null, selectedCandidatePairId: pair?.id ?? null, inboundCodec: firstCodec(inbound), outboundCodec: firstCodec(outbound)
    };
  }

  function start(peerConnection, { endpoint, intervalMs = 1000, uiState = 'unknown', onError = console.warn } = {}) {
    if (!peerConnection || typeof peerConnection.getStats !== 'function') throw new Error('An RTCPeerConnection is required.');
    if (!endpoint) throw new Error('A ChatLab telemetry endpoint is required.');
    let stopped = false;
    const send = async () => {
      if (stopped) return;
      try {
        const visibleUiState = typeof uiState === 'function' ? uiState() : uiState;
        const response = await fetch(endpoint, { method: 'POST', headers: { 'content-type': 'application/json' }, body: JSON.stringify(await snapshot(peerConnection, visibleUiState)) });
        if (!response.ok) throw new Error(`ChatLab telemetry upload failed (${response.status}).`);
      } catch (error) { onError(error); }
    };
    void send();
    const timer = window.setInterval(() => void send(), Math.max(1000, intervalMs));
    return () => { stopped = true; window.clearInterval(timer); };
  }

  return { start, snapshot };
})();
