// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

#pragma warning disable IDE0057 // Use range operator

using System.Globalization;

namespace Subatomix.PowerShell.TaskHost;

/// <summary>
///   Helpers to inject and extract a task identifier into and from objects
///   that do not natively carry task information.
/// </summary>
internal static class TaskEncoding
{
    #region Injection

    public const char
        StartDelimiter = '\x01', // SOH = Start of Heading
        EndDelimiter   = '\x02'; // SOT = Start of Text

    [return: NotNullIfNotNull(nameof(obj))]
    public static object? Inject(object? obj)
    {
        if (TaskInfo.Current is not { } task)
            return obj;

        if (IsInjected(obj))
            return obj;

        return InjectCore(obj, task);
    }

    [return: NotNullIfNotNull(nameof(message))]
    public static string? Inject(string? message)
    {
        if (TaskInfo.Current is not { } task)
            return message;

        message ??= string.Empty;

        if (IsInjected(message))
            return message;

        return InjectCore(message, task);
    }

    [return: NotNullIfNotNull(nameof(record))]
    public static ErrorRecord? Inject(ErrorRecord? record)
    {
        // PowerShell does not send a null record
        if (record is null || TaskInfo.Current is not { } task)
            return record;

        var details = record.ErrorDetails;
        var message = details?.Message ?? string.Empty;

        if (IsInjected(message))
            return record;

        record.ErrorDetails = new(InjectCore(message, task))
        {
            RecommendedAction = details?.RecommendedAction
        };

        return record;
    }

    [return: NotNullIfNotNull(nameof(record))]
    public static InformationRecord? Inject(InformationRecord? record)
    {
        // PowerShell does not send a null record
        if (record is null || TaskInfo.Current is not { } task)
            return record;

        var source = record.Source ?? string.Empty;

        if (IsInjected(source))
            return record;

        record.Source = InjectCore(source, task);

        return record;
    }

    [return: NotNullIfNotNull(nameof(record))]
    public static InformationalRecord? Inject(InformationalRecord? record)
    {
        // PowerShell does not send a null record
        if (record is null || TaskInfo.Current is not { } task)
            return record;

        var message = record.Message ?? string.Empty;

        if (IsInjected(message))
            return record;

        record.Message = InjectCore(message, task);

        return record;
    }

    public static string EncodeId(long id)
    {
        return Invariant($"{StartDelimiter}{id}{EndDelimiter}");
    }

    private static bool IsInjected(object? obj)
    {
        return obj is PSObject { BaseObject: TaskOutput };
    }

    private static bool IsInjected(string message)
    {
        return message.StartsWith(StartDelimiter);
    }

    private static TaskOutput InjectCore(object? obj, TaskInfo task)
    {
        // Injection constitutes a reference
        task.Retain();

        return new TaskOutput(task, obj);
    }

    private static string InjectCore(string message, TaskInfo task)
    {
        // Injection constitutes a reference
        task.Retain();

        return task.EncodedId + message;
    }

    #endregion
    #region Extraction

    public static TaskScope? Extract(ref object? obj)
    {
        if (obj is not PSObject { BaseObject: TaskOutput output })
            return null;

        obj = output.Object;

        return Consign(output.Task);
    }

    public static TaskScope? Extract([NotNullIfNotNull(nameof(message))] ref string? message)
    {
        if (!TryExtractId(ref message, out var id))
            return null;

        return Resolve(id);
    }

    public static TaskScope? Extract([NotNullIfNotNull(nameof(record))] ref ErrorRecord? record)
    {
        if (record is not { ErrorDetails: { Message: var message } details })
            return null;

        if (!TryExtractId(ref message, out var id))
            return null;

        record.ErrorDetails = new(message)
        {
            RecommendedAction = details.RecommendedAction
        };

        return Resolve(id);
    }

    public static TaskScope? Extract([NotNullIfNotNull(nameof(record))] ref InformationRecord? record)
    {
        if (record is not { Source: var source })
            return null;

        if (!TryExtractId(ref source, out var id))
            return null;

        record.Source = source;

        return Resolve(id);
    }

    internal static bool TryExtractId([NotNullIfNotNull(nameof(s))] ref string? s, out long id)
    {
        if (s is not null
            && s.StartsWith(StartDelimiter)
            && s.IndexOf   (EndDelimiter, 1) is (> 1 and var index)
            && long.TryParse(
                s.AsSpan(1, index - 1),
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out id
            ))
        {
            s = s.Substring(index + 1);
            return true;
        }
        else
        {
            id = default;
            return false;
        }
    }

    private static TaskScope? Resolve(long id)
    {
        return TaskInfo.Get(id) is { } task ? Consign(task) : null;
    }

    private static TaskScope Consign(TaskInfo task)
    {
        // Transfer release responsibility to a scope
        var scope = new TaskScope(task);
        task.Release();
        return scope;
    }

    #endregion
}
