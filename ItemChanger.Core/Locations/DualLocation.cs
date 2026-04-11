using System;
using System.Collections.Generic;
using ItemChanger.Placements;
using ItemChanger.Serialization;
using ItemChanger.Tags;
using Newtonsoft.Json;

namespace ItemChanger.Locations;

/// <summary>
/// Helper location representing a binary choice of locations based on a condition.
/// </summary>
/// <seealso cref="MultiLocation{T}"/>
public class DualLocation : Location
{
    /// <summary>
    /// Enum value representing a boolean
    /// </summary>
    public enum BoolValue
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        True,
        False
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }

    /// <summary>
    /// Wraps a boolean test as a BoolValue provider.
    /// </summary>
    public class BoolSelector : IValueProvider<BoolValue>
    {
        /// <summary>
        /// Gets the value provider that supplies the current test state.
        /// </summary>
        public required IValueProvider<bool> Test { get; init; }

        /// <inheritdoc/>
        [JsonIgnore]
        public BoolValue Value => Test.Value ? BoolValue.True : BoolValue.False;
    }

    /// <inheritdoc/>
    protected override void DoLoad()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    protected override void DoUnload()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// A test to determine which location to use
    /// </summary>
    public required IValueProvider<bool> Test { get; init; }

    /// <summary>
    /// The location to use when <see cref="Test"/> is <code>false</code>
    /// </summary>
    public required Location FalseLocation { get; init; }

    /// <summary>
    /// The location to use when <see cref="Test"/> is <code>true</code>
    /// </summary>
    public required Location TrueLocation { get; init; }

    /// <inheritdoc/>
    public override Placement Wrap()
    {
        return new MultiPlacement<BoolValue>(Name)
        {
            Selector = new BoolSelector { Test = Test },
            Locations = new Dictionary<BoolValue, Location>
            {
                [BoolValue.True] = TrueLocation,
                [BoolValue.False] = FalseLocation,
            },
            Tags = Tags,
            Cost = DefaultCostTag.GetDefaultCost(this),
        };
    }
}
