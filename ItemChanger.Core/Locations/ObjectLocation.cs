using System;
using ItemChanger.Containers;
using ItemChanger.Extensions;
using ItemChanger.Tags;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ItemChanger.Locations;

/// <summary>
/// Location that uses an existing scene object as the anchor for a container.
/// </summary>
public class ObjectLocation : ContainerLocation, IReplaceableLocation
{
    /// <summary>
    /// Path of the in-scene object to replace or modify.
    /// </summary>
    public required string ObjectName { get; init; }

    /// <summary>
    /// Offset correction applied when placing a new container.
    /// </summary>
    public required Vector3 Correction { get; init; }

    /// <inheritdoc/>
    protected override void DoLoad()
    {
        ItemChangerHost.Singleton.GameEvents.AddSceneEdit(UnsafeSceneName, OnSceneLoaded);
    }

    /// <inheritdoc/>
    protected override void DoUnload()
    {
        ItemChangerHost.Singleton.GameEvents.RemoveSceneEdit(UnsafeSceneName, OnSceneLoaded);
    }

    /// <summary>
    /// Invoked when the target scene is loaded so the container can be applied.
    /// </summary>
    protected virtual void OnSceneLoaded(Scene scene)
    {
        base.GetContainer(scene, out Container container, out ContainerInfo info);
        if (container.Name == OriginalContainerType && container.SupportsModifyInPlace)
        {
            ModifyContainerInPlace(scene, container, info);
        }
        else
        {
            ReplaceWithContainer(scene, container, info);
        }
    }

    /// <summary>
    /// Modifies the existing container object in place.
    /// </summary>
    protected virtual void ModifyContainerInPlace(
        Scene scene,
        Container container,
        ContainerInfo info
    )
    {
        GameObject target = FindObject(scene, ObjectName);
        container.ModifyContainerInPlace(target, info);
    }

    /// <summary>
    /// Replaces the target object with a new container.
    /// </summary>
    public virtual void ReplaceWithContainer(Scene scene, Container container, ContainerInfo info)
    {
        GameObject target = FindObject(scene, ObjectName);
        GameObject newContainer = container.GetNewContainer(info);
        container.ApplyTargetContext(newContainer, target, Correction);
        UnityEngine.Object.Destroy(target);
        foreach (IActionOnContainerReplaceTag tag in GetTags<IActionOnContainerReplaceTag>())
        {
            tag.OnReplace(scene);
        }
    }

    /// <summary>
    /// Finds the target object by name or path in the given scene.
    /// </summary>
    protected static GameObject FindObject(Scene scene, string objectName)
    {
        GameObject? candidate;
        if (!objectName.Contains("/"))
        {
            candidate = scene.FindGameObjectByName(objectName);
        }
        else
        {
            candidate = scene.FindGameObject(objectName);
        }

        if (candidate == null)
        {
            throw new ArgumentException($"{objectName} does not exist in {scene.name}");
        }
        return candidate;
    }
}
