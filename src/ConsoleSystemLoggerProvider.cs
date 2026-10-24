using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using Workes.ConsoleSystem.Core;
using Workes.ConsoleSystem.Entries;
using ConsoleLogLevel = Workes.ConsoleSystem.Logging.LogLevel;

namespace Workes.ConsoleSystem.Microsoft.Extensions.Logging;

/// <summary>Maps Microsoft logging levels to ConsoleSystem levels. Return null to ignore a level.</summary>
public sealed class ConsoleSystemLoggerOptions
{
    /// <summary>Gets or sets the mapping policy. The default maps Trace through Critical directly; None is ignored.</summary>
    public Func<LogLevel, ConsoleLogLevel?> LevelMapper { get; set; } = DefaultMap;

    /// <summary>Maps standard levels to their ConsoleSystem equivalents.</summary>
    public static ConsoleLogLevel? DefaultMap(LogLevel level) => level switch
    {
        LogLevel.Trace => ConsoleLogLevel.Trace,
        LogLevel.Debug => ConsoleLogLevel.Debug,
        LogLevel.Information => ConsoleLogLevel.Information,
        LogLevel.Warning => ConsoleLogLevel.Warning,
        LogLevel.Error => ConsoleLogLevel.Error,
        LogLevel.Critical => ConsoleLogLevel.Critical,
        _ => null
    };
}

/// <summary>Forwards Microsoft.Extensions.Logging records to a ConsoleManager.</summary>
public sealed class ConsoleSystemLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly object _gate = new object();
    private readonly ConcurrentDictionary<string, ConsoleSystemLogger> _loggers = new ConcurrentDictionary<string, ConsoleSystemLogger>(StringComparer.Ordinal);
    private readonly Func<LogLevel, ConsoleLogLevel?> _levelMapper;
    private ConsoleManager? _manager;
    private IExternalScopeProvider _scopeProvider = new LoggerExternalScopeProvider();

    /// <summary>Creates a provider attached to the supplied console manager.</summary>
    public ConsoleSystemLoggerProvider(ConsoleManager manager, ConsoleSystemLoggerOptions? options = null)
    {
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        _levelMapper = options?.LevelMapper ?? ConsoleSystemLoggerOptions.DefaultMap;
    }

    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName)
    {
        if (categoryName is null) throw new ArgumentNullException(nameof(categoryName));
        return _loggers.GetOrAdd(categoryName, name => new ConsoleSystemLogger(this, name));
    }

    /// <inheritdoc />
    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        if (scopeProvider is null) throw new ArgumentNullException(nameof(scopeProvider));
        lock (_gate) { if (_manager is not null) _scopeProvider = scopeProvider; }
    }

    internal IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        lock (_gate) return _scopeProvider.Push(state);
    }

    internal bool IsEnabled(LogLevel level)
    {
        lock (_gate) return _manager is not null && _levelMapper(level).HasValue;
    }

    internal void Write(string category, LogLevel level, string message, Exception? exception)
    {
        lock (_gate)
        {
            if (_manager is null) return;
            ConsoleLogLevel? mapped = _levelMapper(level);
            if (!mapped.HasValue) return;
            var scopes = new List<object?>();
            _scopeProvider.ForEachScope(static (scope, state) => state.Add(scope), scopes);
            if (scopes.Count > 0) message = "[" + string.Join(" => ", scopes) + "] " + message;
            _manager.RecordEntry(new LogEntry(DateTimeOffset.UtcNow, mapped.Value, message, category, exception));
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        lock (_gate)
        {
            _manager = null;
            _loggers.Clear();
            _scopeProvider = new LoggerExternalScopeProvider();
        }
    }
}

internal sealed class ConsoleSystemLogger : ILogger
{
    private readonly ConsoleSystemLoggerProvider _provider;
    private readonly string _category;
    internal ConsoleSystemLogger(ConsoleSystemLoggerProvider provider, string category) { _provider = provider; _category = category; }
    #pragma warning disable CS8633, CS8766
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => _provider.BeginScope(state);
    #pragma warning restore CS8633, CS8766
    public bool IsEnabled(LogLevel logLevel) => _provider.IsEnabled(logLevel);
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (formatter is null) throw new ArgumentNullException(nameof(formatter));
        if (!IsEnabled(logLevel)) return;
        _provider.Write(_category, logLevel, formatter(state, exception), exception);
    }
}







