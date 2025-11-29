namespace EventScheduler.Tests;

/// <summary>
/// Test utilities for creating common test fixtures.
/// </summary>
public static class TestHelpers
{
    public static ScheduledEvent<MockTimeProvider> CreateTestEvent(
        DateTime scheduledTime,
        Action<MockTimeProvider>? runAction = null,
        TimeSpan warningBefore = default)
    {
        return new ScheduledEvent<MockTimeProvider>.Builder()
            .SetScheduledTime(scheduledTime)
            .SetRunAction(runAction ?? (_ => { }))
            .SetWarningBefore(warningBefore)
            .Build();
    }

    public static ScheduledEvent<MockTimeProvider> CreateTestEventWithWarning(
        DateTime scheduledTime,
        TimeSpan warningBefore,
        Action<MockTimeProvider>? runAction = null,
        Action<MockTimeProvider>? warningAction = null)
    {
        return new ScheduledEvent<MockTimeProvider>.Builder()
            .SetScheduledTime(scheduledTime)
            .SetWarningBefore(warningBefore)
            .SetRunAction(runAction ?? (_ => { }))
            .SetRunWarningAction(warningAction ?? (_ => { }))
            .Build();
    }
}
