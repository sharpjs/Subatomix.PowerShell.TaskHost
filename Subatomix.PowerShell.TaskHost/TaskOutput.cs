// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

/// <summary>
///   A pair consisting of a task associated with an output object.
/// </summary>
public class TaskOutput
{
    /// <summary>
    ///   Initializes a new <see cref="TaskOutput"/> instance with the
    ///   specified task and object.
    /// </summary>
    /// <param name="task">
    ///   The task associated with <paramref name="obj"/>.
    /// </param>
    /// <param name="obj">
    ///   The output object.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="task"/> is <see langword="null"/>.
    /// </exception>
    public TaskOutput(TaskInfo task, object? obj)
    {
        if (task is null)
            throw new ArgumentNullException(nameof(task));

        Task   = task;
        Object = obj;
    }

    /// <summary>
    ///   Gets the task associated with <see cref="Object"/>.
    /// </summary>
    public TaskInfo Task { get; }

    /// <summary>
    ///   Gets the output object.
    /// </summary>
    public object? Object { get; }
}
