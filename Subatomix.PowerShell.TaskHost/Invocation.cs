// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

/// <summary>
///   Utility to invoke PowerShell commands with TaskHost-related extensions
///   and fixups.
/// </summary>
public class Invocation : IDisposable
{
    private const PSDataCollection<PSObject?>? None = null;

    private readonly Sma.PowerShell              _powershell;
    private readonly PSInvocationSettings        _settings;
    private readonly PSDataCollection<PSObject?> _output;
    private readonly Stack<IDisposable>          _disposables;

    private bool _isRedirectionConfigured;

    /// <summary>
    ///   Initializes a new <see cref="Invocation"/> instance.
    /// </summary>
    public Invocation()
    {
        _powershell  = Sma.PowerShell.Create(RunspaceMode.CurrentRunspace);
        _settings    = new();
        _output      = new();
        _disposables = new();
    }

    /// <summary>
    ///   Configures the invocation to use a <see cref="TaskHost"/> if
    ///   possible and to redirect output to the specified cmdlet.
    /// </summary>
    /// <param name="cmdlet">
    ///   The cmdlet to which to redirect output from the invocation.
    /// </param>
    /// <param name="withElapsed">
    ///   <see langword="true"/> to report elapsed time;
    ///   <see langword="false"/> otherwise.
    /// </param>
    /// <returns>
    ///   The invocation instance, for method chaining.
    /// </returns>
    /// <remarks>
    ///   <para>
    ///     This method attempts to redirect the following streams from the
    ///     invocation to the <paramref name="cmdlet"/>: Output (success),
    ///     Error, Warning, Information, Verbose, Debug, and Progress.
    ///   </para>
    ///   <para>
    ///     ⚠ This method uses a PowerShell internal API.  It is possible that
    ///     some future version of PowerShell changes that API, breaking this
    ///     method.  In that case, this method takes care to fail gracefully,
    ///     but some output stream redirection might not occur.
    ///   </para>
    ///   <para>
    ///     If a PowerShell variable <c>$BypassTaskHost</c> exists in the scope
    ///     of the <paramref name="cmdlet"/> and has a value other than
    ///     <c>$null</c> or <c>$false</c>, this method has minimal effect: it
    ///     does not override the host for the invocation and avoids using any
    ///     internal PowerShell APIs.
    ///   </para>
    ///     If <see cref="TaskHost.Current"/> is not <see langword="null"/>,
    ///     this method reuses host and does not create a new
    ///     see cref="TaskHost"/> instance.
    ///   <para>
    ///   </para>
    ///   <para>
    ///     <see cref="TaskHost"/> requires the underlying host of the
    ///     <paramref name="cmdlet"/> to provide a
    ///     <see cref="PSHostUserInterface"/> implementation.  Some hosts do
    ///     not, such as the host used inside <c>ForEach-Object -Parallel</c>.
    ///     If this method detects such a host, this method avoids avoids
    ///     wrapping it with a <see cref="TaskHost"/> (which would throw an
    ///     exception) and instead causes the <paramref name="cmdlet"/> to
    ///     write a warning message.
    ///   </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="cmdlet"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///   The <see cref="UseTaskHost"/> or <see cref="UseTask"/> has been
    ///   invoked already.
    /// </exception>
    public Invocation UseTaskHost(PSCmdlet cmdlet, bool withElapsed = true)
    {
        if (IsBypassed(cmdlet))
            return this;

        UseTaskExtractingRedirection(cmdlet);

        if (TryGetOrCreateTaskHost(cmdlet, withElapsed, out var host))
            UseHost(host);

        return SetConfigured();
    }
    /// <summary>
    ///   Configures the invocation to run as a task (<see cref="TaskInfo"/>)
    ///   and to redirect output to the specified cmdlet.
    /// </summary>
    /// <param name="cmdlet">
    ///   The cmdlet to which to redirect output from the invocation.
    /// </param>
    /// <param name="name">
    ///   The name of the task, or <see langword="null"/> to generate a default
    ///   name.
    /// </param>
    /// <returns>
    ///   The invocation instance, for method chaining.
    /// </returns>
    /// <remarks>
    ///   <para>
    ///     This method attempts to redirect the following streams from the
    ///     invocation to the <paramref name="cmdlet"/>: Output (success),
    ///     Error, Warning, Information, Verbose, Debug, and Progress.
    ///   </para>
    ///   <para>
    ///     ⚠ This method uses a PowerShell internal API.  It is possible that
    ///     some future version of PowerShell changes that API, breaking this
    ///     method.  In that case, this method takes care to fail gracefully,
    ///     but some output stream redirection might not occur.
    ///   </para>
    ///   <para>
    ///     If a PowerShell variable <c>$BypassTaskHost</c> exists in the scope
    ///     of the <paramref name="cmdlet"/> and has a value other than
    ///     <c>$null</c> or <c>$false</c>, this method has minimal effect: it
    ///     does create a <see cref="TaskInfo"/> and avoids using any internal
    ///     PowerShell APIs.
    ///   </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="cmdlet"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///   The <see cref="UseTaskHost"/> or <see cref="UseTask"/> has been
    ///   invoked already.
    /// </exception>
    public Invocation UseTask(PSCmdlet cmdlet, string? name = null)
    {
        if (IsBypassed(cmdlet))
            return this;

        BeginTask(name);

        UseTaskInjectingRedirection(cmdlet);

        return SetConfigured();
    }

