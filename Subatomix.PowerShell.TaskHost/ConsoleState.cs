// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

/// <summary>
///   Necessary state to keep when multiple tasks are writing concurrently to a
///   textual output console.
/// </summary>
internal class ConsoleState
{
    /// <summary>
    ///   Gets or sets whether the console is at the beginning of a line.
    /// </summary>
    internal bool IsAtBol { get; set; } = true;

    /// <summary>
    ///   Gets or sets the id of the task that most recently wrote to the
    ///   console.
    /// </summary>
    internal int LastTaskId { get; set; }
}
