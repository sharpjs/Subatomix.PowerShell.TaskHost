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
    private          string?   _name;
    private          long      _retainCount;

    // Caches
    private string? _parentFullName;
    private string? _fullName;
    private string? _formattedName;

    private TaskInfo(string? name)
    {
        _id     = Interlocked.Increment(ref _counter);
        _name   = name ?? Invariant($"Task {_id}");
        _parent = Current;
        IsAtBol = true;

        _all[_id] = this;
    }

    /// <summary>
    ///   Gets the current task.
    /// </summary>
    /// <remarks>
    ///   This property is local to the current thread or asynchronous flow.
    /// </remarks>
    public static TaskInfo? Current
    {
        get          => _current.Value;
        internal set => _current.Value = value;
    }

    /// <summary>
    ///   Gets a read-only dictionary containing all retained tasks, keyed by
    ///   <see cref="TaskInfo.Id"/>.
    /// </summary>
    public static ReadOnlyDictionary<long, TaskInfo> All { get; } = new(_all);

    /// <summary>
    ///   Gets the parent task, or <see langword="null"/> if the current task
    ///   has no parent.
    /// </summary>
    public TaskInfo? Parent => _parent;

    /// <summary>
    ///   Gets a unique identifier for the task.
    /// </summary>
    public long Id => _id;

    /// <summary>
    ///   Gets or sets the name of the task.
    /// </summary>
    public string Name
    {
        get => _name ?? string.Empty;
        set
        {
            _name          = value ?? throw new ArgumentNullException(nameof(value));
            _fullName      = null;
            _formattedName = null;
        }
    }

    /// <summary>
    ///   Gets the fully-qualified name of the task, consisting of the task's
    ///   <see cref="Name"/> prefixed by the fully-qualified name of the task's
    ///   <see cref="Parent"/> parent.
    /// </summary>
    public string FullName
    {
        get
        {
            if (_parent is not { } parent)
                return Name;

            var parentFullName = parent.FullName;

            if (ReferenceEquals(_parentFullName, parentFullName) && _fullName is { } value)
                return value;

            _parentFullName = parentFullName;
            _formattedName = null;

            return _fullName = parentFullName.Length > 0
                ? string.Concat(parentFullName, "|", Name)
                : Name;
        }
    }

    /// <summary>
    ///   Gets the fully-qualified name of the task, formatted as a header to
    ///   appear before lines of output.
    /// </summary>
    internal string FormattedName
    {
        get
        {
            var fullName = FullName; // Do before next line; has caching side effects

            return _formattedName ??= string.Concat("[", fullName, "]: ");
        }
    }

    /// <summary>
    ///   Gets or sets whether the task's output is at the beginning of a line.
    /// </summary>
    internal bool IsAtBol { get; set; }

    /// <summary>
    ///   Begins a new task.
    /// </summary>
    /// <param name="name">
    ///   The name of the task, or <see langword="null"/> to generate a default
    ///   name.
    /// </param>
    /// <returns>
    ///   A disposable scope referencing the new task.  When all such scopes
    ///   are disposed, the task becomes inaccessible via <see cref="All"/> and
    ///   <see cref="Get"/>.
    /// </returns>
    /// <remarks>
    ///   The new task becomes the <see cref="Current"/> instance and has a
    ///   retain count of <c>1</c>.
    /// </remarks>
    public static TaskScope Begin(string? name = null)
    {
        return new(new(name));
    }

    /// <summary>
    ///   Increments the task's retain count.
    /// </summary>
    /// <remarks>
    ///   While the retain count is positive, the task is accessible via
    ///   <see cref="All"/> and <see cref="Get"/>.
    /// </remarks>
    internal void Retain()
    {
        Interlocked.Increment(ref _retainCount);
    }

    /// <summary>
    ///   Decrements the task's retain count.
    /// </summary>
    /// <remarks>
    ///   When the retain count reaches zero, the task becomes inaccessible via
    ///   <see cref="All"/> and <see cref="Get"/>.
    /// </remarks>
    internal void Release()
    {
        if (Interlocked.Decrement(ref _retainCount) <= 0)
            _all.TryRemove(_id, out _);
    }

    /// <summary>
    ///   Gets the task with the specified unique identifier, or
    ///   <see langword="null"/> if no such task exists.
    /// </summary>
    /// <param name="id">
    ///   The unique identifier of the task to get.
    /// </param>
    /// <returns>
    ///   The task whose <see cref="Id"/> is <paramref name="id"/>, or
    ///   <see langword="null"/> if no such task scope exists.
    /// </returns>
    public static TaskInfo? Get(long id)
    {
        _all.TryGetValue(id, out var result);
        return result;
    }
}