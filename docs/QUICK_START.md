# Quick Start

After installing the package, create a `ConsoleManager` and register the provider with your logging host. The adapter targets .NET Standard 2.1 and uses `Microsoft.Extensions.Logging.Abstractions` 10.0.0. The provider itself does not include a concrete logging framework; use the framework already provided by your application. The `LoggerFactory` example below uses `Microsoft.Extensions.Logging`.

```csharp
using Microsoft.Extensions.Logging;
using Workes.ConsoleSystem.Core;
using Workes.ConsoleSystem.Microsoft.Extensions.Logging;

var console = new ConsoleManager();
using var provider = new ConsoleSystemLoggerProvider(console);
using var factory = LoggerFactory.Create(builder =>
{
    builder.SetMinimumLevel(LogLevel.Information);
    builder.AddProvider(provider);
});

var logger = factory.CreateLogger("Game.SaveSystem");
logger.LogInformation("Loaded save {SaveId}", "slot-1");
```

The provider maps Trace, Debug, Information, Warning, Error, and Critical to the matching ConsoleSystem levels. `LogLevel.None` is ignored. A logging host's normal filters, such as `SetMinimumLevel`, run before records reach the provider.

## Customize level mapping

Set `LevelMapper` to map levels differently or return `null` to ignore a level:

```csharp
var options = new ConsoleSystemLoggerOptions
{
    LevelMapper = level => level switch
    {
        LogLevel.Trace or LogLevel.Debug => null,
        LogLevel.Information => Workes.ConsoleSystem.Logging.LogLevel.Debug,
        _ => ConsoleSystemLoggerOptions.DefaultMap(level)
    }
};

using var provider = new ConsoleSystemLoggerProvider(console, options);
```

## Records, scopes, and lifetime

The adapter stores the formatter's message in `LogEntry.Message`, the logger category in `LogEntry.Source`, and the original exception in `LogEntry.Exception`. Microsoft structured state and `EventId` are not retained as separate fields. Active scopes are formatted in order and prepended to the message, for example `[request-42 => user-7] Loaded save slot-1`.

The provider does not own or dispose the `ConsoleManager`. Dispose the provider when the logging host shuts down; disposing it stops later writes and releases its logger and scope references. The manager records synchronously on the thread that logs. ConsoleSystem synchronizes history writes, while any history-change callbacks run on that same logging thread; marshal UI work to the appropriate host thread yourself.

## Register with a Generic Host

For an application using .NET Generic Host, register one manager and create one provider from it. Add the adapter package to the project along with the host package your app uses (for example, `Microsoft.Extensions.Hosting`). The adapter package supplies the provider and logging abstractions; it does not supply the Generic Host.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Workes.ConsoleSystem.Core;
using Workes.ConsoleSystem.Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// Create one ConsoleManager for this application and make it injectable.
builder.Services.AddSingleton<ConsoleManager>();

// Create one adapter provider using that same manager.
builder.Logging.Services.AddSingleton<ILoggerProvider>(services =>
    new ConsoleSystemLoggerProvider(
        services.GetRequiredService<ConsoleManager>()));

using var host = builder.Build();

// Application services can receive ILogger<T> through constructor injection.
var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application started");

await host.RunAsync();
```

`builder.Services` is the application's dependency-injection container. `AddSingleton<ConsoleManager>()` asks it to construct one manager and reuse it wherever `ConsoleManager` is requested. `builder.Logging.Services` is the service collection used by the logging system. Its `AddSingleton<ILoggerProvider>(...)` registration creates the adapter provider when logging is built and gives it the manager from the same container.

The host's logging system obtains the provider from dependency injection, creates category-specific `ILogger` instances on demand, and disposes the provider during shutdown. The adapter does not dispose the manager. The manager is registered once so all records flow into the same ConsoleSystem history. Normal host logging filters, including configuration-based filters, apply before the provider receives a record.

In an application that already has a `ConsoleManager`, register that existing instance instead of asking the container to construct another one. For direct `LoggerFactory` use without a host, see the first example above and dispose the factory and provider when the application shuts down.
