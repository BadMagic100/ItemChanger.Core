using ItemChanger.Containers;
using ItemChanger.Events;
using ItemChanger.Logging;
using ItemChanger.Modules;

namespace ItemChanger.Tests.Fixtures;

internal class TestLogger : ILogger
{
    public List<string?> ErrorMessages { get; } = [];

    public void LogFine(string? message)
    {
        Console.WriteLine($"[FINE]: {message}");
    }

    public void LogInfo(string? message)
    {
        Console.WriteLine($"[INFO]: {message}");
    }

    public void LogDebug(string? message)
    {
        Console.WriteLine($"[DEBUG]: {message}");
    }

    public void LogWarn(string? message)
    {
        Console.WriteLine($"[WARN]: {message}");
    }

    public void LogError(string? message)
    {
        ErrorMessages.Add(message);
        Console.WriteLine($"[ERROR]: {message}");
    }
}

public class TestHost : ItemChangerHost, IDisposable
{
    public TestHost()
        : base()
    {
        Logger = new TestLogger();
        Profile = new(this);
    }

    public void Dispose()
    {
        DetachSingleton();
    }

    public override ILogger Logger { get; }
    public List<string?> ErrorMessages
    {
        get => ((TestLogger)Logger).ErrorMessages;
    }

    public ItemChangerProfile Profile { get; }

    public override ContainerRegistry ContainerRegistry
    {
        get
        {
            FakedContainer fake = new();
            return field ??= new ContainerRegistry()
            {
                DefaultSingleItemContainer = fake,
                DefaultMultiItemContainer = fake,
            };
        }
    }

    public override Finder Finder { get; } = new();

    public override IEnumerable<Module> BuildDefaultModules() => [];

    protected override void PrepareEvents(
        LifecycleEvents.Invoker lifecycleInvoker,
        GameEvents.Invoker gameInvoker
    )
    {
        LifecycleEventsInvoker = lifecycleInvoker;
        GameEventsInvoker = gameInvoker;
    }

    protected override void UnhookEvents(
        LifecycleEvents.Invoker lifecycleInvoker,
        GameEvents.Invoker gameInvoker
    )
    {
        LifecycleEventsInvoker = null;
        GameEventsInvoker = null;
    }

    /// <summary>
    /// Executes lifecycle events in order, stopping early if an error message is recorded to the <see cref="Logger"/>.
    /// </summary>
    /// <returns>False if the execution stopped early, otherwise true.</returns>
    public bool RunStartNewLifecycle()
    {
        if (LifecycleEventsInvoker is null)
        {
            throw new NullReferenceException(nameof(LifecycleEventsInvoker));
        }

        IEnumerable<Action> cycle =
        [
            LifecycleEventsInvoker.NotifyBeforeStartNewGame,
            Profile.Load,
            LifecycleEventsInvoker.NotifyOnEnterGame,
            LifecycleEventsInvoker.NotifyAfterStartNewGame,
            LifecycleEventsInvoker.NotifyOnSafeToGiveItems,
        ];

        foreach (Action a in cycle)
        {
            a();
            if (ErrorMessages.Count > 0)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Executes lifecycle events in order, stopping early if an error message is recorded to the <see cref="Logger"/>.
    /// </summary>
    /// <returns>False if the execution stopped early, otherwise true.</returns>
    public bool RunContinueLifecycle()
    {
        if (LifecycleEventsInvoker is null)
        {
            throw new NullReferenceException(nameof(LifecycleEventsInvoker));
        }

        IEnumerable<Action> cycle =
        [
            LifecycleEventsInvoker.NotifyBeforeContinueGame,
            Profile.Load,
            LifecycleEventsInvoker.NotifyOnEnterGame,
            LifecycleEventsInvoker.NotifyAfterContinueGame,
            LifecycleEventsInvoker.NotifyOnSafeToGiveItems,
        ];

        foreach (Action a in cycle)
        {
            a();
            if (ErrorMessages.Count > 0)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Executes lifecycle events in order, stopping early if an error message is recorded to the <see cref="Logger"/>.
    /// </summary>
    /// <returns>False if the execution stopped early, otherwise true.</returns>
    public bool RunLeaveLifecycle()
    {
        if (LifecycleEventsInvoker is null)
        {
            throw new NullReferenceException(nameof(LifecycleEventsInvoker));
        }

        IEnumerable<Action> cycle = [LifecycleEventsInvoker.NotifyOnLeaveGame, Profile.Unload];

        foreach (Action a in cycle)
        {
            a();
            if (ErrorMessages.Count > 0)
            {
                return false;
            }
        }

        return true;
    }

    public LifecycleEvents.Invoker? LifecycleEventsInvoker { get; private set; }
    public GameEvents.Invoker? GameEventsInvoker { get; private set; }
}