    private bool TryGetOrCreateTaskHost(
        PSCmdlet cmdlet, bool withElapsed, [MaybeNullWhen(false)] out TaskHost taskHost)
    {
        // Use existing TaskHost if available
        taskHost = TaskHost.Current;
        if (taskHost is not null)
            return true;

        // Typically, the value of $Host is an instance of InternalPSHost whose
        // ExternalHost property exposes the actual host.
        var actualHost = cmdlet.Host.Unwrap();

        // Try to create new TaskHost
        if (actualHost.UI is not null)
        {
            taskHost = new(actualHost, withElapsed);
            DeferDisposal(TaskHost.GetOrSetCurrent(ref taskHost));
            return true;
        }

        // Fail
        cmdlet.WriteWarning("TaskHost is bypassed because $Host.UI is null.");
        return false;
    }

    private void BeginTask(string? name)
    {
        DeferDisposal(TaskScope.Begin(name));
    }

    private bool IsBypassed(PSCmdlet cmdlet)
    {
        if (cmdlet is null)
            throw new ArgumentNullException(nameof(cmdlet));

        if (_isRedirectionConfigured)
            throw new InvalidOperationException(
                "The invocation has been configured with a task host, a task, " +
                "or bypass redirection already."
            );

        var isBypassed = cmdlet.ShouldBypass();
        if (isBypassed)
            UseBypassRedirection(cmdlet);

        return isBypassed;
    }

    private void UseBypassRedirection(PSCmdlet cmdlet)
    {
        new Redirector(_output, _powershell.Streams, cmdlet);
    }

    private void UseTaskInjectingRedirection(PSCmdlet cmdlet)
    {
        new TaskInjectingRedirector(_output, _powershell.Streams, cmdlet);

        DeferDisposal(new TaskInjectingBufferTransformer(cmdlet.Host));
    }

    private void UseTaskExtractingRedirection(PSCmdlet cmdlet)
    {
        new TaskExtractingRedirector(_output, _powershell.Streams, cmdlet);

        DeferDisposal(new DefaultStreamsFixup(cmdlet));
    }

    private void UseHost(PSHost host)
    {
        _settings.Host = host;
    }

    private Invocation SetConfigured()
    {
        _isRedirectionConfigured = true;
        return this;
    }

    /// <summary>
    ///   Adds a script block to be invoked.
    /// </summary>
    /// <param name="script">
    ///   The script block to invoke.
    /// </param>
    /// <returns>
    ///   The invocation instance, for method chaining.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="script"/> is <see langword="null"/>.
    /// </exception>
    public Invocation AddScript(ScriptBlock script)
    {
        if (script is null)
            throw new ArgumentNullException(nameof(script));

        _powershell.AddScript(script.ToString());

        return this;
    }

    /// <summary>
    ///   Adds a reinvocation of the specified invocation.
    /// </summary>
    /// <param name="invocation">
    ///   The invocation to be reinvoked.
    /// </param>
    /// <returns>
    ///   The invocation instance, for method chaining.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="invocation"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    ///   ⚠ To prevent a stack overflow, the reinvoked command must be able to
    ///   detect when it is reinvoked and avoid reinvoking itself infinitely.
    /// </remarks>
    public Invocation AddReinvocation(InvocationInfo invocation)
    {
        if (invocation is null)
            throw new ArgumentNullException(nameof(invocation));

        _powershell
            .AddStatement()
            .AddCommand(invocation.MyCommand)
            .AddParameters(invocation.BoundParameters);

        foreach (var arg in invocation.UnboundArguments)
            _powershell.AddArgument(arg);

        return this;
    }

    /// <summary>
    ///   Executes the invocation.
    /// </summary>
    public void Invoke()
    {
        _powershell.Invoke(input: None, _output, _settings);
    }

#if ASYNC_SUPPORT
    // Waiting on PowerShell support.  Currently, InvokeAsnc throws:
    // 
    //   PSInvalidOperationException: Nested PowerShell instances cannot be
    //   invoked asynchronously. Use the Invoke method.

    /// <summary>
    ///   Executes the invocation asynchronously.
    /// </summary>
    /// <param name="cancellation">
    ///   The token to monitor for cancellation requests.
    ///   <para>
    ///     ⚠ Currently, this parameter is ignored because the underlying
    ///     <see cref="Sma.PowerShell"/> API does not accept a cancellation
    ///     token.
    ///   </para>
    /// </param>
    /// <returns>
    ///   A <see cref="Task"/> representing the asynchronous operation.
    /// </returns>
    public Task InvokeAsync(CancellationToken cancellation = default)
    {
        return _powershell.InvokeAsync(input: None, _output, _settings, callback: null, state: null);
    }
#endif

    /// <summary>
    ///   Adds the specifiedc object to the deferred disposal collection.
    /// </summary>
    /// <param name="obj">
    ///   The object to be disposed.
    /// </param>
    protected void DeferDisposal(IDisposable obj)
    {
        _disposables.Push(obj);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(managed: true);
        GC.SuppressFinalize(this);
    }

    [ExcludeFromCodeCoverage]
    ~Invocation()
    {
        Dispose(managed: false);
    }
    internal void SimulateFinalizer()
    {
        Dispose(managed: false);
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    protected virtual void Dispose(bool managed)
    {
        if (!managed)
            return;

        while (_disposables.TryPop(out var obj))
            obj.Dispose();

        _output    .Dispose();
        _powershell.Dispose();
    }
}
