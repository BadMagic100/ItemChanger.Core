namespace ItemChanger.Logging;

/// <summary>
/// Logger implementation that discards all messages.
/// </summary>
public class NullLogger : ILogger
{
    /// <summary>
    /// Singleton instance.
    /// </summary>
    public static NullLogger Instance { get; } = new();

    /// <summary>
    /// Ignores verbose diagnostic messages.
    /// </summary>
    public void LogFine(string? message) { }

    /// <summary>
    /// Ignores debug messages.
    /// </summary>
    public void LogDebug(string? message) { }

    /// <summary>
    /// Ignores informational messages.
    /// </summary>
    public void LogInfo(string? message) { }

    /// <summary>
    /// Ignores warnings.
    /// </summary>
    public void LogWarn(string? message) { }

    /// <summary>
    /// Ignores error messages.
    /// </summary>
    public void LogError(string? message) { }
}
