using System;
using ItemChanger.Logging;
using Newtonsoft.Json;

namespace ItemChanger.Costs;

/// <summary>
/// Data type used generally for cost handling, including in shops and y/n dialogue prompts.
/// </summary>
public abstract class Cost : IFinderCloneable
{
    /// <summary>
    /// Whether the cost has been loaded
    /// </summary>
    [JsonIgnore]
    public bool Loaded { get; private set; }

    /// <summary>
    /// Method to implement optional loading logic, called once during loading.
    /// </summary>
    protected virtual void DoLoad() { }

    /// <summary>
    /// Method to implement optional unloading logic, called once during unloading.
    /// </summary>
    protected virtual void DoUnload() { }

    /// <summary>
    /// Loads the cost. If the cost is already loaded, does nothing.
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
                LoggerProxy.LogError($"Error loading cost {this}:\n{e}");
            }
            Loaded = true;
        }
    }

    /// <summary>
    /// Unloads the cost. If the cost is already loaded, does nothing.
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
                LoggerProxy.LogError($"Error unloading cost {this}:\n{e}");
            }
            Loaded = false;
        }
    }

    /// <summary>
    /// Returns whether the cost can currently be paid.
    /// </summary>
    public abstract bool CanPay();

    /// <summary>
    /// Pays the cost, performing any effects and setting the cost to Paid.
    /// </summary>
    public void Pay()
    {
        if (Paid)
        {
            return;
        }

        OnPay();
        if (!Recurring)
        {
            Paid = true;
        }

        AfterPay();
    }

    /// <summary>
    /// Method for administering all effects of the cost during Pay.
    /// </summary>
    public abstract void OnPay();

    /// <summary>
    /// Method for any effects which should take place after the cost has been paid (e.g. conditionally setting Paid, etc).
    /// </summary>
    public virtual void AfterPay() { }

    /// <summary>
    /// Represents whether the cost has been paid yet. Paid costs will be subsequently ignored.
    /// </summary>
    public bool Paid { get; set; }

    /// <summary>
    /// If true, the cost will not set the value of Paid during Pay. Use for costs which are expected to be paid multiple times.
    /// <br/>Note that Paid can still be set independently to indicate when the cost should no longer be required.
    /// </summary>
    public virtual bool Recurring { get; set; }

    /// <summary>
    /// A number between 0 and 1 which modifies numeric costs. Only considered by some costs.
    /// <br/>For example, the Leg Eater dung discount sets this to 0.8, to indicate that geo costs should be at 80% price.
    /// </summary>
    public virtual float DiscountRate { get; set; } = 1.0f;

    /// <summary>
    /// Whether the cost is free, i.e. requires nothing from the player.
    /// </summary>
    [JsonIgnore]
    public abstract bool IsFree { get; }

    /// <summary>
    /// Method which provides the cost text used in y/n prompts.
    /// </summary>
    public abstract string GetCostText();

    /// <summary>
    /// Points to the root-level cost for pattern-matching contexts such as CostDisplayer. Primarily intended
    /// for implementation by costs which wrap a single other cost to apply additional functionality.
    /// </summary>
    /// <remarks>
    /// Implementers of wrapper costs should keep in mind that costs being wrapped may themselves be wrapper costs.
    /// A typical correct implementation would likely be `WrappedCost.GetBaseCost()`.
    /// </remarks>
    public virtual Cost GetBaseCost() => this;

    /// <summary>
    /// Does paying this cost have effects (particularly that could prevent paying other costs of the same type)?
    /// </summary>
    public abstract bool HasPayEffects();

    /// <summary>
    /// Combines two costs into a MultiCost. If either argument is null, returns the other argument. If one or both costs is a MultiCost, flattens the result.
    /// </summary>
    public static Cost operator +(Cost a, Cost b)
    {
        if (a == null)
        {
            return b;
        }

        if (b == null)
        {
            return a;
        }

        return new MultiCost(a, b);
    }
}
