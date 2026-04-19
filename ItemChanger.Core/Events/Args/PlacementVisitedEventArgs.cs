using System;
using ItemChanger.Enums;
using ItemChanger.Placements;

namespace ItemChanger.Events.Args;

/// <summary>
/// Event arguments describing a visit to a placement
/// </summary>
public class PlacementVisitedEventArgs(Placement placement, VisitState proposedNewFlags) : EventArgs
{
    /// <summary>
    /// Placement whose state changed.
    /// </summary>
    public Placement Placement => placement;

    /// <summary>
    /// Flags that were set before the change.
    /// </summary>
    public VisitState Orig => placement.Visited;

    /// <summary>
    /// Flags being applied in this change.
    /// </summary>
    public VisitState ProposedNewFlags => proposedNewFlags;

    /// <summary>
    /// The final flags expected after the change is applied.
    /// </summary>
    public VisitState ModifiedFlags => Orig | ProposedNewFlags;

    /// <summary>
    /// Whether any change has occurred to the visit state
    /// </summary>
    public bool HasChangedAny => ModifiedFlags != Orig;

    /// <summary>
    /// Whether any change has occurred to the specified flags of the visit state
    /// </summary>
    /// <param name="checkFlags">The flags to check for changes</param>
    public bool HasChangedFlags(VisitState checkFlags)
    {
        VisitState maskedOrig = Orig & checkFlags;
        VisitState maskedModified = ModifiedFlags & checkFlags;
        return maskedOrig != maskedModified;
    }
}
