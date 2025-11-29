using System;
using System.Collections.Generic;

namespace EventScheduler.Abstractions
{
    public interface IEventScheduler<TWorld> 
        where TWorld : IWorldDateTimeProvider
    {
        event EventHandler<ScheduledEventRunEventArgs<TWorld>>? ScheduledEventRun;

        Guid ScheduleEvent(IScheduledEvent<TWorld> scheduledEvent);
        void RescheduleEvent(Guid scheduledEventId, DateTime newScheduledTime);
        IEnumerable<IScheduledEvent<TWorld>> GetScheduledEvents();
        IScheduledEvent<TWorld>? GetScheduledEvent(Guid scheduledEventId);
        bool CancelEvent(Guid scheduledEventId);

        void Update(TWorld world);
    }
}
