using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ItemChanger.Containers;

/// <summary>
/// Base class for types which implement creating and editing item containers.
/// </summary>
/// <remarks>
/// Most concrete container types should derive from a parent class which implements a shared hook system
/// and provides its own interface to add give effects to the FSM, rather than directly deriving from Container.
/// </remarks>
/// <example>
/// This example shows a sample implementation of an intermediary container loosely based on the original Container class
/// from Hollow Knight ItemChanger.
///
/// <code>
/// abstract class FsmContainer : Container
/// {
///     protected internal sealed override void Load()
///     {
///         On.PlayMakerFSM.OnEnable += FsmHook;
///     }
///     protected internal sealed override void Unload()
///     {
///         On.PlayMakerFSM.OnEnable -= FsmHook;
///     }
///
///     private void FsmHook(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM fsm)
///     {
///         orig(self);
///
///         ContainerInfo? info = ContainerInfo.FindContainerInfo(fsm.gameObject);
///         if (info == null)
///         {
///             return;
///         }
///
///         if (info.ContainerType == this.ContainerType)
///         {
///             return;
///         }
///
///         if (info.GiveInfo is ContainerGiveInfo gi &amp;&amp; !gi.Applied)
///         {
///             fc.AddGiveEffectToFsm(fsm, gi);
///             gi.Applied = true;
///         }
///
///         if (info.CostInfo is ContainerCostInfo ci &amp;&amp; !ci.Applied)
///         {
///             fc.AddGiveEffectToFsm(fsm, ci);
///             ci.Applied = true;
///         }
///     }
///
///     protected abstract void AddGiveEffectToFsm(PlayMakerFSM fsm, ContainerGiveInfo info);
///     protected virtual void AddCostToFsm(PlayMakerFSM fsm, ContainerCostInfo info) { }
/// }
///
/// class ChestContainer : FsmContainer
/// {
///     public override string Name => "Chest";
///
///     public override bool SupportsInstantiate => true;
///
///     public override uint SupportedCapabilities => ContainerCapabilities.NONE;
///
///     protected override void AddGiveEffectToFsm(PlayMakerFSM fsm, ContainerGiveInfo info)
///     {
///         // ...
///     }
/// }
/// </code>
/// </example>
public abstract class Container
{
    /// <summary>
    /// The unique name of the container.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Returns true if the container can be instantiated. For some containers, this value is variable in which prefabs are available.
    /// </summary>
    /// <remarks>
    /// SupportsInstantiate is intentionally not implemented as part of the bitfield because its variable nature would otherwise
    /// complicate the implementation of <see cref="SupportedCapabilities"/> for consumers.
    /// </remarks>
    public virtual bool SupportsInstantiate => false;

    /// <summary>
    /// Returns true if the container type can be modified in place, or false if it must be re-created.
    /// </summary>
    /// <remarks>
    /// Generally, a container that does not support modification in place should support instantiation.
    /// </remarks>
    public virtual bool SupportsModifyInPlace => false;

    /// <summary>
    /// A bitfield of additional capabilities supported by the container. The lower order 8 bits are reserved by ItemChanger.
    /// </summary>
    /// <seealso cref="ContainerCapabilities"/>
    public abstract uint SupportedCapabilities { get; }

    /// <summary>
    /// Produces a new object of this container type.
    /// </summary>
    /// <param name="info">The container info to be attached to the new container instance.</param>
    /// <exception cref="NotImplementedException">The container does not support instantiation.</exception>
    public virtual GameObject GetNewContainer(ContainerInfo info)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Modifies a GameObject in-place to act as a container of this type by attaching container info and making any other required changes.
    /// </summary>
    /// <param name="obj">The object to be modified.</param>
    /// <param name="info">The container info to be attached.</param>
    /// <exception cref="NotImplementedException">The container does not support in-place modification.</exception>
    public virtual void ModifyContainerInPlace(GameObject obj, ContainerInfo info)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Puts the container in the same position and hierarchy of the target, up to contextually-defined correction.
    /// </summary>
    public virtual void ApplyTargetContext(GameObject obj, GameObject target, Vector3 correction)
    {
        if (target.transform.parent != null)
        {
            obj.transform.SetParent(target.transform.parent);
        }

        obj.transform.position = target.transform.position;
        obj.transform.localPosition = target.transform.localPosition;
        obj.transform.Translate(-correction);
        obj.SetActive(target.activeSelf);
    }

    /// <summary>
    /// Puts the container in the specified position, up to contextually-defined correction.
    /// </summary>
    public virtual void ApplyTargetContext(GameObject obj, Vector3 position, Vector3 correction)
    {
        obj.transform.position = position - correction;
        obj.SetActive(true);
    }

    /// <summary>
    /// Determines whether all specified capabilities are supported.
    /// </summary>
    /// <param name="capabilities">A bitmask representing the capabilities to check.</param>
    ///
    /// <returns><see langword="true"/> if all specified capabilities are supported; otherwise, <see langword="false"/>.</returns>
    public virtual bool SupportsAll(uint capabilities)
    {
        return (SupportedCapabilities & capabilities) == capabilities;
    }

    /// <summary>
    /// Determines whether all specified capabilities are supported.
    /// </summary>
    /// <remarks>
    /// This method evaluates the combined result of the provided capability flags to determine if
    /// all are supported.
    /// </remarks>
    /// <param name="capabilities">
    /// A collection of capability flags represented as unsigned integers. Each flag is combined
    /// using a bitwise OR operation.
    /// </param>
    ///
    /// <returns><see langword="true"/> if all specified capabilities are supported; otherwise, <see langword="false"/>.</returns>
    public bool SupportsAll(IEnumerable<uint> capabilities)
    {
        uint union = capabilities.Aggregate(0u, (acc, next) => acc | next);
        return SupportsAll(union);
    }

    /// <summary>
    /// Hook point to set up global hooks for containers of this type.
    /// </summary>
    protected internal abstract void Load();

    /// <summary>
    /// Hook point to undo hooks prepared in <see cref="Load"/>
    /// </summary>
    protected internal abstract void Unload();
}
