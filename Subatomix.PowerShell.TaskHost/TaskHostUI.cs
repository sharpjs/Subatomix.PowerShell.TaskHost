// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Security;

namespace Subatomix.PowerShell.TaskHost;

/// <summary>
///   A wrapper for <see cref="PSHostUserInterface"/> to improve the clarity of
///   output from long-running, potentially parallel tasks.
/// </summary>
public sealed class TaskHostUI : PSHostUserInterface
{
    private readonly PSHostUserInterface _ui;           // Underlying UI implementation
    private readonly TaskHostRawUI       _rawUI;        // Child RawUI wrapper
    private readonly ConsoleState        _console;      // Global console state and sync root
    private readonly Stopwatch?          _stopwatch;    // Total elapsed time

    internal TaskHostUI(PSHostUserInterface ui, Stopwatch? stopwatch)
    {
        if (ui is null)
            throw new ArgumentNullException(nameof(ui));

        _ui        = ui;
        _rawUI     = new TaskHostRawUI(_ui.RawUI, _console = new());
        _stopwatch = stopwatch;
    }

    /// <summary>
    ///   Gets the global console state.
    /// </summary>
    internal ConsoleState Console => _console;

    /// <summary>
    ///   Gets the stopwatch from which the instance reports elapsed time, or
    ///   <see langword="null"/> if the instance does not report elapsed time.
    /// </summary>
    internal Stopwatch? Stopwatch { get; set; }

    /// <inheritdoc/>
    public override PSHostRawUserInterface RawUI
        => _rawUI;

    /// <inheritdoc/>
    public override bool SupportsVirtualTerminal
        => _ui.SupportsVirtualTerminal;

    /// <inheritdoc/>
    public override void Write(string? text)
    {
        lock (_console)
        {
            var task = Prepare();
            _ui.Write(text);
            Update(task, EndsWithEol(text));
        }
    }

    /// <inheritdoc/>
    public override void Write(ConsoleColor foreground, ConsoleColor background, string? text)
    {
        lock (_console)
        {
            var task = Prepare();
            _ui.Write(foreground, background, text);
            Update(task, EndsWithEol(text));
        }
    }

    /// <inheritdoc/>
    public override void WriteLine()
    {
        lock (_console)
        {
            var task = Prepare();
            _ui.WriteLine();
            Update(task, eol: true);
        }
    }

    /// <inheritdoc/>
    public override void WriteLine(string? text)
    {
        lock (_console)
        {
            var task = Prepare();
            _ui.WriteLine(text);
            Update(task, eol: true);
        }
    }

    /// <inheritdoc/>
    public override void WriteLine(ConsoleColor foreground, ConsoleColor background, string? text)
    {
        lock (_console)
        {
            var task = Prepare();
            _ui.WriteLine(foreground, background, text);
            Update(task, eol: true);
        }
    }

    /// <inheritdoc/>
    public override void WriteDebugLine(string? text)
    {
        lock (_console)
        {
            var task = Prepare();
            _ui.WriteDebugLine(text);
            Update(task, eol: true);
        }
    }

    /// <inheritdoc/>
    public override void WriteVerboseLine(string? text)
    {
        lock (_console)
        {
            var task = Prepare();
            _ui.WriteVerboseLine(text);
            Update(task, eol: true);
        }
    }

    /// <inheritdoc/>
    public override void WriteWarningLine(string? text)
    {
        lock (_console)
        {
            var task = Prepare();
            _ui.WriteWarningLine(text);
            Update(task, eol: true);
        }
    }

    /// <inheritdoc/>
    public override void WriteErrorLine(string? text)
    {
        lock (_console)
        {
            var task = Prepare();
            _ui.WriteErrorLine(text);
            Update(task, eol: true);
        }
    }

    /// <inheritdoc/>
    public override void WriteInformation(InformationRecord record)
    {
        lock (_console)
        {
            // NOTE: Do not modify record; doing so results in duplicate prefixes
            _ui.WriteInformation(record);
        }
    }

