// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Reflection;

namespace Subatomix.PowerShell.TaskHost;

internal static class ReflectionExtensions
{
    const BindingFlags Flags
        = BindingFlags.Instance
        | BindingFlags.Public
        | BindingFlags.NonPublic;

    public static object? GetPropertyValue(this object obj, string name)
    {
        return obj
            .GetType()
            .GetProperty(name, Flags)?
            .GetValue(obj);
    }
}
