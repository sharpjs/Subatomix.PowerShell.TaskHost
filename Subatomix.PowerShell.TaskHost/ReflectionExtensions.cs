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

    public static bool IsPropertyValue(this object obj, string name, Func<object?, bool> predicate)
    {
        if (obj.GetProperty(name) is not { } property)
            return false;

        return predicate(property.GetValue(obj));
    }

    public static object? GetPropertyValue(this object obj, string name)
    {
        return obj
            .GetProperty(name)?
            .GetValue(obj);
    }

    public static bool SetPropertyValue(this object obj, string name, object? value)
    {
        if (obj.GetProperty(name) is not { } property)
            return false;

        property.SetValue(obj, value);
        return true;
    }

    public static object? CreateInstance(this Type type)
    {
        return Activator.CreateInstance(type, nonPublic: true);
    }

    public static object? InvokeMethod(this object obj, string name)
    {
        return obj
            .GetMethod(name)?
            .Invoke(obj, null);
    }

    private static PropertyInfo? GetProperty(this object obj, string name)
    {
        return obj
            .GetType()
            .GetProperty(name, Flags);
    }

    private static MethodInfo? GetMethod(this object obj, string name)
    {
        return obj
            .GetType()
            .GetMethod(name, Flags, binder: null, Type.EmptyTypes, modifiers: null);
    }
}
