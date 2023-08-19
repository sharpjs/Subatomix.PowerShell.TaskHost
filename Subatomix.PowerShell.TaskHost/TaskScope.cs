// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

using static TaskInfo;

/// <summary>
///   A scope in which a particular task is the current task.
/// </summary>
public readonly ref struct TaskScope
{
    private readonly TaskInfo? _task;
    private readonly TaskInfo? _previous;
    private readonly bool      _disposable;

    /// <summary>
    ///   Initializes a new <see cref="TaskScope"/> instance referencing the
    ///   specified task.
    /// </summary>
    /// <param name="task">
    ///   The task to reference.
    /// </param>
    /// <remarks>
    ///   This constructor sets <see cref="Current"/> to the specified
    ///   <paramref name="task"/>.  When the scope is disposed,
    ///   <see cref="Current"/> reverts to its previous value.  When all scopes
    ///   referencing the task are disposed, the task becomes inaccessible via
    ///   <see cref="All"/> and <see cref="Get"/>.
    /// </remarks>
    public TaskScope(TaskInfo? task)
    {
        _task = task;
        task?.Retain();

        _previous = Current;
        Current   = task;

        _disposable = true;
    }

    /// <summary>
    ///   Gets the task referenced by the scope, or <see langword="null"/> for
    ///   the default <see cref="TaskScope"/>.
    /// </summary>
    public TaskInfo? Task => _task;

    /// <summary>
    ///   Disposes the scope.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     When invoked on the default <see cref="TaskScope"/> value, this
    ///     method does nothing.  Otherwise, this method restores the previous
    ///     value of <see cref="Current"/>.  When all scopes referencing a task
    ///     are disposed, the task becomes inaccessible via <see cref="All"/>
    ///     and <see cref="Get"/>.
    ///   </para>
    ///   <para>
    ///     A nested scope must be disposed before its containing scope.
    ///     Scopes may be disposed only once.
    ///   </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///   The scope has been disposed already, or a nested scope has not been
    ///   disposed.
    /// </exception>
    public void Dispose()
    {
        if (!_disposable)
            return;

        if (Current != _task)
            throw new InvalidOperationException(
                "Cannot dispose the TaskScope object because its task is not current. " +
                "Dispose each scope exactly once, and dispose a nested scope before its parent."
            );

        Current = _previous;

        _task?.Release();
    }
}
