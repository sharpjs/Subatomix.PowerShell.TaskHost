// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

/// <summary>
///   Additions to <see cref="Interlocked"/>.
/// </summary>
internal static class InterlockedEx
{
    /// <summary>
    ///   Decrements the specified variable, saturating at zero, and stores the
    ///   result, as an atomic operation.
    /// </summary>
    /// <param name="location">
    ///   The variable to be decremented.
    /// </param>
    /// <returns>
    ///   The decremented value.
    /// </returns>
    [ExcludeFromCodeCoverage] // timing-dependent
    public static long DecrementZeroSaturating(ref long location)
    {
        var value = Volatile.Read(ref location);

        while (value > 0)
        {
            var expected = value;
            var actual   = Interlocked.CompareExchange(ref location, value -= 1, expected);
            if (actual == expected)
                break;

            value = actual;
        }

        return value;
    }
}
