using EventScheduler.Abstractions;

namespace EventScheduler.Tests;

/// <summary>
/// Mock time provider for testing purposes.
/// Allows controlling the current time for precise test scenarios.
/// </summary>
public class MockTimeProvider : IWorldDateTimeProvider
{
    public DateTime CurrentTime { get; set; }

    public MockTimeProvider(DateTime initialTime)
    {
        CurrentTime = initialTime;
    }
}
