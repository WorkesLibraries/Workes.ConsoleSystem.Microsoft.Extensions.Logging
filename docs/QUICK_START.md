# Quick Start

Create a `ConsoleManager`, pass it to `ConsoleSystemLoggerProvider`, and register the provider with `LoggerFactory` or an `ILoggingBuilder`. The provider leaves manager lifetime to the caller and must be disposed before or with the host. `LogLevel.None` and custom mappings that return `null` do not create console entries.
