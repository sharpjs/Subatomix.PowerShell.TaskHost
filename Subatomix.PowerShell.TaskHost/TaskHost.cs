// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Globalization;

namespace Subatomix.PowerShell.TaskHost;

/// <summary>
///   A wrapper for <see cref="PSHost"/> to improve the clarity of output from
///   parallel tasks.
/// </summary>
public sealed class TaskHost : PSHost, IDisposable
{
    private static readonly AsyncLocal<TaskHost?> _current = new();

    private readonly PSHost     _host;      // Underlying host implementation
    private readonly TaskHost?  _parent;    // Containing host wrapper if nested
    private readonly TaskHostUI _ui;        // Child UI wrapper
    private readonly Guid       _id;        // Host identifier (random)
    private readonly string     _name;      // Host name

    internal TaskHost(PSHost host, ConsoleState state, int taskId, string? header)
    {
        if (host is TaskHost parent)
        {
            _host   = parent._host;
            _parent = parent;
            _name   = Invariant($"{parent._name}.{taskId}");
        }
        else
        {
            _host = host;
            _name = Invariant($"TaskHost<{host.Name}>#{taskId}");
        }

        _ui = new TaskHostUI(host.UI, state, taskId, header);
        _id = Guid.NewGuid();

        _parent = Current;
        Current = this;
    }

    /// <summary>
    ///   Gets or sets the current task host, or <see langword="null"/>, or
    ///   <see langword="null"/> if no task host is current.
    /// </summary>
    public static TaskHost? Current
    {
        get         => _current.Value;
        private set => _current.Value = value;
    }

    /// <inheritdoc/>
    public override Guid InstanceId
        => _id;

    /// <inheritdoc/>
    public override string Name
        => _name;

    /// <inheritdoc/>
    public override Version Version
        => TaskHostFactory.Version;

    /// <inheritdoc cref="UI" />
    public TaskHostUI TaskHostUI
        => _ui;

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

    /// <inheritdoc/>
    public void Dispose()
    {
        if (Current != this)
            throw new InvalidOperationException(
                "Cannot Dispose the TaskHost object because a nested instance is current. " +
                "Dispose the nested instance first."
            );

        Current = _parent;
    }
}
