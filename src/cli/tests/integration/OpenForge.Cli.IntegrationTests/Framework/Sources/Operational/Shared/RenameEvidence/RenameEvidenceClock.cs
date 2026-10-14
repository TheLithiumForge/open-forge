namespace OpenForge.Cli.IntegrationTests.Framework.Sources.Operational.Shared.RenameEvidence;

internal sealed class RenameEvidenceClock : TimeProvider
{
    private readonly List<DeadlineTimer> _timers = [];
    private long _ticks;
    public override long TimestampFrequency => TimeSpan.TicksPerSecond;
    public override long GetTimestamp() => _ticks;
    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        var timer = new DeadlineTimer(callback, state, _ticks + dueTime.Ticks);
        _timers.Add(timer);
        return timer;
    }

    internal void Advance(TimeSpan elapsed)
    {
        _ticks += elapsed.Ticks;
        foreach (var timer in _timers.ToArray()) timer.Fire(_ticks);
    }

    private sealed class DeadlineTimer(TimerCallback callback, object? state, long due) : ITimer
    {
        private bool _disposed;
        internal void Fire(long now)
        {
            if (_disposed || now < due) return;
            _disposed = true;
            callback(state);
        }
        public bool Change(TimeSpan dueTime, TimeSpan period) => throw new NotSupportedException();
        public void Dispose() => _disposed = true;
        public ValueTask DisposeAsync() { Dispose(); return ValueTask.CompletedTask; }
    }
}
