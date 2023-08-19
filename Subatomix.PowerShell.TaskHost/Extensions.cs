// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

/// <summary>
///   Extension methods.
/// </summary>
public static class Extensions
{
    /// <summary>
    ///   Returns an array containing the non-<see langword="null"/> elements
    ///   of the specified array.
    /// </summary>
    /// <typeparam name="T">
    ///   The type of elements in <paramref name="array"/>.
    /// </typeparam>
    /// <param name="array">
    ///   The array to sanitize.
    /// </param>
    /// <returns>
    ///   An array containing the non-<see langword="null"/> elements of
    ///   <paramref name="array"/>, if <paramref name="array"/> itself is not
    ///   <see langword="null"/>; otherwise, an empty array.
    /// </returns>
    public static T[] Sanitize<T>(this T?[]? array)
        where T : class
    {
        if (array is null)
            return Array.Empty<T>();

        var count = 0;

        foreach (var item in array)
            if (item is not null)
                count++;

        if (count == array.Length)
            return array!;

        var result = new T[count];
            count  = 0;

        foreach (var item in array)
            if (item is not null)
                result[count++] = item;

        return result;
    }
}
