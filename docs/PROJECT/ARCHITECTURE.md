# Architecture

## Boundary

This package implements `ILoggerProvider` and writes to a caller-supplied ConsoleSystem manager. It targets .NET Standard 2.1 and depends on ConsoleSystem 0.9.0 and Microsoft.Extensions.Logging.Abstractions 10.0.0; it does not depend on a concrete host or DI framework.

## Flow

`ConsoleSystemLoggerProvider` caches category-specific internal loggers in an ordinal concurrent dictionary and implements `ISupportExternalScope`. Loggers delegate enablement, scope handling, and writes to the provider. The provider owns the level mapper, external scope provider, and manager reference.

A log call applies the mapper, formats the message with active scopes prepended in scope order, and records a `LogEntry`. Category is `Source`; the original exception is retained. Structured state and `EventId` are not stored separately. Standard host filters run before the provider; a mapper returning null disables a level, and None is ignored by default.

Provider operations are serialized. Disposal stops writes, clears cached loggers, resets scope state, and does not dispose the manager. Writes and history notifications are synchronous on the logging thread.
