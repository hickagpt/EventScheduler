using EventScheduler.Abstractions;
using System;

namespace EventScheduler
{
    public class ScheduledEvent<TWorld> : IScheduledEvent<TWorld>
        where TWorld : IWorldDateTimeProvider
    {
        public Guid Id { get; private set; }
        public string? Name { get; private set; }
        public string? Description { get; private set; }
        public DateTime ScheduledTime { get; private set; }
        public TimeSpan WarningBefore { get; private set; }
        public bool HasWarning => WarningBefore > TimeSpan.Zero;

        public bool IsWarningSent => _warningSent;

        private bool _warningSent = false;
        Action<TWorld>? _runAction;
        Action<TWorld>? _runWarningAction;

        public bool IsDue(TWorld dateTimeProvider) => dateTimeProvider.CurrentTime >= ScheduledTime;

        public void Run(TWorld world) => _runAction?.Invoke(world);

        public void RunWarning(TWorld world)
        {
            _runWarningAction?.Invoke(world);
            _warningSent = true;
        }

        public bool IsWarningDue(TWorld dateTimeProvider)=>HasWarning ? dateTimeProvider.CurrentTime >= ScheduledTime - WarningBefore : false;
        

        ScheduledEvent() { }

        public class Builder
        {
            private readonly ScheduledEvent<TWorld> _scheduledEvent = new ScheduledEvent<TWorld>();

            public Builder()
            {
                _scheduledEvent.Id = Guid.NewGuid();
            }

            public Builder(Guid id)
            {
                _scheduledEvent.Id = id;
            }

            public Builder SetName(string? name)
            {
                _scheduledEvent.Name = name;
                return this;
            }

            public Builder SetDescription(string? description)
            {
                _scheduledEvent.Description = description;
                return this;
            }
            public Builder SetScheduledTime(DateTime scheduledTime)
            {
                _scheduledEvent.ScheduledTime = scheduledTime;
                return this;
            }
            public Builder SetWarningBefore(TimeSpan warningBefore)
            {
                _scheduledEvent.WarningBefore = warningBefore;
                return this;
            }
            
            public Builder SetRunAction(Action<TWorld> runAction)
            {
                _scheduledEvent._runAction = runAction;
                return this;
            }
            public Builder SetRunWarningAction(Action<TWorld> runWarningAction)
            {
                _scheduledEvent._runWarningAction = runWarningAction;
                return this;
            }

            public Builder Reschedule(IScheduledEvent<TWorld> scheduledEvent)
            {
                if(!(scheduledEvent is ScheduledEvent<TWorld> se))
                {
                    throw new ArgumentException("scheduledEvent must be of type ScheduledEvent<TWorld>", nameof(scheduledEvent));
                }

                _scheduledEvent.Name = se.Name;
                _scheduledEvent.Description = se.Description;
                _scheduledEvent.ScheduledTime = se.ScheduledTime;
                _scheduledEvent.WarningBefore = se.WarningBefore;
                _scheduledEvent._runAction = se._runAction;
                _scheduledEvent._runWarningAction = se._runWarningAction;

                return this;
            }
            public ScheduledEvent<TWorld> Build()
            {
                return _scheduledEvent;
            }
        }
    }
}
