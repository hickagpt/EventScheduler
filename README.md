# EventScheduler

[![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.1-green.svg)](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)
[![NuGet](https://img.shields.io/badge/Test%20Coverage-98.5%25-brightgreen.svg)]()
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

A lightweight, high-performance .NET library for scheduling timed events with configurable warning systems. Perfect for game development, simulation environments, and any application requiring precise event timing and lifecycle management.

## 🎯 Features

- **⏰ Precise Event Scheduling**: Chronological event ordering with efficient O(n) insertion
- **⚠️ Warning System**: Configurable advance warnings before event execution
- **🔄 Event Lifecycle Management**: Schedule, reschedule, cancel, and track events
- **🏗️ Generic Architecture**: Works with any `IWorldDateTimeProvider` implementation
- **📦 Builder Pattern**: Fluent API for declarative event construction
- **🔍 Event Querying**: Retrieve events by ID or enumerate all scheduled events
- **📊 Observer Pattern**: Event execution notifications and callbacks
- **✅ 98.5% Test Coverage**: Comprehensive unit and integration tests

## 📦 Installation

### NuGet Package (Coming Soon)
```bash
# Not yet published to NuGet - build from source
```

### Build from Source
```bash
git clone https://github.com/hickagpt/EventScheduler.git
cd eventscheduler
dotnet build EventScheduler.sln
```

## 🚀 Quick Start

### Basic Event Scheduling

```csharp
using EventScheduler;
using EventScheduler.Abstractions;

// 1. Create a time provider (implement your own or use a mock)
public class GameTimeProvider : IWorldDateTimeProvider
{
    public DateTime CurrentTime { get; set; } = DateTime.Now;
}

// 2. Create an event scheduler
var scheduler = new EventScheduler<GameTimeProvider>();
var gameWorld = new GameTimeProvider();

// 3. Schedule a simple event
var futureTime = gameWorld.CurrentTime.AddSeconds(30);
var eventId = scheduler.ScheduleEvent(
    new ScheduledEvent<GameTimeProvider>.Builder()
        .SetScheduledTime(futureTime)
        .SetName("Player Spawn")
        .SetRunAction(world => Console.WriteLine("Player spawned!"))
        .SetIsDueFunc(world => world.CurrentTime >= futureTime)
        .Build()
);

Console.WriteLine($"Scheduled event with ID: {eventId}");
```

### Advanced Event with Warnings

```csharp
// Create an event with a 1-minute warning
var missionTime = gameWorld.CurrentTime.AddMinutes(5);
var missionEvent = new ScheduledEvent<GameTimeProvider>.Builder()
    .SetScheduledTime(missionTime)
    .SetName("Mission Start")
    .SetDescription("Assault the enemy base")
    .SetWarningBefore(TimeSpan.FromMinutes(1)) // Warn 1 minute before
    .SetRunAction(world => StartMission(world))
    .SetRunWarningAction(world => ShowMissionWarning(world))
    .SetIsDueFunc(world => world.CurrentTime >= missionTime)
    .Build();

// Schedule the event
scheduler.ScheduleEvent(missionEvent);

// Subscribe to execution notifications
scheduler.ScheduledEventRun += (sender, args) =>
{
    Console.WriteLine($"Event completed: {args.ScheduledEvent.Name}");
};

// Update the scheduler (call this regularly, e.g., in your game loop)
scheduler.Update(gameWorld);
```

### Complex Time Provider Example

```csharp
// Custom time provider for accelerated time scenarios
public class AcceleratedGameTime : IWorldDateTimeProvider
{
    private DateTime _startTime = DateTime.Now;
    public double TimeMultiplier { get; set; } = 1.0;

    public DateTime CurrentTime =>
        _startTime.AddSeconds((DateTime.Now - _startTime).TotalSeconds * TimeMultiplier);
}

// Use in fast-forward scenarios (e.g., time-lapse gameplay)
var acceleratedTime = new AcceleratedGameTime { TimeMultiplier = 10.0 };
var scheduler = new EventScheduler<AcceleratedGameTime>();
```

## 📚 API Reference

### IEventScheduler<TWorld>

```csharp
public interface IEventScheduler<TWorld> where TWorld : IWorldDateTimeProvider
{
    // Events
    event EventHandler<ScheduledEventRunEventArgs<TWorld>>? ScheduledEventRun;

    // Methods
    Guid ScheduleEvent(IScheduledEvent<TWorld> scheduledEvent);
    void RescheduleEvent(Guid scheduledEventId, DateTime newScheduledTime);
    IEnumerable<IScheduledEvent<TWorld>> GetScheduledEvents();
    IScheduledEvent<TWorld>? GetScheduledEvent(Guid scheduledEventId);
    bool CancelEvent(Guid scheduledEventId);
    void Update(TWorld world);
}
```

### IScheduledEvent<TWorld>

```csharp
public interface IScheduledEvent<TWorld>
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
```

### Builder Pattern

```csharp
var event = new ScheduledEvent<MyWorld>.Builder()
    .SetScheduledTime(DateTime.Now.AddHours(1))
    .SetName("Hourly Backup")
    .SetWarningBefore(TimeSpan.FromMinutes(5))
    .SetRunAction(world => PerformBackup(world))
    .SetRunWarningAction(world => NotifyUsers(world))
    .SetIsDueFunc(world => world.CurrentTime >= scheduledTime)
    .Build();
```

## 🏗️ Architecture

```
EventScheduler.sln
├── EventScheduler.Abstractions/
│   ├── IEventScheduler.cs
│   ├── IScheduledEvent.cs
│   └── IWorldDateTimeProvider.cs    # Time abstraction
├── EventScheduler/
│   ├── EventScheduler.cs           # Main scheduler implementation
│   └── ScheduledEvent.cs           # Event implementation with Builder
└── EventScheduler.Tests/           # Comprehensive test suite
    ├── EventSchedulerTests.cs      # 30 unit/integration tests
    └── Coverage/                   # 98.5% code coverage reports
```

### Design Principles

- **Generic Programming**: Works with any time provider implementation
- **Builder Pattern**: Fluent, readable event construction
- **Observer Pattern**: Event-driven notifications for execution
- **Chronological Ordering**: O(n) insertion maintains time-based sequence
- **Abstraction Layer**: Clean separation of interfaces and implementations

## 🧪 Testing

The library includes a comprehensive test suite with excellent coverage:

```bash
# Run all tests with coverage
.\test-suite.ps1

# Or run manually:
dotnet test --collect:"XPlat Code Coverage"
```

### Test Results
- ✅ **30 Tests** - All passing
- ✅ **98.5% Line Coverage**
- ✅ **100% Method Coverage**
- ✅ **90.4% Branch Coverage**

Test categories include:
- Unit tests for core scheduling logic
- Integration tests for event lifecycle
- Warning system validation
- Edge case coverage
- Time manipulation scenarios

## 🚀 Use Cases

### 🎮 Game Development
```csharp
// Level transition events
var levelEndEvent = new ScheduledEvent<GameWorld>.Builder()
    .SetScheduledTime(DateTime.Now.AddMinutes(30))
    .SetWarningBefore(TimeSpan.FromSeconds(30))
    .SetRunAction(world => LoadNextLevel(world))
    .SetRunWarningAction(world => ShowLevelEndingMessage(world))
    .Build();
```

### 🤖 IoT Applications
```csharp
// Sensor maintenance scheduling
var maintenanceEvent = new ScheduledEvent<IoTDevice>.Builder()
    .SetScheduledTime(DateTime.Now.AddDays(30))
    .SetWarningBefore(TimeSpan.FromDays(1))
    .SetRunAction(device => PerformCalibration(device))
    .SetRunWarningAction(device => SendMaintenanceAlert(device))
    .Build();
```

### 🏭 Simulation Systems
```csharp
// Production line events
var productionShift = new ScheduledEvent<Factory>.Builder()
    .SetScheduledTime(DateTime.Parse("6:00 AM").AddDays(1))
    .SetWarningBefore(TimeSpan.FromMinutes(15))
    .SetRunAction(factory => StartProductionLine(factory))
    .SetRunWarningAction(factory => AlertWorkers(factory))
    .Build();
```

### 🖥️ Server Applications
```csharp
// Job scheduling system
var backupJob = new ScheduledEvent<Server>.Builder()
    .SetScheduledTime(DateTime.Now.Date.AddHours(2))
    .SetWarningBefore(TimeSpan.FromMinutes(10))
    .SetRunAction(server => ExecuteBackup(server))
    .SetRunWarningAction(server => PrepareBackupResources(server))
    .Build();
```

## 🤝 Contributing

We welcome contributions! Please follow these steps:

### Development Setup
```bash
# Clone and build
git clone https://github.com/yourusername/eventscheduler.git
cd eventscheduler
dotnet build EventScheduler.sln

# Run tests
.\test-suite.ps1

# View coverage report (opens in browser)
start coveragereport\index.html
```

### Contribution Guidelines
1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/amazing-feature`)
3. **Test** your changes thoroughly
4. **Commit** your changes (`git commit -m 'Add some amazing feature'`)
5. **Push** to the branch (`git push origin feature/amazing-feature`)
6. **Open** a Pull Request

### Code Standards
- Follow C# coding guidelines
- Add comprehensive unit tests for new features
- Maintain code coverage above 95%
- Use meaningful commit messages
- Update documentation for API changes

### Testing Requirements
- All new code must include unit tests
- Integration tests for complex features
- Code coverage must not decrease
- All tests must pass on CI

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

```
MIT License

Copyright (c) 2025 EventScheduler Contributors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
```

## 📞 Support

- 📖 **Documentation**: See this README and inline code comments
- 🐛 **Bug Reports**: Open an issue with detailed reproduction steps
- 💡 **Feature Requests**: Open an issue with use case descriptions
- 💬 **Discussions**: GitHub Discussions for general questions

## 🙏 Acknowledgments

- Built with .NET Standard for maximum compatibility
- Comprehensive test coverage using xUnit and Coverlet
- Inspired by event scheduling needs in game development and simulation

---

**EventScheduler** - Precise timing, reliable execution, flexible warnings.
