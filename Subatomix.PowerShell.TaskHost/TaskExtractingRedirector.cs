// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Subatomix.PowerShell.TaskHost;

/// <summary>
///   A <see cref="Redirector"/> that extracts the task identity from output
///   streams previously injected by <see cref="TaskInjectingRedirector"/> and
///   restores the value of <see cref="TaskInfo.Current"/>.
/// </summary>
/// <remarks>
///   An injector-extractor pair flows the value of <see cref="TaskInfo.Current"/>
///   across a boundary that does not preserve execution context, such as
///   <c>ForEach-Object-Parallel</c>.
/// </remarks>
internal class TaskExtractingRedirector : Redirector
{
    /// <summary>
    ///   Initializes a new <see cref="TaskExtractingRedirector"/> instance.
    /// </summary>
    /// <inheritdoc/>
    public TaskExtractingRedirector(
        PSDataCollection<PSObject?> output,
        PSDataStreams               streams,
        Cmdlet                      target)
        : base(output, streams, target)
    { }

    /// <inheritdoc/>
    protected override void WriteObject(object? obj)
    {
        using (Extract(ref obj)) base.WriteObject(obj);
    }

    /// <inheritdoc/>
    protected override void WriteError(ErrorRecord record)
    {
        using (Extract(ref record)) base.WriteError(record);
    }

    /// <inheritdoc/>
    protected override void WriteWarning(string? message)
    {
        using (Extract(ref message)) base.WriteWarning(message);
    }

    /// <inheritdoc/>
    protected override void WriteVerbose(string? message)
    {
        using (Extract(ref message)) base.WriteVerbose(message);
    }

    /// <inheritdoc/>
    protected override void WriteDebug(string? message)
    {
        using (Extract(ref message)) base.WriteDebug(message);
    }

    /// <inheritdoc/>
    protected override void WriteInformation(InformationRecord record)
    {
        using (Extract(ref record)) base.WriteInformation(record);
    }

    private TaskScope Extract(ref object? obj)
    {
        var output
            = (obj as PSObject)?.BaseObject as TaskOutput
            ?? obj as TaskOutput;

        if (output is null)
            return default;

        obj = output.Object;
        return new(output.Task);
    }

    private TaskScope Extract(ref string? message)
    {
        if (!TryExtract(ref message, out var task))
            return default;

        return Wrap(task);
    }

    private TaskScope Extract(ref ErrorRecord record)
    {
        if (record is not { ErrorDetails: { Message: var message } details })
            return default;

        if (!TryExtract(ref message, out var task))
            return default;

        record.ErrorDetails = new(message)
        {
            RecommendedAction = details.RecommendedAction
        };

        return Wrap(task);
    }

    private TaskScope Extract(ref InformationRecord record)
    {
        if (record is not { Source: var source })
            return default;

        if (!TryExtract(ref source, out var task))
            return default;

        record.Source = source;

        return Wrap(task);
    }

    private static bool TryExtract(ref string? s, [MaybeNullWhen(false)] out TaskInfo task)
    {
        if (TryExtractId(ref s, out var id)
            && TaskInfo.Get(id) is { } found)
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

    private static bool TryExtractId(ref string? s, out long id)
    {
        const char
            StartDelimiter = '\x01', // SOH = Start of Heading
            EndDelimiter   = '\x02'; // SOT = Start of Text

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
        var scope = new TaskScope(task);
        task.Release();
        return scope;
    }
}
