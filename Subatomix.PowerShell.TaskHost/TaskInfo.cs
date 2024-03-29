// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace Subatomix.PowerShell.TaskHost;

/// <summary>
///   Information about a task provided to <see cref="TaskHost"/>.
/// </summary>
public sealed class TaskInfo
{
    // The current task
    private static readonly AsyncLocal<TaskInfo?> _current = new();

    // All tasks with retain count > 0, keyed by id
    private static readonly ConcurrentDictionary<long, TaskInfo> _all = new();

    // Counter for task ids
    private static long _counter;

    // Primary instance data
    private readonly TaskInfo? _parent;
    private readonly long      _id;
    private readonly string    _encodedId;
    private readonly object    _lock = new();
    private          string    _name;
    private          long      _retainCount;

    // Caches
    private string? _parentFullName;
    private string? _fullName;
    private string? _formattedName;
    private string? _toString;

    /// <summary>
    ///   Initializes a new <see cref="TaskInfo"/> instance.
    /// </summary>
    /// <param name="name">
    ///   The name of the task, or <see langword="null"/> to generate a default
    ///   name.
    /// </param>
    /// <remarks>
    ///   <para>
    ///     The <see cref="Current"/> task becomes the <see cref="Parent"/>
    ///     of the new instance.
    ///   </para>
    ///   <para>
    ///     The new instance has a <see cref="RetainCount"/> of <c>1</c> and
    ///     is accessible via <see cref="All"/> and <see cref="Get"/> until the
    ///     retain count transitions to <c>0</c>.  Use <see cref="TaskScope"/>,
    ///     <see cref="Retain"/>, and/or <see cref="Release"/> to manage the
    ///     retain count.
    ///   </para>
    /// </remarks>
    public TaskInfo(string? name)
    {
        _parent      = Current;
        _id          = Interlocked.Increment(ref _counter);
        _encodedId   = TaskEncoding.EncodeId(_id);
        _name        = name ?? Invariant($"Task {_id}");
        _retainCount = 1;
        _all[_id]    = this;
    }

    /// <summary>
    ///   Initializes a new <see cref="TaskInfo"/> instance as an unnamed
    ///   virtual task.
    /// </summary>
    /// <remarks>
    ///   <see cref="TaskHostUI"/> uses a task created by this constructor to
    ///   simplify code paths and to store <see cref="IsAtBol"/> when
    ///   <see cref="Current"/> is <see langword="null"/>.
    /// </remarks>
    internal TaskInfo()
    {
        _id        = -1;
        _encodedId = string.Empty;
        _name      = string.Empty;
    }

    /// <summary>
    ///   Gets or sets the current task, or <see langword="null"/> to indicate
    ///   no current task.
    /// </summary>
    /// <remarks>
    ///   This property is local to the current thread or asynchronous flow.
    /// </remarks>
    public static TaskInfo? Current
    {
        get => _current.Value;
        set => _current.Value = value;
    }

    /// <summary>
    ///   Gets a read-only dictionary containing all retained tasks, keyed by
    ///   <see cref="TaskInfo.Id"/>.
    /// </summary>
    public static ReadOnlyDictionary<long, TaskInfo> All { get; } = new(_all);

    /// <summary>
    ///   Gets the parent task, or <see langword="null"/> if the task has no
    ///   parent.
    /// </summary>
    public TaskInfo? Parent => _parent;

    /// <summary>
    ///   Gets a unique identifier for the task.
    /// </summary>
    public long Id => _id;

    /// <summary>
    ///   Gets a unique identifier for the task, encoded for injection into
    ///   strings by <see cref="TaskEncoding"/>.
    /// </summary>
    internal string EncodedId => _encodedId;

    /// <summary>
    ///   Gets or sets the name of the task.  Cannot be <see langword="null"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    ///   Attempted to set the property to <see langword="null"/>.
    /// </exception>
    public string Name
    {
        get { lock (_lock) return GetNameLocked(); }
        set { lock (_lock) SetNameLocked(value); }
    }

