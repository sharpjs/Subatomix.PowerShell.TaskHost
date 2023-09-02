// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Diagnostics;
using System.Globalization;

namespace Subatomix.PowerShell.TaskHost;

/// <summary>
///   A wrapper for <see cref="PSHost"/> to improve the clarity of output from
///   long-running, potentially parallel tasks.
/// </summary>
public sealed class TaskHost : PSHost
{
    private readonly PSHost     _host;  // Underlying host implementation
    private readonly TaskHostUI _ui;    // Child UI wrapper
    private readonly Guid       _id;    // Host identifier (random)
    private readonly string     _name;  // Host name

    /// <summary>
    ///   Initializes a new <see cref="TaskHost"/> instance wrapping the
    ///   specified host, optionally reporting elapsed time from the specified
    ///   stopwatch.
    /// </summary>
    /// <param name="host">
    ///   The host to wrap.
    /// </param>
    /// <param name="stopwatch">
    ///   A stopwatch from which to report elapsed time, or
    ///   <see langword="null"/> to not report elapsed time.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="host"/>,
    ///   its <see cref="PSHost.UI"/>, and/or
    ///   its <see cref="PSHostUserInterface.RawUI"/>
    ///   is <see langword="null"/>.
    /// </exception>
    public TaskHost(PSHost host, Stopwatch? stopwatch = null)
    {
        if (host is null)
            throw new ArgumentNullException(nameof(host));

        _host = host.GetPropertyValue("ExternalHost") as PSHost ?? host;
        _ui   = new TaskHostUI(_host.UI, stopwatch);
        _id   = Guid.NewGuid();
        _name = string.Concat("TaskHost<", _host.Name, ">");
    }

    /// <summary>
    ///   Initializes a new <see cref="TaskHost"/> instance wrapping the
    ///   specified host, optionally reporting elapsed time since construction.
    /// </summary>
    /// <param name="host">
    ///   The host to wrap.
    /// </param>
    /// <param name="withElapsed">
    ///   <see langword="true"/> to report elapsed time since construction;
    ///   <see langword="false"/> otherwise.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="host"/>,
    ///   its <see cref="PSHost.UI"/>, and/or
    ///   its <see cref="PSHostUserInterface.RawUI"/>
    ///   is <see langword="null"/>.
    /// </exception>
    public TaskHost(PSHost host, bool withElapsed)
        : this(host, withElapsed ? Stopwatch.StartNew() : null) { }

    /// <inheritdoc/>
    public override Guid InstanceId
        => _id;

    /// <inheritdoc/>
    public override string Name
        => _name;

    /// <inheritdoc/>
    public override Version Version
        => _version;

    private static readonly Version _version
        = typeof(TaskHost).Assembly.GetName().Version!;

    /// <inheritdoc/>
    public override PSHostUserInterface UI
        => _ui;

    /// <inheritdoc/>
    public override CultureInfo CurrentCulture
        => _host.CurrentCulture;

    /// <inheritdoc/>
    public override CultureInfo CurrentUICulture
        => _host.CurrentUICulture;

    /// <inheritdoc/>
    public override PSObject PrivateData
        => _host.PrivateData;

    /// <inheritdoc/>
    public override bool DebuggerEnabled
    {
        get => _host.DebuggerEnabled;
        set => _host.DebuggerEnabled = value;
    }

    /// <inheritdoc/>
    public override void EnterNestedPrompt()
        => _host.EnterNestedPrompt();

    /// <inheritdoc/>
    public override void ExitNestedPrompt()
        => _host.ExitNestedPrompt();

    /// <inheritdoc/>
    public override void NotifyBeginApplication()
        => _host.NotifyBeginApplication();

    /// <inheritdoc/>
    public override void NotifyEndApplication()
        => _host.NotifyEndApplication();

    /// <inheritdoc/>
    public override void SetShouldExit(int exitCode)
        => _host.SetShouldExit(exitCode);
}
