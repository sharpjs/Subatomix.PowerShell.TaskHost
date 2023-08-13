// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Reflection;

namespace Subatomix.PowerShell.TaskHost;

/// <summary>
///   A scope in which the a <see cref="TaskHost"/> becomes the current
///   PowerShell host, improving the clarity of output from long-running,
///   potentially parallel tasks.
/// </summary>
/// <remarks>
///   ⚠ This class uses a PowerShell internal API to activate a custom host.
///   It is possible that some future version of PowerShell changes that API,
///   breaking this class.  In that case, this class takes care to fail
///   gracefully: the custom host does not become active, and output will not
///   be modified for clarity.
/// </remarks>
public sealed class TaskHostScope : IDisposable
{
    private readonly PSHost  _visibleHost;  // InternalHost ($Host)
                                            //   ↓ .ExternalHost
    private readonly PSHost? _wrapperHost;  // TaskHost
                                            //   ↓ ._host
    private readonly PSHost? _actualHost;   // ConsoleHost (usually)

    /// <summary>
    ///   Initializes a new <see cref="TaskHostScope"/> instance, replacing the
    ///   current PowerShell host with a <see cref="TaskHost"/> wrapper until
    ///   the scope is disposed.
    /// </summary>
    /// <param name="host">
    ///   The current PowerShell host (<c>$Host</c>).
    /// </param>
    /// <param name="withElapsed">
    ///   <see langword="true"/> to report elapsed time;
    ///   <see langword="false"/> otherwise.
    /// </param>
    /// <remarks>
    ///   ⚠ This constructor uses a PowerShell internal API to activate a
    ///   custom host. It is possible that some future version of PowerShell
    ///   changes that API, breaking this constructor.  In that case, this
    ///   constructor takes care to fail gracefully: the custom host does not
    ///   become active, and output will not be modified for clarity.
    /// </remarks>
    public TaskHostScope(PSHost host, bool withElapsed = false)
    {
        if (host is null)
            throw new ArgumentNullException(nameof(host));

        _visibleHost = host;

        // Initially, PowerShell's $Host is an instance of its wrapper class
        // InternalHost, whose ExternalHost property references the actual
        // host.  ExternalHost and the methods to override it are, confusingly,
        // internal, so use reflection to invoke them.  Those members might
        // change without notice; take care to fail gracefully if that happens.

        try
        {
            // Attempt to get actual host
            if (ExternalHost is not { } actualHost)
                return;

            // Attempt to replace with wrapper host
            var wrapperHost = new TaskHost(actualHost, withElapsed);
            SetHostRef(wrapperHost);

            // Internal API worked
            _wrapperHost = wrapperHost;
            _actualHost  = actualHost;
        }
        catch
        {
            // Fail gracefully
        }
    }

    /// <summary>
    ///   Restores the <see cref="TaskHost"/> wrapper as the current PowerShell
    ///   host if some other code replaced it and did not clean up after itself.
    /// </summary>
    /// <remarks>
    ///   ⚠ This method uses a PowerShell internal API to activate a custom
    ///   host. It is possible that some future version of PowerShell changes
    ///   that API, breaking this method.  In that case, this method takes care
    ///   to fail gracefully: the custom host does not become active, and
    ///   output will not be modified for clarity.
    /// </remarks>
    public void Restore()
    {
        if (_wrapperHost is not null)
            return;

        try
        {
            if (ReferenceEquals(ExternalHost, _wrapperHost))
                return; // Custom host still set

            // Custom host needs to be set again
            SetHostRef(_wrapperHost!);
        }
        catch
        {
            // Fail gracefully
        }
    }

    /// <summary>
    ///   Disposes the scope, restoring the original current PowerShell host.
    /// </summary>
    /// <remarks>
    ///   ⚠ This method uses a PowerShell internal API to deactivate a custom
    ///   host. It is possible that some future version of PowerShell changes
    ///   that API, breaking this method.  In that case, this method takes care
    ///   to fail gracefully: if the custom host previously become active, it
    ///   will remain active.
    /// </remarks>
    public void Dispose()
    {
        if (_wrapperHost is not null)
            return;

        try
        {
            RevertHostRef();
        }
        catch
        {
            // Fail gracefully
        }
    }

    /// <summary>
    ///   Gets whether the <see cref="TaskHost"/> became the current PowerShell
    ///   host.
    /// </summary>
    /// <remarks>
    ///   This property is <see langword="true"/> unless the constructor of
    ///   this class failed to activate a custom host.
    /// </remarks>
    public bool WasTaskHostActivated => _wrapperHost is not null;

    private PSHost? ExternalHost
        => GetHostPropertyValue<PSHost>(nameof(ExternalHost));

    private void SetHostRef(PSHost host)
        => InvokeHostMethod(nameof(SetHostRef), host);

    private void RevertHostRef()
        => InvokeHostMethod(nameof(RevertHostRef));

    const BindingFlags Flags
        = BindingFlags.Instance
        | BindingFlags.Public
        | BindingFlags.NonPublic;

    private T? GetHostPropertyValue<T>(string name)
        where T : class
    {
        return (T?) _visibleHost
            .GetType()
            .GetProperty(name, Flags)?
            .GetValue(_visibleHost);
    }

    private void InvokeHostMethod(string name)
    {
        _visibleHost
            .GetType()
            .GetMethod(name, Flags, binder: null, Type.EmptyTypes, modifiers: null)?
            .Invoke(_visibleHost, null);
    }

    private void InvokeHostMethod<T0>(string name, T0 arg0)
    {
        _visibleHost
            .GetType()
            .GetMethod(name, Flags, binder: null, new[] { typeof(T0) }, modifiers: null)?
            .Invoke(_visibleHost, new object?[] { arg0 });
    }
}
