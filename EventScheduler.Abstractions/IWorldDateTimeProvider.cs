using System;

namespace EventScheduler.Abstractions
{
    public interface IWorldDateTimeProvider
    {
        DateTime CurrentTime { get; }
    }
}
