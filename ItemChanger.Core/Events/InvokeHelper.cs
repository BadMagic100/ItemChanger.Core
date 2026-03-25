using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ItemChanger.Logging;

namespace ItemChanger.Events;

internal static class InvokeHelper
{
    internal static void InvokeList(List<Action> list, [CallerMemberName] string caller = "")
    {
        LoggerProxy.LogDebug($"Invoking {caller}");
        foreach (Action a in list)
        {
            try
            {
                a?.Invoke();
            }
            catch (Exception e)
            {
                LoggerProxy.LogError($"Error thrown by a subscriber during {caller}:\n{e}");
            }
        }
    }

    internal static void InvokeList<T>(
        T t,
        List<Action<T>> list,
        [CallerMemberName] string caller = ""
    )
    {
        LoggerProxy.LogDebug($"Invoking {caller}");
        foreach (Action<T> a in list)
        {
            try
            {
                a?.Invoke(t);
            }
            catch (Exception e)
            {
                LoggerProxy.LogError(
                    $"Error thrown by a subscriber during {caller} with {t}:\n{e}"
                );
            }
        }
    }

    internal static void InvokeList<T1, T2>(
        T1 t1,
        T2 t2,
        List<Action<T1, T2>> list,
        [CallerMemberName] string caller = ""
    )
    {
        LoggerProxy.LogDebug($"Invoking {caller}");
        foreach (Action<T1, T2> a in list)
        {
            try
            {
                a?.Invoke(t1, t2);
            }
            catch (Exception e)
            {
                LoggerProxy.LogError(
                    $"Error thrown by a subscriber during {caller} with {t1}, {t2}:\n{e}"
                );
            }
        }
    }
}
