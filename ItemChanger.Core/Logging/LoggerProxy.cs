namespace ItemChanger.Logging;

/// <summary>
/// Public hook point to have ItemChanger inject its logs to a logger of the caller's choosing.
/// </summary>
public static class LoggerProxy
{
    internal static void LogFine(string? message)
    {
        ItemChangerHost.Singleton.Logger.LogFine(message);
    }

    internal static void LogDebug(string? message)
    {
        ItemChangerHost.Singleton.Logger.LogDebug(message);
    }

    internal static void LogInfo(string? message)
    {
        ItemChangerHost.Singleton.Logger.LogInfo(message);
    }

    internal static void LogWarn(string? message)
    {
        ItemChangerHost.Singleton.Logger.LogWarn(message);
    }

    internal static void LogError(string? message)
    {
        ItemChangerHost.Singleton.Logger.LogError(message);
    }
}
