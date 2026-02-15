using System;
using ItemChanger.Enums;
using ItemChanger.Logging;
using Newtonsoft.Json;

namespace ItemChanger.Modules;

/// <summary>
/// Base type for classes which perform self-contained changes that should be applied when a save is created or continued and disabled when the save is unloaded.
/// </summary>
#pragma warning disable CA1716 // Identifiers should not match keywords (sorry VB users, your suffering is self-inflicted as usual)
public abstract class Module
#pragma warning restore CA1716
{
    /// <summary>
    /// Whether the module is loaded.
    /// </summary>
    [JsonIgnore]
    public bool Loaded { get; private set; }

    /// <summary>
    /// Human-readable name for the module, defaulting to the type name.
    /// </summary>
    public string Name => GetType().Name;

    /// <summary>
    /// Method allowing derived classes to perform loading logic. Called once during loading.
    /// </summary>
    protected abstract void DoLoad();

    /// <summary>
    /// Method allowing derived classes to perform unloading logic. Called once during unloading.
    /// </summary>
    protected abstract void DoUnload();

    /// <summary>
    /// Loads the module. If the module is already loaded, does nothing.
    /// </summary>
    public void LoadOnce()
    {
        if (!Loaded)
        {
            try
            {
                DoLoad();
            }
            catch (Exception e)
            {
                LoggerProxy.LogError($"Error initializing module {Name}:\n{e}");
            }
            Loaded = true;
        }
    }

    /// <summary>
    /// Unloads the module. If the module is not loaded, does nothing.
    /// </summary>
    public void UnloadOnce()
    {
        if (Loaded)
        {
            try
            {
                DoUnload();
            }
            catch (Exception e)
            {
                LoggerProxy.LogError($"Error unloading module {Name}:\n{e}");
            }
            Loaded = false;
        }
    }

    /// <summary>
    /// Additional information for serialization and other tag handling purposes.
    /// </summary>
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public virtual ModuleHandlingFlags ModuleHandlingProperties { get; set; }
}
