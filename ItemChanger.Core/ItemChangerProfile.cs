using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using ItemChanger.Containers;
using ItemChanger.Enums;
using ItemChanger.Events;
using ItemChanger.Items;
using ItemChanger.Logging;
using ItemChanger.Modules;
using ItemChanger.Placements;
using ItemChanger.Serialization;
using ItemChanger.Tags;
using Newtonsoft.Json;

namespace ItemChanger;

/// <summary>
/// Represents the ItemChanger data (placements, modules, hooks) associated with an in-game save file, tied to a particular host.
/// </summary>
public class ItemChangerProfile : IDisposable
{
    internal enum LoadState : uint
    {
        Unloaded = 0,
        LoadStarted = 1,
        ModuleLoadStarted = 2,
        ModuleLoadCompleted = 3,
        PlacementsLoadStarted = 4,
        PlacementsLoadCompleted = 5,
        LoadCompleted = uint.MaxValue,
    }

    [JsonProperty("Placements")]
    private readonly Dictionary<string, Placement> placements = [];

    /// <summary>
    /// Gets the set of modules that are part of this profile.
    /// </summary>
    [JsonProperty]
    public ModuleCollection Modules { get; private init; } = [];

    private bool hooked;
    internal LoadState State { get; private set; } = LoadState.Unloaded;

    private ItemChangerHost host;
    private LifecycleEvents.Invoker lifecycleInvoker;
    private GameEvents.Invoker gameInvoker;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
    [JsonConstructor]
    private ItemChangerProfile() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor.

    /// <summary>
    /// Initializes a new profile
    /// </summary>
    /// <param name="host">The associated host</param>
    public ItemChangerProfile(ItemChangerHost host)
    {
        AttachHost(host);

        Modules = [.. host.BuildDefaultModules()];

        DoHook();
    }

    /// <summary>
    /// Loads a profile from a stream
    /// </summary>
    /// <param name="host">The associated host</param>
    /// <param name="stream">The stream to read from</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">The stream doesn't contain a profile.</exception>
    /// <returns>The deserialized profile.</returns>
    public static ItemChangerProfile FromStream(ItemChangerHost host, Stream stream)
    {
        ItemChangerProfile? profile = SerializationHelper.DeserializeResource<ItemChangerProfile>(
            stream
        );
        if (profile == null)
        {
            throw new ArgumentException(
                "The provided stream did not contain a valid profile",
                nameof(stream)
            );
        }
        profile.AttachHost(host);
        profile.DoHook();
        return profile;
    }

    /// <summary>
    /// Ensures that the profile is unhooked when garbage-collected.
    /// </summary>
    ~ItemChangerProfile()
    {
        Dispose(false);
    }

    private bool disposed;

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases managed and unmanaged resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> when called from <see cref="Dispose()"/>; <see langword="false"/> from the finalizer.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (State == LoadState.LoadCompleted)
        {
            Unload();
        }

        if (host != null)
        {
            DoUnhook();
            if (host.ActiveProfile == this)
            {
                host.ActiveProfile = null;
            }
        }

