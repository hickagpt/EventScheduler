using EventScheduler.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventScheduler
{
    public class EventScheduler<TWorld> : IEventScheduler<TWorld>
        where TWorld : IWorldDateTimeProvider
    {
        List<IScheduledEvent<TWorld>> _scheduledEvents = new List<IScheduledEvent<TWorld>>();

        public event EventHandler<ScheduledEventRunEventArgs<TWorld>>? ScheduledEventRun;

        public bool CancelEvent(Guid scheduledEventId)
        {
            var scheduledEvent = GetScheduledEvent(scheduledEventId);
            if (scheduledEvent != null)
            {
                _scheduledEvents.Remove(scheduledEvent);
                return true;
            }
            return false;
        }

        public IScheduledEvent<TWorld>? GetScheduledEvent(Guid scheduledEventId)
        {
            return _scheduledEvents.Find(e => e.Id == scheduledEventId);
        }

        public IEnumerable<IScheduledEvent<TWorld>> GetScheduledEvents()
        {
            return _scheduledEvents;
        }

        public void RescheduleEvent(Guid scheduledEventId, DateTime newScheduledTime)
        {
            var scheduledEvent = GetScheduledEvent(scheduledEventId);
            if (scheduledEvent != null)
            {
                _scheduledEvents.Remove(scheduledEvent);

                ScheduleEvent(new ScheduledEvent<TWorld>.Builder(scheduledEvent.Id)
                    .Reschedule(scheduledEvent)
                    .SetScheduledTime(newScheduledTime)
                    .Build());
            }
        }

        public Guid ScheduleEvent(IScheduledEvent<TWorld> scheduledEvent)
        {
            for(var i = 0; i < _scheduledEvents.Count; i++)
            {
                if(_scheduledEvents[i].ScheduledTime > scheduledEvent.ScheduledTime)
                {
                    _scheduledEvents.Insert(i, scheduledEvent);

                    return scheduledEvent.Id;
                }
            }

            // If we reach here, the event is scheduled for the latest time
            _scheduledEvents.Add(scheduledEvent);

            return scheduledEvent.Id;
        }

        public void Update(TWorld world)
        {
            var warningBatch = _scheduledEvents.Where(e => !e.IsWarningSent && e.IsWarningDue(world)).ToList();

            var executeBatch = _scheduledEvents.Where(e => e.IsDue(world)).ToList();

            foreach (var scheduledEvent in warningBatch)
            {
                scheduledEvent.RunWarning(world);
            }

            // Remove executed events from the list after processing all batches
            foreach (var scheduledEvent in executeBatch)
            {
                scheduledEvent.Run(world);
                _scheduledEvents.Remove(scheduledEvent);
                ScheduledEventRun?.Invoke(this, new ScheduledEventRunEventArgs<TWorld>(scheduledEvent));
            }
        }
    }
}
