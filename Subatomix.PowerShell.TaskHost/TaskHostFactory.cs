/*
    Copyright 2022 Jeffrey Sharp

    Permission to use, copy, modify, and distribute this software for any
    purpose with or without fee is hereby granted, provided that the above
    copyright notice and this permission notice appear in all copies.

    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
    WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
    MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
    ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
    WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
    ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
    OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
*/

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
    ///   Initializes a new <see cref="TaskHostFactory"/> instance that
    ///   creates <see cref="TaskHost"/> objects wrapping the specified
    ///   host.
    /// </summary>
    /// <param name="host">
    ///   The host that created <see cref="TaskHost"/> objects should wrap.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="host"/> or its <see cref="PSHost.UI"/> is
    ///   <see langword="null"/>.
    /// </exception>
    public TaskHostFactory(PSHost host)
    {
        if (host is null)
            throw new ArgumentNullException(nameof(host));
        if (host.UI is null)
            throw new ArgumentNullException(nameof(host) + ".UI");

        _host    = host;
        _console = new();
    }

    /// <summary>
    ///   Gets the version of the <see cref="TaskHostFactory"/>
    ///   implementation.
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
