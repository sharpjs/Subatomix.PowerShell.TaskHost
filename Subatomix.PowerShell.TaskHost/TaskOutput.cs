// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

/// <summary>
///   A wrapper that carries a <see cref="TaskInfo"/> reference along with an
///   output object.
/// </summary>
internal class TaskOutput
{
    public TaskOutput(TaskInfo task, object? obj)
    {
        if (task is null)
            throw new ArgumentNullException(nameof(task));

        Task   = task;
        Object = obj;
    }

    public TaskInfo Task { get; }

    public object? Object { get; }
}
