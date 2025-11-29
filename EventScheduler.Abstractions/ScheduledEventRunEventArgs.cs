using EventScheduler.Abstractions;
using System;

namespace EventScheduler.Abstractions
{
    public class ScheduledEventRunEventArgs<TWorld> : EventArgs
        where TWorld : IWorldDateTimeProvider
    {
        public IScheduledEvent<TWorld> ScheduledEvent { get; private set; }
        public ScheduledEventRunEventArgs(IScheduledEvent<TWorld> scheduledEvent)
        {
            ScheduledEvent = scheduledEvent;
        }
    }
}
