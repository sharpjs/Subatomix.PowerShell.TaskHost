// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

internal class TaskHostRawUI : PSHostRawUserInterface
{
    private readonly PSHostRawUserInterface _ui;    // Underlying implementation
    private readonly object                 _lock;  // Synchronization object

    public TaskHostRawUI(PSHostRawUserInterface ui, object lockable)
    {
        if (ui is null)
            throw new ArgumentNullException(nameof(ui));
        if (lockable is null)
            throw new ArgumentNullException(nameof(lockable));

        _ui   = ui;
        _lock = lockable;
    }

    public override ConsoleColor ForegroundColor
    {
        get { lock (_lock) return _ui.ForegroundColor; }
        set { lock (_lock) _ui.ForegroundColor = value; }
    }

    public override ConsoleColor BackgroundColor
    {
        get { lock (_lock) return _ui.BackgroundColor; }
        set { lock (_lock) _ui.BackgroundColor = value; }
    }

    public override Coordinates CursorPosition
    {
        get { lock (_lock) return _ui.CursorPosition; }
        set { lock (_lock) _ui.CursorPosition = value; }
    }

    public override int CursorSize
    {
        get { lock (_lock) return _ui.CursorSize; }
        set { lock (_lock) _ui.CursorSize = value; }
    }

    public override Coordinates WindowPosition
    {
        get { lock (_lock) return _ui.WindowPosition; }
        set { lock (_lock) _ui.WindowPosition = value; }
    }

    public override Size WindowSize
    {
        get { lock (_lock) return _ui.WindowSize; }
        set { lock (_lock) _ui.WindowSize = value; }
    }

    public override Size MaxWindowSize
    {
        get { lock (_lock) return _ui.MaxWindowSize; }
    }

    public override Size MaxPhysicalWindowSize
    {
        get { lock (_lock) return _ui.MaxPhysicalWindowSize; }
    }

    public override string WindowTitle
    {
        get { lock (_lock) return _ui.WindowTitle; }
        set { lock (_lock) _ui.WindowTitle = value; }
    }

    public override Size BufferSize
    {
        get { lock (_lock) return _ui.BufferSize; }
        set { lock (_lock) _ui.BufferSize = value; }
    }

    public override bool KeyAvailable
    {
        get { lock (_lock) return _ui.KeyAvailable; }
    }

    public override void FlushInputBuffer()
    {
        lock (_lock) _ui.FlushInputBuffer();
    }

    public override BufferCell[,] GetBufferContents(Rectangle rectangle)
    {
        lock (_lock) return _ui.GetBufferContents(rectangle);
    }

    public override KeyInfo ReadKey(ReadKeyOptions options)
    {
        lock (_lock) return _ui.ReadKey(options);
    }

    public override void SetBufferContents(Rectangle rectangle, BufferCell fill)
    {
        lock (_lock) _ui.SetBufferContents(rectangle, fill);
    }

    public override void SetBufferContents(Coordinates origin, BufferCell[,] contents)
    {
        lock (_lock) _ui.SetBufferContents(origin, contents);
    }

    public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill)
    {
        lock (_lock) _ui.ScrollBufferContents(source, destination, clip, fill);
    }

    public override int LengthInBufferCells(char source)
    {
        return _ui.LengthInBufferCells(source);
    }

    public override int LengthInBufferCells(string source)
    {
        return _ui.LengthInBufferCells(source);
    }

    public override int LengthInBufferCells(string source, int offset)
    {
        return _ui.LengthInBufferCells(source, offset);
    }
}
