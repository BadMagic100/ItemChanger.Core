using ItemChanger.Serialization;

namespace ItemChanger.Costs;

/// <summary>
/// An integer cost where the source integer is not consumed when the cost is paid.
/// </summary>
public abstract class ThresholdIntCost : Cost
{
    /// <summary>
    /// The amount needed to pay the cost
    /// </summary>
    public required int Amount { get; set; }

    /// <summary>
    /// The value to use to evaluate whether the cost is payable
    /// </summary>
    protected abstract IValueProvider<int> GetValueSource();

    /// <inheritdoc/>
    public override bool CanPay() => Amount >= GetValueSource().Value;

    /// <inheritdoc/>
    public override bool HasPayEffects() => false;

    /// <inheritdoc/>
    public override void OnPay() { }

    /// <inheritdoc/>
    public override bool IsFree => Amount == 0;
}

/// <summary>
/// An integer cost where the source integer is consumed when the cost is paid.
/// </summary>
public abstract class ConsumableIntCost : Cost
{
    /// <summary>
    /// The amount needed to pay the cost
    /// </summary>
    public required int Amount { get; set; }

    /// <summary>
    /// The value to use to evaluate and pay the cost
    /// </summary>
    protected abstract IWritableValueProvider<int> GetValueSource();

    /// <inheritdoc/>
    public override bool CanPay() => Amount >= GetValueSource().Value;

    /// <inheritdoc/>
    public override bool HasPayEffects() => true;

    /// <inheritdoc/>
    public override void OnPay() => GetValueSource().Value -= Amount;

    /// <inheritdoc/>
    public override bool IsFree => Amount == 0;
}
