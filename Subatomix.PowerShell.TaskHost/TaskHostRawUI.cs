// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

/// <summary>
///   A wrapper that synchronizes access to a <see cref="PSHostRawUserInterface"/>
///   using exclusive locks.
/// </summary>
internal sealed class TaskHostRawUI : PSHostRawUserInterface
{
    private readonly PSHostRawUserInterface _ui;    // Underlying implementation
    private readonly object                 _lock;  // Synchronization object

    /// <summary>
    ///   Initializes a new <see cref="TaskHostRawUI"/> instance that
    ///   synchronizes access to the specified raw UI by exclusively locking
    ///   the specified object.
    /// </summary>
    /// <param name="ui">
    ///   The raw UI.
    /// </param>
    /// <param name="lockable">
    ///   The object to lock exclusively to synchronize access to
    ///   <paramref name="ui"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="ui"/> and/or
    ///   <paramref name="lockable"/>
    ///   is <see langword="null"/>.
    /// </exception>
    public TaskHostRawUI(PSHostRawUserInterface ui, object lockable)
    {
        if (ui is null)
            throw new ArgumentNullException(nameof(ui));
        if (lockable is null)
            throw new ArgumentNullException(nameof(lockable));

        _ui   = ui;
        _lock = lockable;
    }

    /// <inheritdoc/>
    public override ConsoleColor ForegroundColor
    {
        get { lock (_lock) return _ui.ForegroundColor; }
        set { lock (_lock) _ui.ForegroundColor = value; }
    }

    /// <inheritdoc/>
    public override ConsoleColor BackgroundColor
    {
        get { lock (_lock) return _ui.BackgroundColor; }
        set { lock (_lock) _ui.BackgroundColor = value; }
    }

    /// <inheritdoc/>
    public override Coordinates CursorPosition
    {
        get { lock (_lock) return _ui.CursorPosition; }
        set { lock (_lock) _ui.CursorPosition = value; }
    }

    /// <inheritdoc/>
    public override int CursorSize
    {
        get { lock (_lock) return _ui.CursorSize; }
        set { lock (_lock) _ui.CursorSize = value; }
    }

    /// <inheritdoc/>
    public override Coordinates WindowPosition
    {
        get { lock (_lock) return _ui.WindowPosition; }
        set { lock (_lock) _ui.WindowPosition = value; }
    }

    /// <inheritdoc/>
    public override Size WindowSize
    {
        get { lock (_lock) return _ui.WindowSize; }
        set { lock (_lock) _ui.WindowSize = value; }
    }

    /// <inheritdoc/>
    public override Size MaxWindowSize
    {
        get { lock (_lock) return _ui.MaxWindowSize; }
    }

    /// <inheritdoc/>
    public override Size MaxPhysicalWindowSize
    {
        get { lock (_lock) return _ui.MaxPhysicalWindowSize; }
    }

    /// <inheritdoc/>
    public override string WindowTitle
    {
        get { lock (_lock) return _ui.WindowTitle; }
        set { lock (_lock) _ui.WindowTitle = value; }
    }

    /// <inheritdoc/>
    public override Size BufferSize
    {
        get { lock (_lock) return _ui.BufferSize; }
        set { lock (_lock) _ui.BufferSize = value; }
    }

    /// <inheritdoc/>
    public override bool KeyAvailable
    {
        get { lock (_lock) return _ui.KeyAvailable; }
    }

    /// <inheritdoc/>
    public override void FlushInputBuffer()
    {
        lock (_lock) _ui.FlushInputBuffer();
    }

    /// <inheritdoc/>
    public override BufferCell[,] GetBufferContents(Rectangle rectangle)
    {
        lock (_lock) return _ui.GetBufferContents(rectangle);
    }

    /// <inheritdoc/>
    public override KeyInfo ReadKey(ReadKeyOptions options)
    {
        lock (_lock) return _ui.ReadKey(options);
    }

    /// <inheritdoc/>
    public override void SetBufferContents(Rectangle rectangle, BufferCell fill)
    {
        lock (_lock) _ui.SetBufferContents(rectangle, fill);
    }

    /// <inheritdoc/>
    public override void SetBufferContents(Coordinates origin, BufferCell[,] contents)
    {
        lock (_lock) _ui.SetBufferContents(origin, contents);
    }

    /// <inheritdoc/>
    public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill)
    {
        lock (_lock) _ui.ScrollBufferContents(source, destination, clip, fill);
    }

    /// <inheritdoc/>
    public override int LengthInBufferCells(char source)
    {
        return _ui.LengthInBufferCells(source);
    }

    /// <inheritdoc/>
    public override int LengthInBufferCells(string source)
    {
        return _ui.LengthInBufferCells(source);
    }

    /// <inheritdoc/>
    public override int LengthInBufferCells(string source, int offset)
    {
        return _ui.LengthInBufferCells(source, offset);
    }
}
