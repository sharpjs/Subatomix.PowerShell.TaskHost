// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Diagnostics;

namespace Subatomix.PowerShell.TaskHost;

/// <summary>
///   Necessary state to keep when multiple tasks are writing concurrently to a
///   textual output console.
/// </summary>
internal class ConsoleState
{
    /// <summary>
    ///   Gets or sets whether the console is at the beginning of a line.
    ///   The default value is <see langword="true"/>.
    /// </summary>
    internal bool IsAtBol { get; set; } = true;

    /// <summary>
    ///   Gets or sets the identifier of the task that most recently wrote to
    ///   the console.  The default value is <c>0</c>.
    /// </summary>
    internal long LastTaskId { get; set; }

    /// <summary>
    ///   Gets or sets the optional stopwatch used to report elapsed time on
    ///   the console.  The default value is <see langword="null"/>.
    /// </summary>
    internal Stopwatch? Stopwatch { get; set; }
}
