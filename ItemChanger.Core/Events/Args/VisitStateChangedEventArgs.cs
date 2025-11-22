using System;
using ItemChanger.Enums;
using ItemChanger.Placements;

namespace ItemChanger.Events.Args;

/// <summary>
/// Event arguments describing changes to a placement's visit-state flags.
/// </summary>
public class VisitStateChangedEventArgs(Placement placement, VisitState newFlags) : EventArgs
{
    /// <summary>
    /// Placement whose state changed.
    /// </summary>
    public Placement Placement { get; } = placement;

    /// <summary>
    /// Flags that were set before the change.
    /// </summary>
    public VisitState Orig { get; } = placement.Visited;

    /// <summary>
    /// Flags being applied in this change.
    /// </summary>
    public VisitState NewFlags { get; } = newFlags;

    /// <summary>
    /// Returns true when the new flags are already part of the original state.
    /// </summary>
    public bool NoChange => (NewFlags & Orig) == NewFlags;
}
