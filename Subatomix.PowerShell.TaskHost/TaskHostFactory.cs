// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Diagnostics;

namespace Subatomix.PowerShell.TaskHost;

/// <summary>
///   A factory to create <see cref="TaskHost"/> instances.
/// </summary>
public class TaskHostFactory
{
    private readonly PSHost       _host;    // Host to be wrapped
    private readonly ConsoleState _console; // Overall console state
    private int                   _taskId;  // Counter for task IDs

    /// <summary>
    ///   Initializes a new <see cref="TaskHostFactory"/> instance that creates
    ///   <see cref="TaskHost"/> object that wrap the specified host.
    /// </summary>
    /// <param name="host">
    ///   The host that created <see cref="TaskHost"/> objects should wrap.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="host"/> or its <see cref="PSHost.UI"/> is
    ///   <see langword="null"/>.
    /// </exception>
    public TaskHostFactory(PSHost host)
        : this(host, null) { }

    /// <summary>
    ///   Initializes a new <see cref="TaskHostFactory"/> instance that creates
    ///   <see cref="TaskHost"/> objects that wrap the specified host and that
    ///   optionally report time elapsed since construction of the factory.
    /// </summary>
    /// <param name="host">
    ///   The host that created <see cref="TaskHost"/> objects should wrap.
    /// </param>
    /// <param name="withElapsed">
    ///   <see langword="true"/>  to report elapsed time;
    ///   <see langword="false"/> otherwise.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="host"/> or its <see cref="PSHost.UI"/> is
    ///   <see langword="null"/>.
    /// </exception>
    public TaskHostFactory(PSHost host, bool withElapsed)
        : this(host, withElapsed ? Stopwatch.StartNew() : null) { }

    /// <summary>
    ///   Initializes a new <see cref="TaskHostFactory"/> instance that creates
    ///   <see cref="TaskHost"/> objects that wrap the specified host and that
    ///   optionally report time elapsed measured by the specified stopwatch.
    /// </summary>
    /// <param name="host">
    ///   The host that created <see cref="TaskHost"/> objects should wrap.
    /// </param>
    /// <param name="stopwatch">
    ///   The stopwatch to measure elapsed time, or <see langword="null"/> to
    ///   not report elapsed time.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="host"/> or its <see cref="PSHost.UI"/> is
    ///   <see langword="null"/>.
    /// </exception>
    public TaskHostFactory(PSHost host, Stopwatch? stopwatch)
    {
        if (host is null)
            throw new ArgumentNullException(nameof(host));
        if (host.UI is null)
            throw new ArgumentNullException(nameof(host) + ".UI");
        if (host.UI.RawUI is null)
            throw new ArgumentNullException(nameof(host) + ".UI.RawUI");

        if (TaskHost.Current is { } parent)
        {
            _host    = parent;
            _console = parent.TaskHostUI.Console;
        }
        else
        {
            _host    = host;
            _console = new() { Stopwatch = stopwatch };
        }
    }

    /// <summary>
    ///   Gets the version of the <see cref="TaskHostFactory"/> implementation.
    /// </summary>
    public static Version Version { get; }
        = typeof(TaskHost).Assembly.GetName().Version!;

    /// <summary>
    ///   Creates a new <see cref="TaskHost"/> instance.
    /// </summary>
    /// <param name="header">
    ///   A custom header that appears before each line of output.  If
    ///   provided, this parameter overrides the default generated header.
    ///   To change the header later, set <see cref="TaskHostUI.Header"/>
    ///   to the desired header value.
    /// </param>
    public TaskHost Create(string? header = null)
    {
        var taskId = Interlocked.Increment(ref _taskId);

        return new TaskHost(_host, _console, taskId, header);
    }
}
