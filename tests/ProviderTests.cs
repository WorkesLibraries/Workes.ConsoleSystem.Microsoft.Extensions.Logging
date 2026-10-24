using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Workes.ConsoleSystem.Core;
using Workes.ConsoleSystem.Entries;
using ConsoleLogLevel = Workes.ConsoleSystem.Logging.LogLevel;

namespace Workes.ConsoleSystem.Microsoft.Extensions.Logging.Tests;

public sealed class ProviderTests
{
    [TestCase(LogLevel.Trace, ConsoleLogLevel.Trace)]
    [TestCase(LogLevel.Debug, ConsoleLogLevel.Debug)]
    [TestCase(LogLevel.Information, ConsoleLogLevel.Information)]
    [TestCase(LogLevel.Warning, ConsoleLogLevel.Warning)]
    [TestCase(LogLevel.Error, ConsoleLogLevel.Error)]
    [TestCase(LogLevel.Critical, ConsoleLogLevel.Critical)]
    public void DefaultMap_MapsLevels(LogLevel source, ConsoleLogLevel expected) => Assert.That(ConsoleSystemLoggerOptions.DefaultMap(source), Is.EqualTo(expected));

    [Test]
    public void None_IsIgnored() => Assert.That(ConsoleSystemLoggerOptions.DefaultMap(LogLevel.None), Is.Null);

    [Test]
    public void Provider_PreservesCategoryMessageAndExceptionAndStopsAfterDispose()
    {
        var manager = new ConsoleManager();
        var provider = new ConsoleSystemLoggerProvider(manager);
        var logger = provider.CreateLogger("sample.category");
        var error = new InvalidOperationException("failed");
        logger.Log(LogLevel.Error, new EventId(7), "formatted", error, (state, _) => state);
        var entry = manager.History.Entries.OfType<LogEntry>().Single();
        Assert.That(entry.Message, Is.EqualTo("formatted"));
        Assert.That(entry.Source, Is.EqualTo("sample.category"));
        Assert.That(entry.Exception, Is.SameAs(error));
        provider.Dispose();
        logger.Log(LogLevel.Error, new EventId(8), "ignored", null, (state, _) => state);
        Assert.That(manager.History.Entries.OfType<LogEntry>().Count(), Is.EqualTo(1));
        Assert.That(logger.IsEnabled(LogLevel.Error), Is.False);
    }

    [Test]
    public void CustomMapping_CanIgnoreLevels()
    {
        var manager = new ConsoleManager();
        using var provider = new ConsoleSystemLoggerProvider(manager, new ConsoleSystemLoggerOptions { LevelMapper = level => level == LogLevel.Warning ? null : ConsoleLogLevel.Debug });
        var logger = provider.CreateLogger("test");
        logger.Log(LogLevel.Warning, default, "ignored", null, (state, _) => state);
        logger.Log(LogLevel.Error, default, "kept", null, (state, _) => state);
        Assert.That(manager.History.Entries.OfType<LogEntry>().Select(e => e.Message), Is.EqualTo(new[] { "kept" }));
    }

    [Test]
    public void ExternalScopes_AreIncludedInMessage()
    {
        var manager = new ConsoleManager();
        using var provider = new ConsoleSystemLoggerProvider(manager);
        var scopeProvider = new LoggerExternalScopeProvider();
        provider.SetScopeProvider(scopeProvider);
        using (scopeProvider.Push("request-42"))
            provider.CreateLogger("cat").Log(LogLevel.Information, default, "started", null, (state, _) => state);
        Assert.That(manager.History.Entries.OfType<LogEntry>().Single().Message, Is.EqualTo("[request-42] started"));
    }
}