        disposed = true;
    }

    /// <summary>
    /// Saves the profile to a stream as a JSON blob
    /// </summary>
    /// <param name="stream">The stream to save to.</param>
    public void ToStream(Stream stream)
    {
        SerializationHelper.Serialize(stream, this);
    }

    /// <summary>
    /// Gets all placements currently registered with this profile.
    /// </summary>
    public IEnumerable<Placement> GetPlacements() => placements.Values;

    /// <summary>
    /// Enumerates every item across all placements.
    /// </summary>
    public IEnumerable<Item> GetItems() => placements.Values.SelectMany(x => x.Items);

    /// <summary>
    /// Retrieves a placement by name.
    /// </summary>
    /// <param name="name">Placement name.</param>
    /// <returns>The requested placement.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no placement with the given name exists.</exception>
    public Placement GetPlacement(string name)
    {
        if (!placements.TryGetValue(name, out Placement? placement))
        {
            throw new KeyNotFoundException($"No placement with name {name} found");
        }
        return placement;
    }

    /// <summary>
    /// Attempts to find a placement by name.
    /// </summary>
    /// <param name="name">Placement name.</param>
    /// <param name="placement">Resolved placement when found.</param>
    /// <returns><see langword="true"/> when the placement exists; otherwise <see langword="false"/>.</returns>
    public bool TryGetPlacement(string name, [NotNullWhen(true)] out Placement? placement)
    {
        return placements.TryGetValue(name, out placement);
    }

    /// <summary>
    /// Resets the obtained state on items that match the provided persistence category.
    /// </summary>
    /// <param name="persistence">Persistence type to refresh.</param>
    internal void ResetPersistentItems(Persistence persistence)
    {
        if (persistence == Persistence.NonPersistent)
        {
            throw new ArgumentException(
                $"Cannot reset non-persistent items (persistence {nameof(Persistence.NonPersistent)})",
                nameof(persistence)
            );
        }

        foreach (Item item in GetItems())
        {
            if (
                item.GetTag<IPersistenceTag>(out IPersistenceTag? tag)
                && tag.Persistence == persistence
            )
            {
                item.RefreshObtained();
            }
        }
    }

    /// <summary>
    /// Loads modules and placements associated with this profile.
    /// </summary>
    public void Load()
    {
        if (State != LoadState.Unloaded)
        {
            throw new InvalidOperationException(
                $"Cannot load an already loaded profile. Current state is {State}"
            );
        }

        State = LoadState.LoadStarted;

        State = LoadState.ModuleLoadStarted;
        Modules.Load();
        State = LoadState.ModuleLoadCompleted;

        State = LoadState.PlacementsLoadStarted;
        foreach (Placement placement in placements.Values)
        {
            placement.LoadOnce();
        }
        State = LoadState.PlacementsLoadCompleted;

        State = LoadState.LoadCompleted;
    }

    /// <summary>
    /// Unloads modules and placements associated with this profile.
    /// </summary>
    public void Unload()
    {
        if (State != LoadState.LoadCompleted)
        {
            throw new InvalidOperationException(
                $"Cannot unload an unloaded or partially loaded profile. Current state is {State}"
            );
        }

        State = LoadState.PlacementsLoadCompleted;
        foreach (Placement placement in placements.Values)
        {
            placement.Unload();
        }
        State = LoadState.PlacementsLoadStarted;

        State = LoadState.ModuleLoadCompleted;
        Modules.Unload();
        State = LoadState.ModuleLoadStarted;

        State = LoadState.Unloaded;
    }

    /// <summary>
    /// Adds a placement to the profile, optionally resolving naming conflicts.
    /// </summary>
    /// <param name="placement">Placement to add.</param>
    /// <param name="conflictResolution">Conflict behavior when a placement with the same name exists.</param>
    public void AddPlacement(
        Placement placement,
        PlacementConflictResolution conflictResolution = PlacementConflictResolution.MergeKeepingNew
    )
    {
        EnsurePlacementMutationAllowed();

        bool placementActive = placements.TryGetValue(placement.Name, out Placement? existing)
            ? HandleExistingPlacement(placement, existing, conflictResolution)
            : AddBrandNewPlacement(placement);

        if (placementActive)
        {
            LoadIfNeeded(placement);
        }
    }

    /// <summary>
    /// Adds multiple placements.
    /// </summary>
    /// <param name="placements">Placements to add.</param>
    /// <param name="conflictResolution">Conflict behavior when names collide.</param>
    public void AddPlacements(
        IEnumerable<Placement> placements,
        PlacementConflictResolution conflictResolution = PlacementConflictResolution.MergeKeepingNew
    )
    {
        foreach (Placement placement in placements)
        {
            AddPlacement(placement, conflictResolution);
        }
    }

    private void EnsurePlacementMutationAllowed()
    {
        if (State == LoadState.PlacementsLoadStarted)
        {
            throw new InvalidOperationException(
                "Cannot add a placement while placement loading is in progress"
            );
        }
    }

    private bool HandleExistingPlacement(
        Placement newPlacement,
        Placement existing,
        PlacementConflictResolution resolution
    )
    {
        return resolution switch
        {
            PlacementConflictResolution.MergeKeepingNew => MergeKeepingNew(newPlacement, existing),
            PlacementConflictResolution.MergeKeepingOld => MergeKeepingOld(newPlacement, existing),
            PlacementConflictResolution.Replace => ReplacePlacement(newPlacement, existing),
            PlacementConflictResolution.Ignore => false,
            PlacementConflictResolution.Throw => throw new ArgumentException(
                $"A placement named {newPlacement.Name} already exists"
            ),
            _ => throw new NotImplementedException("Unknown conflict resolution mode."),
        };
    }

    private bool AddBrandNewPlacement(Placement placement)
    {
        placements.Add(placement.Name, placement);
        return true;
    }

    private bool MergeKeepingNew(Placement newPlacement, Placement existing)
    {
        newPlacement.Items.AddRange(existing.Items);
        placements[newPlacement.Name] = newPlacement;
        UnloadIfNeeded(existing);
        return true;
    }

    private bool MergeKeepingOld(Placement newPlacement, Placement existing)
    {
        existing.Items.AddRange(newPlacement.Items);
        if (State >= LoadState.PlacementsLoadCompleted)
        {
            foreach (Item item in newPlacement.Items)
            {
                item.LoadOnce();
            }
        }
        return false;
    }

    private bool ReplacePlacement(Placement newPlacement, Placement existing)
    {
        placements[newPlacement.Name] = newPlacement;
        UnloadIfNeeded(existing);
        return true;
    }

    private void UnloadIfNeeded(Placement placement)
    {
        if (State >= LoadState.PlacementsLoadCompleted)
        {
            placement.Unload();
        }
    }

    private void LoadIfNeeded(Placement placement)
    {
        if (State >= LoadState.PlacementsLoadCompleted && placements[placement.Name] == placement)
        {
            placement.LoadOnce();
        }
    }

    [MemberNotNull(nameof(host), nameof(lifecycleInvoker), nameof(gameInvoker))]
    private void AttachHost(ItemChangerHost host)
    {
        host.ActiveProfile = this;
        this.host = host;
        lifecycleInvoker = new LifecycleEvents.Invoker(host.LifecycleEvents);
        gameInvoker = new GameEvents.Invoker(this, host.GameEvents);
    }

    private void DoHook()
    {
        if (hooked)
        {
            return;
        }

        host.GameEvents.Hook();
        host.PrepareEvents(lifecycleInvoker, gameInvoker);
        foreach (Container c in host.ContainerRegistry)
        {
            try
            {
                c.Load();
            }
            catch (Exception e)
            {
                LoggerProxy.LogError($"Error loading container {c.Name}:\n{e}");
            }
        }

        lifecycleInvoker.NotifyHooked();

        hooked = true;
    }

    private void DoUnhook()
    {
        if (!hooked)
        {
            return;
        }

        foreach (Container c in host.ContainerRegistry)
        {
            try
            {
                c.Unload();
            }
            catch (Exception e)
            {
                LoggerProxy.LogError($"Error unloading container {c.Name}:\n{e}");
            }
        }
        host.UnhookEvents(lifecycleInvoker, gameInvoker);
        host.GameEvents.Unhook();

        lifecycleInvoker.NotifyUnhooked();

        hooked = false;
    }
}
