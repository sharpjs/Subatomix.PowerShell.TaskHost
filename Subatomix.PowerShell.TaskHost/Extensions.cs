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

    /// <summary>
    ///   Gets the underlying host if the specified host is an instance of the
    ///   PowerShell <c>InternalPSHost</c> wrapper class.  Otherwise, returns
    ///   the specified host.
    /// </summary>
    /// <param name="host">
    ///   The host instance to unwrap.
    /// </param>
    /// <returns>
    ///   The underlying host, if <paramref name="host"/> is an instance of
    ///   <c>InternalPSHost</c>; otherwise, <paramref name="host"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="host"/> is <see langword="null"/>.
    /// </exception>
    internal static PSHost Unwrap(this PSHost host)
    {
        if (host is null)
            throw new ArgumentNullException(nameof(host));

        return host.GetPropertyValue("ExternalHost") as PSHost ?? host;
    }
}
