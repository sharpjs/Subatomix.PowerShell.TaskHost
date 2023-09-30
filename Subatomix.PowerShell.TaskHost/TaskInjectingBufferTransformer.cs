// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Collections.Concurrent;

namespace Subatomix.PowerShell.TaskHost;

using static TaskEncoding;

/// <summary>
///   A helper that injects the identity of the current <see cref="TaskInfo"/>
///   into the informational streams of the currently executing runspace-level
///   invocation, for consumption in a different runspace by
///   <see cref="TaskExtractingRedirector"/> or <see cref="TaskHostUI"/> .
/// </summary>
/// <remarks>
///   <para>
///     A <see cref="Sma.PowerShell"/> invocation using a new runspace allows
///     the invoking code to pre-arrange the capture of output from all streams
///     not subject to piping/redirection.  However, a child invocation reusing
///     the current runspace (via <see cref="RunspaceMode.CurrentRunspace"/>)
///     may capture only from the output (success) and error streams.  Unless
///     redirected, the warning, information, verbose, and debug streams behave
///     in the invoked code as in the invoking code: the runspace-creating
///     invocation captures their output, or their output flows to the host.
///   </para>
///   <para>
///     On construction, an instance of this class detects if any of the
///     warning, information, verbose, or debug streams are captured by a
///     runspace-creating invocation.  If so, the instance temporarily adds an
///     interceptor to the captured stream(s). The interceptor injects the
///     identity of the current task, if any, into an output object as the
///     object enters the stream.  On disposal, an instance of this class
///     removes any interceptors it added.
///   </para>
///   <para>
///     An injector-extractor pair flows the value of <see cref="TaskInfo.Current"/>
///     across a boundary that does not preserve execution context, such as
///     <c>ForEach-Object -Parallel</c>.
///   </para>
///   <para>
///     ⚠ This class uses a PowerShell internal API.  It is possible that some
///     future version of PowerShell changes that API, breaking this class.  In
///     that case, this class takes care to fail gracefully: task identity is
///     not injected into the affected stream(s).
///   </para>
/// </remarks>
internal sealed class TaskInjectingBufferTransformer : IDisposable
{
    private static readonly ConcurrentDictionary<object, ValueTuple> VisitedBuffers = new();

    private readonly object? _buffers;

    /// <summary>
    ///   Initializes a new <see cref="TaskInjectingBufferTransformer"/>
    ///   instance.
    /// </summary>
    /// <param name="host">
    ///   The current host.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="host"/> is <see langword="null"/>.
    /// </exception>
    public TaskInjectingBufferTransformer(PSHost host)
    {
        if (host is null)
            throw new ArgumentNullException(nameof(host));

        if (host.UI?.InvokeMethod("GetInformationalMessageBuffers") is not { } buffers)
            return;

        if (!VisitedBuffers.TryAdd(buffers, default))
            return;

        _buffers = buffers;

        Hook<WarningRecord>    (buffers, "Warning",     DataAdding_Warning);
        Hook<InformationRecord>(buffers, "Information", DataAdding_Information);
        Hook<VerboseRecord>    (buffers, "Verbose",     DataAdding_Verbose);
        Hook<DebugRecord>      (buffers, "Debug",       DataAdding_Debug);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_buffers is not { } buffers)
            return;

        VisitedBuffers.TryRemove(buffers, out _);

        Unhook<WarningRecord>    (buffers, "Warning",     DataAdding_Warning);
        Unhook<InformationRecord>(buffers, "Information", DataAdding_Information);
        Unhook<VerboseRecord>    (buffers, "Verbose",     DataAdding_Verbose);
        Unhook<DebugRecord>      (buffers, "Debug",       DataAdding_Debug);
    }

    private static void Hook<T>(
        object                            buffers,
        string                            name,
        EventHandler<DataAddingEventArgs> handler)
    {
        if (buffers.GetPropertyValue(name) is not PSDataCollection<T> stream)
            return;

        stream.DataAdding += handler;
    }

    private static void Unhook<T>(
        object                            buffers,
        string                            name,
        EventHandler<DataAddingEventArgs> handler)
    {
        if (buffers.GetPropertyValue(name) is not PSDataCollection<T> stream)
            return;

        stream.DataAdding -= handler;
    }

    // NOTE: If the handlers are made static, Coverlet thinks there is an
    //       uncovered branch in each Unhook call in Dispose.

    private void DataAdding_Warning(object? sender, DataAddingEventArgs e)
    {
        Inject(e.ItemAdded as WarningRecord);
    }

    private void DataAdding_Information(object? sender, DataAddingEventArgs e)
    {
        Inject(e.ItemAdded as InformationRecord);
    }

    private void DataAdding_Verbose(object? sender, DataAddingEventArgs e)
    {
        Inject(e.ItemAdded as VerboseRecord);
    }

    private void DataAdding_Debug(object? sender, DataAddingEventArgs e)
    {
        Inject(e.ItemAdded as DebugRecord);
    }
}
