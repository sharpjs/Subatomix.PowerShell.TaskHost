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

    private const char
        StartDelimiter = '\x01', // SOH = Start of Heading
        EndDelimiter   = '\x02'; // SOT = Start of Text

    [return: NotNullIfNotNull(nameof(obj))]
    public static object? Inject(object? obj)
    {
        if (TaskInfo.Current is not { } task)
            return obj;

        return InjectCore(obj, task);
    }

    [return: NotNullIfNotNull(nameof(message))]
    public static string? Inject(string? message)
    {
        if (TaskInfo.Current is not { } task)
            return message;

        message ??= string.Empty;

        return InjectCore(message, task);
    }

    [return: NotNullIfNotNull(nameof(record))]
    public static ErrorRecord? Inject(ErrorRecord? record)
    {
        // PowerShell does not send a null record.
        if (record is null || TaskInfo.Current is not { } task)
            return record;

        var details = record.ErrorDetails;
        var message = details?.Message ?? string.Empty;

        record.ErrorDetails = new(InjectCore(message, task))
        {
            RecommendedAction = details?.RecommendedAction
        };

        return record;
    }

    [return: NotNullIfNotNull(nameof(record))]
    public static InformationRecord? Inject(InformationRecord? record)
    {
        // PowerShell does not send a null record.
        if (record is null || TaskInfo.Current is not { } task)
            return record;

        var source = record.Source ?? string.Empty;

        record.Source = InjectCore(source, task);

        return record;
    }

    public static string EncodeId(long id)
    {
        return Invariant($"{StartDelimiter}{id}{EndDelimiter}");
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
        return TryExtract(ref obj, out var task) ? Wrap(task) : default;
    }

    public static TaskScope? Extract(ref string? message)
    {
        return TryExtract(ref message, out var task) ? Wrap(task) : default;
    }

    public static TaskScope? Extract(ref ErrorRecord record)
    {
        if (record is not { ErrorDetails: { Message: var message } details })
            return default;

        var found = TryExtract(ref message, out var task);

        record.ErrorDetails = new(message)
        {
            RecommendedAction = details.RecommendedAction
        };

        return found ? Wrap(task!) : default;
    }

    public static TaskScope? Extract(ref InformationRecord record)
    {
        if (record is not { Source: var source })
            return default;

        var found = TryExtract(ref source, out var task);

        record.Source = source;

        return found ? Wrap(task!) : default;
    }

    private static bool TryExtract(ref object? obj, [MaybeNullWhen(false)] out TaskInfo task)
    {
        if (obj is PSObject { BaseObject: TaskOutput output })
        {
            obj  = output.Object;
            task = output.Task;
            return true;
        }
        else
        {
            task = null;
            return false;
        }
    }

    private static bool TryExtract(ref string? s, [MaybeNullWhen(false)] out TaskInfo task)
    {
        if (TryExtractId(ref s, out var id) && TaskInfo.Get(id) is { } found)
        {
            task = found;
            return true;
        }
        else
        {
            task = null;
            return false;
        }
    }

    internal static bool TryExtractId(ref string? s, out long id)
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

    private static TaskScope Wrap(TaskInfo task)
    {
        // Transfer release responsibility to a scope
        var scope = new TaskScope(task);
        task.Release();
        return scope;
    }

    #endregion
}
