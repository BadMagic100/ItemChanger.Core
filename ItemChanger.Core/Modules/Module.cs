using System;
using System.Collections.Generic;
using ItemChanger.Enums;
using ItemChanger.Extensions;
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

    private List<IDisposable> disposables = [];

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
                LoggerProxy.LogError($"Error initializing module of type {GetType()}:\n{e}");
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
                LoggerProxy.LogError($"Error unloading module of type {GetType()}:\n{e}");
            }
            finally
            {
                disposables.DisposeAll();
            }
            Loaded = false;
        }
    }

    /// <summary>
    /// Registers a disposable such as a hook subscription for cleanup when the module unloads.
    /// </summary>
    /// <param name="disposable">The disposable to register</param>
    public void Using(IDisposable disposable)
    {
        disposables.Add(disposable);
    }

    /// <summary>
    /// Additional information for serialization and other tag handling purposes.
    /// </summary>
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public virtual ModuleHandlingFlags ModuleHandlingProperties { get; set; }
}
