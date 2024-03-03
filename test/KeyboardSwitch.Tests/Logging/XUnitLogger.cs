namespace KeyboardSwitch.Tests.Logging;

using System.Text;

using Microsoft.Extensions.Logging;

using Xunit.Abstractions;

public class XUnitLogger(
    ITestOutputHelper testOutputHelper,
    LoggerExternalScopeProvider scopeProvider,
    string categoryName)
    : ILogger
{
    private readonly ITestOutputHelper testOutputHelper = testOutputHelper;
    private readonly LoggerExternalScopeProvider scopeProvider = scopeProvider;
    private readonly string categoryName = categoryName;

    public static ILogger Create(Type type, ITestOutputHelper testOutputHelper) =>
        new XUnitLogger(testOutputHelper, new LoggerExternalScopeProvider(), type.FullName ?? String.Empty);

    public static ILogger Create(ITestOutputHelper testOutputHelper) =>
        new XUnitLogger(testOutputHelper, new LoggerExternalScopeProvider(), String.Empty);

    public static ILogger<T> Create<T>(ITestOutputHelper testOutputHelper) =>
        new XUnitLogger<T>(testOutputHelper, new LoggerExternalScopeProvider());

    public bool IsEnabled(LogLevel logLevel) =>
        logLevel != LogLevel.None;

    public IDisposable BeginScope<TState>(TState state)
        where TState : notnull =>
        this.scopeProvider.Push(state);

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var sb = new StringBuilder();

        sb.Append(this.GetLogLevelString(logLevel))
            .Append(" [")
            .Append(this.categoryName)
            .Append("] ")
            .Append(formatter(state, exception));

        if (exception != null)
        {
            sb.Append('\n').Append(exception);
        }

        this.scopeProvider.ForEachScope(
            (scope, state) =>
            {
                state.Append("\n => ");
                state.Append(scope);
            },
            sb);

        this.testOutputHelper.WriteLine(sb.ToString());
    }

    private string GetLogLevelString(LogLevel logLevel) =>
        logLevel switch
        {
            LogLevel.Trace => "TRC",
            LogLevel.Debug => "DBG",
            LogLevel.Information => "INF",
            LogLevel.Warning => "WRN",
            LogLevel.Error => "ERR",
            LogLevel.Critical => "CRT",
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
        };
}

public sealed class XUnitLogger<T>(ITestOutputHelper testOutputHelper, LoggerExternalScopeProvider scopeProvider)
    : XUnitLogger(testOutputHelper, scopeProvider, typeof(T).FullName ?? String.Empty), ILogger<T>;
