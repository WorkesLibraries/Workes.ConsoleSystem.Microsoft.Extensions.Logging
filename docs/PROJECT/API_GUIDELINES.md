# API Guidelines

Create `ConsoleSystemLoggerProvider(manager, options)` and register it with an existing host or `LoggerFactory`. This package supplies abstractions and a provider, not a concrete host, logger factory, or service container.

`ConsoleSystemLoggerOptions.LevelMapper` overrides the default same-name mapping for Trace through Critical. `LogLevel.None` maps to null; custom mappings can also return null to ignore a source level. Host filtering and adapter mapping are separate gates. The mapper can be queried by `IsEnabled` and again when a record is written, so custom delegates should be deterministic and side-effect free.

The formatted message becomes `LogEntry.Message`; category becomes `Source`; the original exception becomes `Exception`. Active scope strings are prepended in scope order. Structured logging state and `EventId` are not separately queryable afterward.

The caller owns the manager. Dispose the provider through the host or explicit lifetime scope to stop writes and release logger/scope references. Operations and history events are synchronous on the logging thread; the adapter does not marshal UI work. Custom mappers and history callbacks run during the logging call, so keep them safe for synchronous use.
