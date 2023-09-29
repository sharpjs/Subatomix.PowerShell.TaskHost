// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

using static TaskEncoding;

/// <summary>
///   A <see cref="Redirector"/> that injects the identity of the current task
///   (<see cref="TaskInfo.Current"/>) into output streams for consumption by
///   <see cref="TaskExtractingRedirector"/>.
/// </summary>
/// <remarks>
///   An injector-extractor pair flows the value of <see cref="TaskInfo.Current"/>
///   across a boundary that does not preserve execution context, such as
///   <c>ForEach-Object -Parallel</c>.
/// </remarks>
internal class TaskInjectingRedirector : Redirector
{
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
    { } 

    /// <inheritdoc/>
    protected override void WriteObject(object? obj)
    {
        base.WriteObject(Inject(obj));
    }

    /// <inheritdoc/>
    protected override void WriteError(ErrorRecord record)
    {
        base.WriteError(Inject(record));
    }

    /// <inheritdoc/>
    protected override void WriteWarning(string? message)
    {
        base.WriteWarning(Inject(message));
    }

    /// <inheritdoc/>
    protected override void WriteVerbose(string? message)
    {
        base.WriteVerbose(Inject(message));
    }

    /// <inheritdoc/>
    protected override void WriteDebug(string? message)
    {
        base.WriteDebug(Inject(message));
    }

    /// <inheritdoc/>
    protected override void WriteInformation(InformationRecord record)
    {
        base.WriteInformation(Inject(record));
    }
}
