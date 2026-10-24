# Workes.ConsoleSystem.Microsoft.Extensions.Logging

An opt-in `ILoggerProvider` that forwards Microsoft.Extensions.Logging records to a supplied Workes.ConsoleSystem `ConsoleManager`. It preserves logger categories and exceptions, applies configurable severity mapping, and includes external scopes in the formatted message.

## Installation

```bash
dotnet add package Workes.ConsoleSystem.Microsoft.Extensions.Logging --version 1.0.0
```

```xml
<PackageReference Include="Workes.ConsoleSystem.Microsoft.Extensions.Logging" Version="1.0.0" />
```

## Registration

```csharp
using Microsoft.Extensions.Logging;
using Workes.ConsoleSystem.Core;
using Workes.ConsoleSystem.Microsoft.Extensions.Logging;

var manager = new ConsoleManager();
using var provider = new ConsoleSystemLoggerProvider(manager);
using var factory = LoggerFactory.Create(builder => builder.AddProvider(provider));
factory.CreateLogger("Game.SaveSystem").LogInformation("Loaded save {SaveId}", "slot-1");
```

Use `ConsoleSystemLoggerOptions.LevelMapper` to override mappings. Return `null` to ignore a level; `LogLevel.None` is ignored by default. Standard `LoggerFactory` filters can also control which records reach the provider. The adapter retains the formatted message, category as `LogEntry.Source`, and original exception; scopes are prepended to the message. Structured state and `EventId` are not stored separately. Disposing the provider stops further writes; it does not dispose the supplied manager.

Targets .NET Standard 2.1 and depends on Workes.ConsoleSystem 0.9.0 and Microsoft.Extensions.Logging.Abstractions 10.0.0. The provider API is built against Microsoft.Extensions.Logging.Abstractions 10.0.0. The example's `LoggerFactory` comes from `Microsoft.Extensions.Logging`; use that package or your host's existing logging framework and dependency injection registrations. This adapter does not bring in a concrete logging framework.

See [Quick Start](docs/QUICK_START.md) and [Changelog](CHANGELOG.md).

## License

MIT. See [LICENSE](LICENSE).

