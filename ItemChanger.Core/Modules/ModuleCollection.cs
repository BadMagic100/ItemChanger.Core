using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ItemChanger.Logging;
using ItemChanger.Serialization.Converters;
using Newtonsoft.Json;

namespace ItemChanger.Modules;

/// <summary>
/// Represents the list of modules attached to an <see cref="ItemChangerProfile"/>.
/// </summary>
[JsonConverter(typeof(ModuleCollectionConverter))]
public class ModuleCollection : IEnumerable<Module>
{
    private readonly List<Module> modules;

    /// <inheritdoc/>
    public int Count => modules.Count;

    internal ModuleCollection()
    {
        this.modules = [];
    }

    /// <summary>
    /// Loads each module once in declaration order.
    /// </summary>
    public void Load()
    {
        foreach (Module module in modules)
        {
            module.LoadOnce();
        }
    }

    /// <summary>
    /// Unloads each module once in declaration order.
    /// </summary>
    public void Unload()
    {
        foreach (Module module in modules)
        {
            module.UnloadOnce();
        }
    }

    /// <summary>
    /// Adds a module instance to the collection and loads it when the host profile is active.
    /// </summary>
    /// <param name="instance">Module to add.</param>
    /// <returns>The same module instance for chaining.</returns>
    public T Add<T>(T instance)
        where T : Module
    {
        if (instance == null)
        {
            throw new ArgumentNullException(nameof(instance));
        }

        if (instance.IsSingleton && Get<T>() != null)
        {
            LoggerProxy.LogError(
                $"Attempted to add another instance of singleton module {instance.Name}"
            );
        }
        else
        {
            modules.Add(instance);
            if (
                ItemChangerHost.Singleton.ActiveProfile != null
                && ItemChangerHost.Singleton.ActiveProfile.State
                    >= ItemChangerProfile.LoadState.ModuleLoadCompleted
            )
            {
                instance.LoadOnce();
            }
        }

        return instance;
    }

    /// <summary>
    /// Adds a module created via reflection.
    /// </summary>
    /// <typeparam name="T">Module type to instantiate.</typeparam>
    /// <returns>The new module instance.</returns>
    public T Add<T>()
        where T : Module, new()
    {
        T t = new();
        return Add(t);
    }

    /// <summary>
    /// Adds a module given the runtime type.
    /// </summary>
    /// <param name="T">Module type to instantiate.</param>
    /// <returns>The new module instance.</returns>
    public Module Add(Type T)
    {
        try
        {
            Module m = (Module)Activator.CreateInstance(T)!;
            return Add(m);
        }
        catch (Exception e)
        {
            LoggerProxy.LogError(
                $"Unable to instantiate module of type {T.Name} through reflection:\n{e}"
            );
            throw;
        }
    }

    /// <summary>
    /// Returns the first module of type T, or default.
    /// </summary>
    public T? Get<T>()
    {
        return modules.OfType<T>().FirstOrDefault();
    }

    /// <summary>
    /// Retrieves the module of type <typeparamref name="T"/> or creates one when missing.
    /// </summary>
    public T GetOrAdd<T>()
        where T : Module, new()
    {
        T? t = modules.OfType<T>().FirstOrDefault();
        t ??= Add<T>();

        return t;
    }

    /// <summary>
    /// Retrieves an existing module of the specified type from the collection, or adds the provided instance if none
    /// exists.
    /// </summary>
    /// <typeparam name="T">The type of module to retrieve or add. Must derive from Module and have a parameterless constructor.</typeparam>
    /// <param name="instance">The module instance to add if a module of type T does not already exist in the collection. Cannot be null.</param>
    /// <returns>The existing module of type T if found; otherwise, the instance that was added.</returns>
    public T GetOrAdd<T>(T instance)
        where T : Module
    {
        T? t = modules.OfType<T>().FirstOrDefault();
        if (t == null)
        {
            Add(instance);
            t = instance;
        }
        return t;
    }

    /// <summary>
    /// Retrieves the module matching the provided type or creates one when missing.
    /// </summary>
    public Module GetOrAdd(Type T)
    {
        Module? m = modules.FirstOrDefault(m => T.IsInstanceOfType(m));
        m ??= Add(T);

        return m;
    }

    /// <summary>
    /// Removes a module instance and unloads it if the profile is currently loaded.
    /// </summary>
    public void Remove(Module m)
    {
        if (
            modules.Remove(m)
            && ItemChangerHost.Singleton.ActiveProfile != null
            && ItemChangerHost.Singleton.ActiveProfile.State
                >= ItemChangerProfile.LoadState.ModuleLoadCompleted
        )
        {
            m.UnloadOnce();
        }
    }

    /// <summary>
    /// Removes the first module of type <typeparamref name="T"/>.
    /// </summary>
    public void Remove<T>()
    {
        if (modules.OfType<T>().FirstOrDefault() is Module m)
        {
            Remove(m);
        }
    }

    /// <summary>
    /// Removes all modules with the given type.
    /// </summary>
    public void Remove(Type T)
    {
        if (
            ItemChangerHost.Singleton.ActiveProfile != null
            && ItemChangerHost.Singleton.ActiveProfile.State
                >= ItemChangerProfile.LoadState.ModuleLoadCompleted
        )
        {
            foreach (Module m in modules.Where(m => m.GetType() == T))
            {
                m.UnloadOnce();
            }
        }
        modules.RemoveAll(m => m.GetType() == T);
    }

    /// <summary>
    /// Removes the first module with the provided name.
    /// </summary>
    public void Remove(string name)
    {
        if (modules.FirstOrDefault(m => m.Name == name) is Module m)
        {
            Remove(m);
        }
    }

    /// <inheritdoc/>
    public IEnumerator<Module> GetEnumerator() => modules.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
