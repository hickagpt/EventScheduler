using System;

namespace EventScheduler.Abstractions
{
    public interface IScheduledEvent<TWorld>
        where TWorld : IWorldDateTimeProvider
    {
        Guid Id { get; }
        string? Name { get; }
        string? Description { get; }
        DateTime ScheduledTime { get; }
        TimeSpan WarningBefore { get; }
        bool HasWarning { get; }
        bool IsWarningSent { get; }

        bool IsDue(TWorld dateTimeProvider);
        bool IsWarningDue(TWorld dateTimeProvider);

        void Run(TWorld world);
        void RunWarning(TWorld world);
    }
}
