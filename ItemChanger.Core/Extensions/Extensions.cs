using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ItemChanger.Enums;
using ItemChanger.Items;

namespace ItemChanger.Extensions;

/// <summary>
/// Miscellaneous extensions, primarily on System types.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// An implementation of GetValueOrDefault, which apparently does not exist pre-.net6
    /// </summary>
    public static TValue? GetOrDefault<TKey, TValue>(
        this Dictionary<TKey, TValue> dict,
        TKey key,
        TValue? defaultValue = default
    )
        where TKey : notnull
    {
        if (dict.TryGetValue(key, out TValue? value))
        {
            return value;
        }
        return defaultValue;
    }

    /// <summary>
    /// Truncates the string to the specified length, if necessary.
    /// </summary>
    public static string CapLength(this string s, int length) =>
        s.Length > length ? s.Substring(0, length) : s;

    /// <summary>
    /// Returns true when the collection has a previously given item, or is null or empty.
    /// </summary>
    public static bool AnyEverObtained(this IEnumerable<Item> items)
    {
        return items == null || !items.Any() || items.Any(i => i.WasEverObtained());
    }

    /// <summary>
    /// Compares two values using the provided operator.
    /// </summary>
    public static bool Compare<T>(this T t, ComparisonOperator op, T u)
        where T : IComparable
    {
        return op switch
        {
            ComparisonOperator.Neq => t.CompareTo(u) != 0,
            ComparisonOperator.Ge => t.CompareTo(u) >= 0,
            ComparisonOperator.Gt => t.CompareTo(u) > 0,
            ComparisonOperator.Le => t.CompareTo(u) <= 0,
            ComparisonOperator.Lt => t.CompareTo(u) < 0,
            _ => t.CompareTo(u) == 0,
        };
    }

    /// <summary>
    /// Compares two integers using the provided operator.
    /// </summary>
    public static bool Compare(this int a, ComparisonOperator op, int b)
    {
        return op switch
        {
            ComparisonOperator.Neq => a != b,
            ComparisonOperator.Ge => a >= b,
            ComparisonOperator.Gt => a > b,
            ComparisonOperator.Le => a <= b,
            ComparisonOperator.Lt => a < b,
            _ => a == b,
        };
    }

    /// <summary>
    /// Returns the zero-based index of <paramref name="t"/> within the sequence.
    /// </summary>
    public static int IndexOf<T>(this IEnumerable<T> ts, T t)
    {
        return ts.TakeWhile(u => !EqualityComparer<T>.Default.Equals(u, t)).Count();
    }

    /// <summary>
    /// Returns a single-element enumerable containing <paramref name="t"/>.
    /// </summary>
    public static IEnumerable<T> Yield<T>(this T t)
    {
        yield return t;
    }

    internal static string FromCamelCase(this string str)
    {
        StringBuilder uiname = new StringBuilder(str);
        if (str.Length > 0)
        {
            uiname[0] = char.ToUpper(uiname[0], CultureInfo.InvariantCulture);
        }

        for (int i = 1; i < uiname.Length; i++)
        {
            if (
                char.IsUpper(uiname[i])
                && (
                    char.IsLower(uiname[i - 1])
                    || (i + 1 < uiname.Length && char.IsLower(uiname[i + 1]))
                )
            )
            {
                uiname.Insert(i++, " ");
            }

            if (
                char.IsDigit(uiname[i])
                && !char.IsDigit(uiname[i - 1])
                && !char.IsWhiteSpace(uiname[i - 1])
            )
            {
                uiname.Insert(i, " ");
            }
        }

        return uiname.ToString();
    }
}