    /// <summary>
    ///   Gets the fully-qualified name of the task, consisting of the task's
    ///   <see cref="Name"/> prefixed by the fully-qualified name of the task's
    ///   <see cref="Parent"/>.
    /// </summary>
    public string FullName
    {
        get { lock (_lock) return GetFullNameLocked(); }
    }

    /// <summary>
    ///   Gets the fully-qualified name of the task, formatted as a header to
    ///   appear before lines of output.
    /// </summary>
    internal string FormattedName
    {
        get { lock (_lock) return GetFormattedNameLocked(); }
    }

    /// <summary>
    ///   Gets the retain count of the task.
    /// </summary>
    /// <remarks>
    ///   The initial retain count of a task is <c>1</c>.  The task remains
    ///   accessible via <see cref="All"/> and <see cref="Get"/> until the
    ///   retain count transitions to <c>0</c>.  Use <see cref="TaskScope"/>,
    ///   <see cref="Retain"/>, and/or <see cref="Release"/> to manage the
    ///   retain count.
    /// </remarks>
    public long RetainCount => Volatile.Read(ref _retainCount);

    /// <summary>
    ///   Gets or sets whether the task's output is at the beginning of a line.
    ///   The default value is <see langword="true"/>.
    /// </summary>
    internal bool IsAtBol { get; set; } = true;

    /// <summary>
    ///   Gets the task with the specified unique identifier, or
    ///   <see langword="null"/> if no such task exists.
    /// </summary>
    /// <param name="id">
    ///   The unique identifier of the task to get.
    /// </param>
    /// <returns>
    ///   The task whose <see cref="Id"/> is <paramref name="id"/>, or
    ///   <see langword="null"/> if no such task exists.
    /// </returns>
    public static TaskInfo? Get(long id)
    {
        _all.TryGetValue(id, out var task);
        return task;
    }

    /// <summary>
    ///   Increments the retain count of the task.
    /// </summary>
    /// <remarks>
    ///   The initial retain count of a task is <c>1</c>.  The task remains
    ///   accessible via <see cref="All"/> and <see cref="Get"/> until the
    ///   retain count transitions to <c>0</c>.
    /// </remarks>
    public void Retain()
    {
        Interlocked.Increment(ref _retainCount);
    }

    /// <summary>
    ///   Decrements the retain count of the task.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     The initial retain count of a task is <c>1</c>.  The task remains
    ///     accessible via <see cref="All"/> and <see cref="Get"/> until the
    ///     retain count transitions to <c>0</c>.
    ///   </para>
    ///   <para>
    ///     If the retain count is already <c>0</c>, this method has no effect.
    ///   </para>
    /// </remarks>
    public void Release()
    {
        if (InterlockedEx.DecrementZeroSaturating(ref _retainCount) is 0)
            _all.TryRemove(_id, out _);
    }

    public override string ToString()
    {
        lock (_lock) return ToStringLocked();
    }

    private void SetNameLocked(string value)
    {
        _name          = value ?? throw new ArgumentNullException(nameof(value));
        _fullName      = null;
        _formattedName = null;
    }

    private string GetNameLocked()
    {
        return _name;
    }

    private string GetFullNameLocked()
    {
        if (_parent is not { } parent)
            return GetNameLocked();

        var parentFullName = parent.GetFullNameLocked();

        if (ReferenceEquals(_parentFullName, parentFullName) && _fullName is { } value)
            return value;

        _parentFullName = parentFullName;
        _formattedName  = null;

        return _fullName = Join(parentFullName, GetNameLocked());
    }

    private string GetFormattedNameLocked()
    {
        var fullName = GetFullNameLocked();

        return _formattedName ??= fullName.Length > 0
            ? string.Concat("[", fullName, "]: ")
            : string.Empty;
    }

    private string ToStringLocked()
    {
        var fullName = GetFullNameLocked();

        return _toString ??= fullName.Length > 0
            ? Invariant($"#{_id}: {fullName}")
            : Invariant($"#{_id}");
    }

    private static string Join(string a, string b)
    {
        return a.Length == 0 ? b
            :  b.Length == 0 ? a
            :  string.Concat(a, "|", b);
    }
}
