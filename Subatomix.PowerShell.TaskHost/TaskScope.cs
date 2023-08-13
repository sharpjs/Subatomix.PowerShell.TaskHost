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

    /// <summary>
    ///   Initializes a new <see cref="TaskScope"/> instance for the specified
    ///   task.
    /// </summary>
    /// <param name="task">
    ///   The task to reference.
    /// </param>
    /// <remarks>
    ///   This constructor sets <paramref name="task"/> as the
    ///   <see cref="Current"/> instance and increments the task's retain
    ///   count, ensuring that the task remains accessible via
    ///   <see cref="Task"/> <see cref="All"/> and <see cref="Get"/>.
    /// </remarks>
    public TaskScope(TaskInfo? task)
    {
        task?.Retain();

        _task     = task;
        _previous = Current;
        Current   = task;
    }

    /// <summary>
    ///   Gets the task referenced by the scope.
    /// </summary>
    public TaskInfo? Task => _task;

    /// <summary>
    ///   Disposes the scope.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///   A nested scope has not been disposed.
    /// </exception>
    /// <remarks>
    ///   <para>
    ///     This method restores the previous value of <see cref="Current"/>
    ///     and decrements the <see cref="Task"/>'s retain count.  When all
    ///     scopes for the task have been disposed, the retain count reaches
    ///     zero, and the task becomes inaccessible via <see cref="All"/> and
    ///     <see cref="Get"/>.
    ///   </para>
    ///   <para>
    ///     A nested scope must be disposed before its containing scope.
    ///   </para>
    /// </remarks>
    public void Dispose()
    {
        if (_task is not { } task)
            return;

        if (Current != task)
            throw new InvalidOperationException(
                "Cannot dispose the TaskScope object because a nested TaskScope has not been disposed. " +
                "Dispose the nested instance first."
            );

        Current = _previous;

        task?.Release();
    }
}
