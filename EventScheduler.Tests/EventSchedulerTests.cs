using EventScheduler;
using EventScheduler.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace EventScheduler.Tests;

public class EventSchedulerTests
{
    private readonly MockTimeProvider _timeProvider;
    private readonly EventScheduler<MockTimeProvider> _scheduler;

    public EventSchedulerTests()
    {
        _timeProvider = new MockTimeProvider(new DateTime(2025, 1, 1, 12, 0, 0));
        _scheduler = new EventScheduler<MockTimeProvider>();
    }

    #region Constructor and Basic Setup Tests

    [Fact]
    public void Constructor_CreatesEmptyScheduler()
    {
        // Arrange & Act
        var scheduler = new EventScheduler<MockTimeProvider>();

        // Assert
        Assert.Empty(scheduler.GetScheduledEvents());
    }

    #endregion

    #region ScheduleEvent Tests

    [Fact]
    public void ScheduleEvent_SingleEvent_AddsToScheduler()
    {
        // Arrange
        var scheduledTime = _timeProvider.CurrentTime.AddMinutes(30);
        var testEvent = TestHelpers.CreateTestEvent(scheduledTime);

        // Act
        var eventId = _scheduler.ScheduleEvent(testEvent);

        // Assert
        Assert.Equal(testEvent.Id, eventId);
        var scheduledEvents = _scheduler.GetScheduledEvents();
        Assert.Single(scheduledEvents);
        Assert.Equal(testEvent.Id, scheduledEvents.First().Id);
    }

    [Fact]
    public void ScheduleEvent_MultipleEvents_InOrderByScheduledTime()
    {
        // Arrange
        var earlyEvent = TestHelpers.CreateTestEvent(_timeProvider.CurrentTime.AddMinutes(10));
        var middleEvent = TestHelpers.CreateTestEvent(_timeProvider.CurrentTime.AddMinutes(20));
        var lateEvent = TestHelpers.CreateTestEvent(_timeProvider.CurrentTime.AddMinutes(30));

        // Act
        _scheduler.ScheduleEvent(lateEvent);
        _scheduler.ScheduleEvent(earlyEvent);
        _scheduler.ScheduleEvent(middleEvent);

        // Assert
        var scheduledEvents = _scheduler.GetScheduledEvents().ToList();
        Assert.Equal(3, scheduledEvents.Count);
        Assert.Equal(earlyEvent.Id, scheduledEvents[0].Id);
        Assert.Equal(middleEvent.Id, scheduledEvents[1].Id);
        Assert.Equal(lateEvent.Id, scheduledEvents[2].Id);
    }

    [Fact]
    public void ScheduleEvent_AtEndOfList_AddsToEnd()
    {
        // Arrange
        var earlyEvent = TestHelpers.CreateTestEvent(_timeProvider.CurrentTime.AddMinutes(10));
        var lateEvent = TestHelpers.CreateTestEvent(_timeProvider.CurrentTime.AddMinutes(30));

        // Act
        _scheduler.ScheduleEvent(earlyEvent);
        _scheduler.ScheduleEvent(lateEvent);

        // Assert
        var scheduledEvents = _scheduler.GetScheduledEvents().ToList();
        Assert.Equal(2, scheduledEvents.Count);
        Assert.Equal(earlyEvent.Id, scheduledEvents[0].Id);
        Assert.Equal(lateEvent.Id, scheduledEvents[1].Id);
    }

    [Fact]
    public void ScheduleEvent_AtBeginningOfList_InsertsAtBeginning()
    {
        // Arrange
        var lateEvent = TestHelpers.CreateTestEvent(_timeProvider.CurrentTime.AddMinutes(30));
        var earlyEvent = TestHelpers.CreateTestEvent(_timeProvider.CurrentTime.AddMinutes(10));

        // Act
        _scheduler.ScheduleEvent(lateEvent);
        _scheduler.ScheduleEvent(earlyEvent);

        // Assert
        var scheduledEvents = _scheduler.GetScheduledEvents().ToList();
        Assert.Equal(2, scheduledEvents.Count);
        Assert.Equal(earlyEvent.Id, scheduledEvents[0].Id);
        Assert.Equal(lateEvent.Id, scheduledEvents[1].Id);
    }

