// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Collections.ObjectModel;
using System.Security;

namespace Subatomix.PowerShell.TaskHost;

/// <summary>
///   A wrapper for <see cref="PSHostUserInterface"/> to improve the clarity of
///   output from parallel tasks.
/// </summary>
public sealed class TaskHostUI : PSHostUserInterface
{
    private readonly PSHostUserInterface    _ui;            // Underlying UI implementation
    private readonly PSHostRawUserInterface _rawUI;         // Raw UI wrapper
    private readonly ConsoleState           _console;       // Console state shared among tasks
    private readonly int                    _taskId;        // Ordinal identifier of this task
    private          bool                   _taskBol;       // Whether this task should be at BOL
    private          string                 _header;        // Short display name for this task
    private          string?                _headerCache;   // ... pre-rendered for display


    internal TaskHostUI(PSHostUserInterface ui, ConsoleState console, int taskId, string? header)
    {
        _ui      = ui;
        _rawUI   = new TaskHostRawUI(ui.RawUI, console);
        _console = console;
        _taskId  = taskId;
        _taskBol = true;
        _header  = header ?? Invariant($"Task {taskId}");
    }

    /// <summary>
    ///   Gets or sets a header that appears before each line of output.
    /// </summary>
    public string Header
    {
        get => _header;
        set
        {
            lock (_console)
            {
                _header      = value ?? throw new ArgumentNullException(nameof(value));
                _headerCache = null;
            }
        }
    }

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
            Prepare();
            _ui.Write(text);
            Update(EndsWithEol(text));
        }
    }

    /// <inheritdoc/>
    public override void Write(ConsoleColor foreground, ConsoleColor background, string? text)
    {
        lock (_console)
        {
            Prepare();
            _ui.Write(foreground, background, text);
            Update(EndsWithEol(text));
        }
    }

    /// <inheritdoc/>
    public override void WriteLine()
    {
        lock (_console)
        {
            Prepare();
            _ui.WriteLine();
            Update(eol: true);
        }
    }

    /// <inheritdoc/>
    public override void WriteLine(string? text)
    {
        lock (_console)
        {
            Prepare();
            _ui.WriteLine(text);
            Update(eol: true);
        }
    }

    /// <inheritdoc/>
    public override void WriteLine(ConsoleColor foreground, ConsoleColor background, string? text)
    {
        lock (_console)
        {
            Prepare();
            _ui.WriteLine(foreground, background, text);
            Update(eol: true);
        }
    }

    /// <inheritdoc/>
    public override void WriteDebugLine(string? text)
    {
        lock (_console)
        {
            Prepare();
            _ui.WriteDebugLine(text);
            Update(eol: true);
        }
    }

    /// <inheritdoc/>
    public override void WriteVerboseLine(string? text)
    {
        lock (_console)
        {
            Prepare();
            _ui.WriteVerboseLine(text);
            Update(eol: true);
        }
    }

    /// <inheritdoc/>
    public override void WriteWarningLine(string? text)
    {
        lock (_console)
        {
            Prepare();
            _ui.WriteWarningLine(text);
            Update(eol: true);
        }
    }

    /// <inheritdoc/>
    public override void WriteErrorLine(string? text)
    {
        lock (_console)
        {
            Prepare();
            _ui.WriteErrorLine(text);
            Update(eol: true);
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
            Update(eol: true);
            return result;
        }
    }

    /// <inheritdoc/>
    public override SecureString ReadLineAsSecureString()
    {
        lock (_console)
        {
            var result = _ui.ReadLineAsSecureString();
            Update(eol: true);
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

    private void Prepare()
    {
        // Assume locked _console

        if (_console.IsAtBol)
        {
            // The console is at BOL.
        }
        else if (_console.LastTaskId != _taskId)
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
            return;
        }

        PrepareAtBol();
    }

    private void PrepareAtBol()
    {
        // Assume locked _console

        // The console is at BOL.  Emit a line header.  If this task last wrote
        // a partial line that was interrupted by some other task, add a line
        // continuation indicator.

        if (_console.Stopwatch is { Elapsed: var elapsed })
            _ui.Write(
                foregroundColor: ConsoleColor.DarkGray,
                backgroundColor: ConsoleColor.Black,
                string.Format(@"[+{0:hh\:mm\:ss}] ", elapsed)
            );

        if (_header.Length > 0)
            _ui.Write(
                foregroundColor: ConsoleColor.DarkBlue,
                backgroundColor: ConsoleColor.Black,
                _headerCache ??= string.Concat("[", _header, "]: ")
            );

        if (!_taskBol)
            _ui.Write(
                foregroundColor: ConsoleColor.DarkGray,
                backgroundColor: ConsoleColor.Black,
                "(...) "
            );
    }

    private void Update(bool eol)
    {
        // Assume locked _console

        _console.IsAtBol    = _taskBol = eol;
        _console.LastTaskId = _taskId;
    }

    private static bool EndsWithEol(string? value)
        => value != null
        && value.EndsWith("\n", StringComparison.Ordinal);
}
