namespace ItemChanger.Enums;

/// <summary>
/// Enum for controlling how items should be flung from a location.
/// </summary>
public enum FlingType
{
    /// <summary>
    /// Any fling behavior is acceptable.
    /// </summary>
    Everywhere,

    /// <summary>
    /// Items should not be flung horizontally.
    /// </summary>
    StraightUp,

    /// <summary>
    /// Items should not be flung at all.
    /// </summary>
    DirectDeposit,
}