    #endregion

    #region GetScheduledEvent Tests

    [Fact]
    public void GetScheduledEvent_ExistingEvent_ReturnsEvent()
    {
        // Arrange
        var testEvent = TestHelpers.CreateTestEvent(_timeProvider.CurrentTime.AddMinutes(30));
        var eventId = _scheduler.ScheduleEvent(testEvent);

        // Act
        var retrievedEvent = _scheduler.GetScheduledEvent(eventId);

        // Assert
        Assert.NotNull(retrievedEvent);
        Assert.Equal(eventId, retrievedEvent!.Id);
    }

    [Fact]
    public void GetScheduledEvent_NonExistingEvent_ReturnsNull()
    {
        // Act
        var retrievedEvent = _scheduler.GetScheduledEvent(Guid.NewGuid());

        // Assert
        Assert.Null(retrievedEvent);
    }

    #endregion

    #region GetScheduledEvents Tests

    [Fact]
    public void GetScheduledEvents_EmptyScheduler_ReturnsEmptyCollection()
    {
        // Act
        var events = _scheduler.GetScheduledEvents();

        // Assert
        Assert.Empty(events);
    }

    [Fact]
    public void GetScheduledEvents_WithEvents_ReturnsAllEvents()
    {
        // Arrange
        var event1 = TestHelpers.CreateTestEvent(_timeProvider.CurrentTime.AddMinutes(10));
        var event2 = TestHelpers.CreateTestEvent(_timeProvider.CurrentTime.AddMinutes(20));

        _scheduler.ScheduleEvent(event1);
        _scheduler.ScheduleEvent(event2);

        // Act
        var events = _scheduler.GetScheduledEvents().ToList();

        // Assert
        Assert.Equal(2, events.Count);
        Assert.Contains(events, e => e.Id == event1.Id);
        Assert.Contains(events, e => e.Id == event2.Id);
    }

    #endregion

    #region CancelEvent Tests

    [Fact]
    public void CancelEvent_ExistingEvent_ReturnsTrueAndRemovesEvent()
    {
        // Arrange
        var testEvent = TestHelpers.CreateTestEvent(_timeProvider.CurrentTime.AddMinutes(30));
        var eventId = _scheduler.ScheduleEvent(testEvent);
        Assert.Single(_scheduler.GetScheduledEvents());

        // Act
        var result = _scheduler.CancelEvent(eventId);

        // Assert
        Assert.True(result);
        Assert.Empty(_scheduler.GetScheduledEvents());
        Assert.Null(_scheduler.GetScheduledEvent(eventId));
    }

