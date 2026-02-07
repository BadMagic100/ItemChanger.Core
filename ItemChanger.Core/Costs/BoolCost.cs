using ItemChanger.Serialization;

namespace ItemChanger.Costs;

/// <summary>
/// A boolean cost requiring the source boolean to be true and has no pay effect.
/// </summary>
public abstract class ThresholdBoolCost : Cost
{
    /// <summary>
    /// The value to use to evaluate whether the cost is payable
    /// </summary>
    protected abstract IValueProvider<bool> GetValueSource();

    /// <inheritdoc/>
    public override bool CanPay() => GetValueSource().Value;

    /// <inheritdoc/>
    public override bool HasPayEffects() => false;

    /// <inheritdoc/>
    public override void OnPay() { }

    /// <inheritdoc/>
    public override bool IsFree => false;
}

/// <summary>
/// A boolean cost requiring the source boolean to be true and sets the boolean to false when the cost is paid.
/// </summary>
public abstract class ConsumableBoolCost : Cost
{
    /// <summary>
    /// The value to use to evaluate and pay the cost
    /// </summary>
    protected abstract IWritableValueProvider<bool> GetValueSource();

    /// <inheritdoc/>
    public override bool CanPay() => GetValueSource().Value;

    /// <inheritdoc/>
    public override bool HasPayEffects() => true;

    /// <inheritdoc/>
    public override void OnPay() => GetValueSource().Value = false;

    /// <inheritdoc/>
    public override bool IsFree => false;
}
