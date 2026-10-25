# Decisions

Append long-lived decisions using the next `D-NNN` identifier. Preserve superseded entries.

### D-001: Provide the abstractions provider without owning a host

#### Context
ConsoleSystem should accept Microsoft logging records without depending on an application's hosting or concrete logging framework.

#### Decision
Implement `ILoggerProvider` and `ISupportExternalScope` using Microsoft.Extensions.Logging.Abstractions only. Use the caller's manager and do not add a concrete host or DI dependency.

#### Reasoning
Applications can integrate the provider into their existing host and retain control of provider registration and manager ownership.

#### Consequences
Host filtering and logger factory lifetime stay outside this package. Consumers must dispose the provider through that host or explicitly.

### D-002: Store rendered scope context in source-neutral entries

#### Context
Microsoft logging supplies category, formatted text, scopes, exceptions, structured state, and EventId, while ConsoleSystem entries expose general message/source/exception fields.

#### Decision
Store formatted text, category as source, original exception, and prepend active scope strings in order. Do not preserve structured state or EventId separately. Default-map standard levels by name and ignore None.

#### Reasoning
This carries common diagnostic context without introducing Microsoft-specific entry types into ConsoleSystem.

#### Consequences
Scope data is rendered at write time, not independently queryable. Writes and history callbacks remain synchronous on the caller thread.