    [Fact]
    public void CancelEvent_NonExistingEvent_ReturnsFalse()
    {
        // Act
        var result = _scheduler.CancelEvent(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    #endregion

    #region RescheduleEvent Tests

    [Fact]
    public void RescheduleEvent_ExistingEvent_UpdatesScheduledTimeAndReorders()
    {
        // Arrange
        var originalTime = _timeProvider.CurrentTime.AddMinutes(30);
        var testEvent = TestHelpers.CreateTestEvent(originalTime);
        var eventId = _scheduler.ScheduleEvent(testEvent);

        var newTime = _timeProvider.CurrentTime.AddMinutes(10);

        // Act
        _scheduler.RescheduleEvent(eventId, newTime);

        // Assert
        var updatedEvent = _scheduler.GetScheduledEvent(eventId);
        Assert.NotNull(updatedEvent);
        Assert.Equal(newTime, updatedEvent!.ScheduledTime);
    }

    [Fact]
    public void RescheduleEvent_NonExistingEvent_DoesNothing()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        var originalEvent = TestHelpers.CreateTestEvent(_timeProvider.CurrentTime.AddMinutes(30));
        _scheduler.ScheduleEvent(originalEvent);

        // Act
        _scheduler.RescheduleEvent(nonExistingId, _timeProvider.CurrentTime.AddMinutes(20));

        // Assert
        var events = _scheduler.GetScheduledEvents();
        Assert.Single(events);
        Assert.Equal(originalEvent.Id, events.First().Id);
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_NoDueEvents_NoEventsExecuted()
    {
        // Arrange
        var futureTime = _timeProvider.CurrentTime.AddMinutes(30);
        var executed = false;
        var testEvent = TestHelpers.CreateTestEvent(futureTime, _ => executed = true);
        _scheduler.ScheduleEvent(testEvent);

        // Act
        _scheduler.Update(_timeProvider);

        // Assert
        Assert.False(executed);
        Assert.Single(_scheduler.GetScheduledEvents());
    }

    [Fact]
    public void Update_DueEvents_ExecutesAndRemovesEvents()
    {
        // Arrange
        var pastTime = _timeProvider.CurrentTime.AddMinutes(-30);
        var executed = false;
        var testEvent = TestHelpers.CreateTestEvent(pastTime, _ => executed = true);
        _scheduler.ScheduleEvent(testEvent);

        // Act
        _scheduler.Update(_timeProvider);

        // Assert
        Assert.True(executed);
        Assert.Empty(_scheduler.GetScheduledEvents());
    }

    [Fact]
    public void Update_MultipleDueAndNotDue_EventsExecutedCorrectly()
    {
        // Arrange
        var pastTime = _timeProvider.CurrentTime.AddMinutes(-30);
        var futureTime = _timeProvider.CurrentTime.AddMinutes(30);

        var executedCount = 0;
        var pastEvent = TestHelpers.CreateTestEvent(pastTime, _ => executedCount++);
        var futureEvent = TestHelpers.CreateTestEvent(futureTime, _ => executedCount++);

        _scheduler.ScheduleEvent(futureEvent);
        _scheduler.ScheduleEvent(pastEvent);

        // Act
        _scheduler.Update(_timeProvider);

        // Assert
        Assert.Equal(1, executedCount); // Only past event should execute
        var remainingEvents = _scheduler.GetScheduledEvents();
        Assert.Single(remainingEvents);
        Assert.Equal(futureEvent.Id, remainingEvents.First().Id);
    }

    [Fact]
    public void Update_EventWithWarning_WarningSent()
    {
        // Arrange
        var warningTime = _timeProvider.CurrentTime.AddMinutes(-5);
        var executionTime = warningTime.AddMinutes(15);
        var warningBefore = TimeSpan.FromMinutes(10); // Warning 10 mins before execution

        var warningSent = false;
        var testEvent = TestHelpers.CreateTestEventWithWarning(
            executionTime,
            warningBefore,
            _ => { }, // No execution action needed for this test
            _ => warningSent = true);

        _scheduler.ScheduleEvent(testEvent);

        // Current time should trigger warning (between warning time and execution time)
        _timeProvider.CurrentTime = warningTime.AddMinutes(5);

        // Act
        _scheduler.Update(_timeProvider);

        // Assert
        Assert.True(warningSent);
        Assert.Single(_scheduler.GetScheduledEvents()); // Event should still be scheduled

        var scheduledEvent = _scheduler.GetScheduledEvent(testEvent.Id);
        Assert.True(scheduledEvent!.IsWarningSent);
    }

    [Fact]
    public void Update_EventExecuted_TriggersEvent()
    {
        // Arrange
        var eventExecuted = false;
        var executionTime = _timeProvider.CurrentTime.AddMinutes(-10);
        var testEvent = TestHelpers.CreateTestEvent(executionTime, _ => eventExecuted = true);
        _scheduler.ScheduleEvent(testEvent);

        ScheduledEventRunEventArgs<MockTimeProvider>? eventArgs = null;
        _scheduler.ScheduledEventRun += (sender, args) => eventArgs = args;

        // Act
        _timeProvider.CurrentTime = executionTime.AddMinutes(1);
        _scheduler.Update(_timeProvider);

        // Assert
        Assert.True(eventExecuted);
        Assert.NotNull(eventArgs);
        Assert.Equal(testEvent.Id, eventArgs!.ScheduledEvent.Id);
        Assert.Equal(executionTime, eventArgs.ScheduledEvent.ScheduledTime);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void FullEventLifecycle_ScheduleExecuteCancel()
    {
        // Arrange - Schedule event
        var executionTime = _timeProvider.CurrentTime.AddMinutes(10);
        var executed = false;
        var testEvent = TestHelpers.CreateTestEvent(executionTime, _ => executed = true);
        var eventId = _scheduler.ScheduleEvent(testEvent);

        Assert.Single(_scheduler.GetScheduledEvents());

        // Act - Try to execute too early
        _scheduler.Update(_timeProvider);
        Assert.False(executed);

        // Act - Cancel the event
        var cancelResult = _scheduler.CancelEvent(eventId);
        Assert.True(cancelResult);
        Assert.Empty(_scheduler.GetScheduledEvents());

        // Act - Try to execute after cancel (should not execute)
        _timeProvider.CurrentTime = executionTime;
        _scheduler.Update(_timeProvider);
        Assert.False(executed);
    }

    [Fact]
    public void ComplexScenario_MultipleEventsWithWarnings()
    {
        // Arrange - Create fresh instances to avoid interference from other tests
        var timeProvider = new MockTimeProvider(new DateTime(2025, 1, 1, 12, 0, 0));
        var scheduler = new EventScheduler<MockTimeProvider>();
        var now = timeProvider.CurrentTime;

        // Event 1: Due immediately
        var exec1 = false;
        var warn1 = false;
        var event1 = TestHelpers.CreateTestEventWithWarning(
            now.AddMinutes(-10),
            TimeSpan.FromMinutes(5),
            _ => exec1 = true,
            _ => warn1 = true);

        // Event 2: Warning due now, execution in future
        var exec2 = false;
        var warn2 = false;
        var event2Time = now.AddMinutes(5); // Schedule 5 mins from now, warning 5 mins before = now
        var event2 = TestHelpers.CreateTestEventWithWarning(
            event2Time,
            TimeSpan.FromMinutes(5),
            _ => exec2 = true,
            _ => warn2 = true);

        // Event 3: All in future
        var exec3 = false;
        var warn3 = false;
        var event3 = TestHelpers.CreateTestEventWithWarning(
            now.AddMinutes(30),
            TimeSpan.FromMinutes(5),
            _ => exec3 = true,
            _ => warn3 = true);

        // Schedule out of order to test sorting
        scheduler.ScheduleEvent(event3);
        scheduler.ScheduleEvent(event1);
        scheduler.ScheduleEvent(event2);

        // Act - First update (should execute event1, send warning for event2)
        var eventsExecuted = new List<string>();
        scheduler.ScheduledEventRun += (sender, args) => eventsExecuted.Add(args.ScheduledEvent.Name ?? "unnamed");

        scheduler.Update(timeProvider);

        // Assert - Event1 executed, Event2 warning sent, Event3 unchanged
        Assert.True(exec1);
        Assert.True(warn2);
        Assert.False(exec3);
        Assert.False(warn3);

        var remainingEvents = scheduler.GetScheduledEvents().ToList();
        Assert.Equal(2, remainingEvents.Count);

        // Event2 warning should be sent, event should stay scheduled
        var scheduledEvent2 = remainingEvents.First(e => e.Id == event2.Id);
        Assert.True(scheduledEvent2.IsWarningSent);

        // Act - Second update after event2 execution time
        timeProvider.CurrentTime = event2Time.AddMinutes(1);
        scheduler.Update(timeProvider);

        // Assert - Event2 now executed
        Assert.True(exec2);
        Assert.Single(scheduler.GetScheduledEvents());
        Assert.Equal(event3.Id, scheduler.GetScheduledEvents().First().Id);
    }

    #endregion
}

public class ScheduledEventTests
{
    [Fact]
    public void Builder_BuildsBasicEvent()
    {
        // Arrange & Act
        var scheduledTime = new DateTime(2025, 1, 1, 12, 30, 0);
        var executed = false;
        var testEvent = new ScheduledEvent<MockTimeProvider>.Builder()
            .SetScheduledTime(scheduledTime)
            .SetName("Test Event")
            .SetDescription("Test Description")
            .SetRunAction(_ => executed = true)
            .Build();

        // Assert
        Assert.Equal(scheduledTime, testEvent.ScheduledTime);
        Assert.Equal("Test Event", testEvent.Name);
        Assert.Equal("Test Description", testEvent.Description);
        Assert.NotEqual(Guid.Empty, testEvent.Id);
    }

    [Fact]
    public void Builder_WithWarning_SetsWarningProperties()
    {
        // Arrange
        var scheduledTime = new DateTime(2025, 1, 1, 12, 30, 0);
        var warningBefore = TimeSpan.FromMinutes(10);

        // Act
        var testEvent = new ScheduledEvent<MockTimeProvider>.Builder()
            .SetScheduledTime(scheduledTime)
            .SetWarningBefore(warningBefore)
            .SetRunAction(_ => { })
            .SetRunWarningAction(_ => { })
            .Build();

        // Assert
        Assert.True(testEvent.HasWarning);
        Assert.Equal(warningBefore, testEvent.WarningBefore);
    }

    [Fact]
    public void IsDue_DefaultLogic_WorksCorrectly()
    {
        // Arrange
        var world = new MockTimeProvider(new DateTime(2025, 1, 1, 12, 0, 0));
        var pastTime = world.CurrentTime.AddMinutes(-10);
        var futureTime = world.CurrentTime.AddMinutes(10);

        var pastEvent = TestHelpers.CreateTestEvent(pastTime);
        var futureEvent = TestHelpers.CreateTestEvent(futureTime);

        // Act & Assert
        Assert.True(pastEvent.IsDue(world));
        Assert.False(futureEvent.IsDue(world));
    }

    [Fact]
    public void IsWarningDue_NoWarning_ReturnsFalse()
    {
        // Arrange
        var world = new MockTimeProvider(new DateTime(2025, 1, 1, 12, 0, 0));
        var eventTime = world.CurrentTime.AddMinutes(10);
        var eventWithoutWarning = TestHelpers.CreateTestEvent(eventTime);

        // Act & Assert
        Assert.False(eventWithoutWarning.IsWarningDue(world));
    }

    [Fact]
    public void IsWarningDue_WithWarning_BeforeWarningPeriod_ReturnsFalse()
    {
        // Arrange
        var world = new MockTimeProvider(new DateTime(2025, 1, 1, 12, 0, 0));
        var eventTime = world.CurrentTime.AddMinutes(20);
        var warningBefore = TimeSpan.FromMinutes(10);
        var eventWithWarning = TestHelpers.CreateTestEventWithWarning(eventTime, warningBefore);

        // Current time is 20 minutes before execution, but warning is only 10 minutes before
        world.CurrentTime = eventTime.AddMinutes(-15);

        // Act & Assert
        Assert.False(eventWithWarning.IsWarningDue(world));
    }

        [Fact]
    public void IsWarningDue_WithWarning_DuringWarningPeriod_ReturnsTrue()
    {
        // Arrange
        var world = new MockTimeProvider(new DateTime(2025, 1, 1, 12, 0, 0));
        var eventTime = world.CurrentTime.AddMinutes(20);
        var warningBefore = TimeSpan.FromMinutes(10);
        var eventWithWarning = TestHelpers.CreateTestEventWithWarning(eventTime, warningBefore);

        // Current time is during warning period (10 minutes before execution)
        world.CurrentTime = eventTime.AddMinutes(-5);

        // Act & Assert
        Assert.True(eventWithWarning.IsWarningDue(world));
    }

    [Fact]
    public void Debug_EventWithWarning_BothWarningAndExecutionShouldWork()
    {
        // Arrange
        var timeProvider = new MockTimeProvider(new DateTime(2025, 1, 1, 12, 0, 0));
        var scheduler = new EventScheduler<MockTimeProvider>();
        var executed = false;
        var warned = false;

        // Event scheduled 10 mins ago, warning 5 mins before execution
        // So warning should happen at T-15, execution at T-10
        // Current time is T, so both should happen
        var pastEvent = TestHelpers.CreateTestEventWithWarning(
            timeProvider.CurrentTime.AddMinutes(-10), // execution time 10 mins ago
            TimeSpan.FromMinutes(5), // warning 5 mins before = 15 mins ago
            _ => executed = true,
            _ => warned = true);

        // Act
        scheduler.ScheduleEvent(pastEvent);
        scheduler.Update(timeProvider);

        // Assert
        var eventsLeft = scheduler.GetScheduledEvents().ToList();
        Assert.Equal(0, eventsLeft.Count); // Event should be executed and removed
        Assert.True(executed);
        Assert.True(warned);
    }

    [Fact]
    public void RunWarning_ExecutesWarningAction()
    {
        // Arrange
        var world = new MockTimeProvider(new DateTime(2025, 1, 1, 12, 0, 0));
        var warningExecuted = false;
        var testEvent = TestHelpers.CreateTestEventWithWarning(
            world.CurrentTime.AddMinutes(10),
            TimeSpan.FromMinutes(5),
            _ => { },
            _ => warningExecuted = true);

        // Act
        testEvent.RunWarning(world);

        // Assert
        Assert.True(warningExecuted);
        Assert.True(testEvent.IsWarningSent);
    }

    [Fact]
    public void RunWarning_NoWarningAction_StillSetsWarningSent()
    {
        // Arrange
        var world = new MockTimeProvider(new DateTime(2025, 1, 1, 12, 0, 0));
        var testEvent = TestHelpers.CreateTestEvent(world.CurrentTime.AddMinutes(10));

        Assert.False(testEvent.IsWarningSent);

        // Act
        testEvent.RunWarning(world);

        // Assert
        Assert.True(testEvent.IsWarningSent);
    }

    [Fact]
    public void Builder_Reschedule_CopiesPropertiesCorrectly()
    {
        // Arrange
        var originalTime = new DateTime(2025, 1, 1, 12, 0, 0);
        var originalEvent = new ScheduledEvent<MockTimeProvider>.Builder()
            .SetScheduledTime(originalTime)
            .SetName("Original Name")
            .SetDescription("Original Description")
            .SetWarningBefore(TimeSpan.FromMinutes(5))
            .SetRunAction(_ => { })
            .SetRunWarningAction(_ => { })
            .Build();

        // Act
        var rescheduledEvent = new ScheduledEvent<MockTimeProvider>.Builder(
            originalEvent.Id)
            .Reschedule(originalEvent)
            .Build();

        // Assert
        Assert.Equal(originalEvent.Id, rescheduledEvent.Id);
        Assert.Equal(originalEvent.Name, rescheduledEvent.Name);
        Assert.Equal(originalEvent.Description, rescheduledEvent.Description);
        Assert.Equal(originalEvent.WarningBefore, rescheduledEvent.WarningBefore);
        Assert.Equal(originalTime, rescheduledEvent.ScheduledTime); // This would be set by calling code
    }
}