    /// <inheritdoc/>
    public override void WriteProgress(long sourceId, ProgressRecord record)
    {
        lock (_console)
        {
            // NOTE: Do not modify record; it is not presented in the textual log
            _ui.WriteProgress(sourceId, record);
        }
    }

    /// <inheritdoc/>
    public override string ReadLine()
    {
        lock (_console)
        {
            var result = _ui.ReadLine();
            Update(TaskInfo.Current, eol: true);
            return result;
        }
    }

    /// <inheritdoc/>
    public override SecureString ReadLineAsSecureString()
    {
        lock (_console)
        {
            var result = _ui.ReadLineAsSecureString();
            Update(TaskInfo.Current, eol: true);
            return result;
        }
    }

    /// <inheritdoc/>
    public override Dictionary<string, PSObject> Prompt(
        string? caption, string? message, Collection<FieldDescription> descriptions)
    {
        lock (_console)
            return _ui.Prompt(caption, message, descriptions);
    }

    /// <inheritdoc/>
    public override int PromptForChoice(
        string? caption, string? message, Collection<ChoiceDescription> choices, int defaultChoice)
    {
        lock (_console)
            return _ui.PromptForChoice(caption, message, choices, defaultChoice);
    }

    /// <inheritdoc/>
    public override PSCredential PromptForCredential(
        string? caption, string? message, string? userName, string? targetName)
    {
        lock (_console)
            return _ui.PromptForCredential(caption, message, userName, targetName);
    }

    /// <inheritdoc/>
    public override PSCredential PromptForCredential(
        string? caption, string? message, string? userName, string? targetName,
        PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
    {
        lock (_console)
            return _ui.PromptForCredential(
                caption, message, userName, targetName, allowedCredentialTypes, options);
    }

    private TaskInfo? Prepare()
    {
        // Assume locked _console

        var task = TaskInfo.Current;

        if (_console.IsAtBol)
        {
            // The console is at BOL.
        }
        else if (task is not null && _console.LastTaskId != task.Id)
        {
            // The console is not at BOL, because some other task wrote a
            // partial line to it.  End that line, so that this task's text
            // will start on a new line.
            _ui.WriteLine();
            _console.IsAtBol = true;
        }
        else
        {
            // The console is not at BOL, but this task was the last one to
            // write to it.  The console state is as the task expects. There is
            // no need to modify the console state or to emit any header.
            return task;
        }

        PrepareAtBol(task);

        return task;
    }

    private void PrepareAtBol(TaskInfo? task)
    {
        // Assume locked _console

        // The console is at BOL.  Emit a line header.  If this task last wrote
        // a partial line that was interrupted by some other task, add a line
        // continuation indicator.

        // Timestamp
        if (_stopwatch is { Elapsed: var elapsed })
            _ui.Write(
                foregroundColor: ConsoleColor.DarkGray,
                backgroundColor: ConsoleColor.Black,
                string.Format(@"[+{0:hh\:mm\:ss}] ", elapsed)
            );

        if (task is null)
            return;

        // Header
        if (task.FormattedName is { Length: > 0 } formattedName)
            _ui.Write(
                foregroundColor: ConsoleColor.DarkBlue,
                backgroundColor: ConsoleColor.Black,
                formattedName
            );

        // Line continuation marker
        if (!task.IsAtBol)
            _ui.Write(
                foregroundColor: ConsoleColor.DarkGray,
                backgroundColor: ConsoleColor.Black,
                "(...) "
            );
    }

    private void Update(TaskInfo? task, bool eol)
    {
        // Assume locked _console

        if (task is null)
            return;

        _console.IsAtBol    = task.IsAtBol = eol;
        _console.LastTaskId = task.Id;
    }

    private static bool EndsWithEol(string? value)
        => value != null
        && value.EndsWith("\n", StringComparison.Ordinal);
}
