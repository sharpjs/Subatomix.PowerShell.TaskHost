// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

using static TaskEncoding;

/// <summary>
///   A <see cref="Redirector"/> that extracts the task identity from output
///   streams previously injected by <see cref="TaskInjectingRedirector"/> and
///   restores the value of <see cref="TaskInfo.Current"/>.
/// </summary>
/// <remarks>
///   An injector-extractor pair flows the value of <see cref="TaskInfo.Current"/>
///   across a boundary that does not preserve execution context, such as
///   <c>ForEach-Object -Parallel</c>.
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
}
