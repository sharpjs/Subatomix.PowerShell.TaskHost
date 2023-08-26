// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

/// <summary>
///   A <see cref="Redirector"/> that injects the identity of the current task
///   (<see cref="TaskInfo.Current"/>) into output streams for consumption by
///   <see cref="TaskExtractingRedirector"/>.
/// </summary>
/// <remarks>
///   An injector-extractor pair flows the value of <see cref="TaskInfo.Current"/>
///   across a boundary that does not preserve execution context, such as
///   <c>ForEach-Object-Parallel</c>.
/// </remarks>
internal class TaskInjectingRedirector : Redirector
{
    private readonly TaskInfo _task;
    private readonly string   _header;

    /// <summary>
    ///   Initializes a new <see cref="TaskInjectingRedirector"/> instance.
    /// </summary>
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">
    ///   <see cref="TaskInfo.Current"/> is <see langword="null"/>.
    /// </exception>
    public TaskInjectingRedirector(
        PSDataCollection<PSObject?> output,
        PSDataStreams               streams,
        Cmdlet                      target)
        : base(output, streams, target)
    {
        _task   = TaskInfo.Current ?? throw OnNoCurrentTask();
        _header = Invariant($"\x01{_task.Id}\x02");
    }

    /// <inheritdoc/>
    protected override void WriteObject(object? obj)
    {
        base.WriteObject(Transform(obj));
    }

    /// <inheritdoc/>
    protected override void WriteError(ErrorRecord record)
    {
        base.WriteError(Transform(record));
    }

    /// <inheritdoc/>
    protected override void WriteWarning(string? message)
    {
        base.WriteWarning(Transform(message));
    }

    /// <inheritdoc/>
    protected override void WriteVerbose(string? message)
    {
        base.WriteVerbose(Transform(message));
    }

    /// <inheritdoc/>
    protected override void WriteDebug(string? message)
    {
        base.WriteDebug(Transform(message));
    }

    /// <inheritdoc/>
    protected override void WriteInformation(InformationRecord record)
    {
        base.WriteInformation(Transform(record));
    }

    private object? Transform(object? obj)
    {
        _task.Retain();
        return new TaskOutput(_task, obj);
    }

    private string? Transform(string? message)
    {
        _task.Retain();
        return string.Concat(_header, message);
    }

    private ErrorRecord Transform(ErrorRecord record)
    {
        if (record is null)
            return record!;

        var details = record.ErrorDetails;

        record.ErrorDetails = new(Transform(details?.Message))
        {
            RecommendedAction = details?.RecommendedAction
        };

        return record;
    }

    private InformationRecord Transform(InformationRecord record)
    {
        if (record is null)
            return record!;

        record.Source = Transform(record.Source);

        return record;
    }

    private static Exception OnNoCurrentTask()
    {
        return new InvalidOperationException("There is no current task.");
    }
}
