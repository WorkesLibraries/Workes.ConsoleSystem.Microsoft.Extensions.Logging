# Workes.ConsoleSystem.Microsoft.Extensions.Logging

An opt-in `ILoggerProvider` that forwards Microsoft.Extensions.Logging records to a supplied Workes.ConsoleSystem `ConsoleManager`. It preserves categories and exceptions, applies configurable severity mapping, and materializes external scopes into the message.

## Installation

```bash
dotnet add package Workes.ConsoleSystem.Microsoft.Extensions.Logging --version 0.1.0
```

```xml
<PackageReference Include="Workes.ConsoleSystem.Microsoft.Extensions.Logging" Version="0.1.0" />
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

Use `ConsoleSystemLoggerOptions.LevelMapper` to override mappings. Return `null` to ignore a level; `LogLevel.None` is ignored by default. The default maps Trace, Debug, Information, Warning, Error, and Critical directly. External scopes are prepended to the formatted message. Disposing the provider stops further writes.

Targets .NET Standard 2.1 and references Microsoft.Extensions.Logging.Abstractions 10.0.0, and supports the 10.x provider API. The consuming app supplies its own logging framework and dependency injection registrations. The host application supplies its own logging framework and dependency injection registrations.

See [Quick Start](docs/QUICK_START.md) and [Changelog](CHANGELOG.md).

## License

MIT. See [LICENSE](LICENSE).

