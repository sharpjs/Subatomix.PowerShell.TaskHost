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

    private IDisposable? _fixup;

    /// <summary>
    ///   Initializes a new <see cref="Invocation"/> instance.
    /// </summary>
    public Invocation()
    {
        _powershell = Sma.PowerShell.Create(RunspaceMode.CurrentRunspace);
        _settings   = new();
        _output     = new();
    }

    /// <summary>
    ///   Redirects output from the invocation to the specified cmdlet.
    ///   Does not perform any transformation on the output.
    /// </summary>
    /// <param name="cmdlet">
    ///   The cmdlet to which to redirect output from the invocation.
    /// </param>
    /// <returns>
    ///   The invocation instance, for method chaining.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="cmdlet"/> is <see langword="null"/>.
    /// </exception>
    public Invocation UseVerbatimRedirection(PSCmdlet cmdlet)
    {
        new Redirector(_output, _powershell.Streams, cmdlet);

        return this;
    }

    /// <summary>
    ///   Redirects output from the invocation to the specified cmdlet.
    ///   Injects current-task information into the output.
    /// </summary>
    /// <param name="cmdlet">
    ///   The cmdlet to which to redirect output from the invocation.
    /// </param>
    /// <returns>
    ///   The invocation instance, for method chaining.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="cmdlet"/> is <see langword="null"/>.
    /// </exception>
    public Invocation UseTaskInjectingRedirection(PSCmdlet cmdlet)
    {
        new TaskInjectingRedirector(_output, _powershell.Streams, cmdlet);

        _fixup = new TaskInjectingBufferTransformer(cmdlet.Host);

        return this;
    }

    /// <summary>
    ///   Redirects output from the invocation to the specified cmdlet.
    ///   Extracts current-task information from the output.
    /// </summary>
    /// <param name="cmdlet">
    ///   The cmdlet to which to redirect output from the invocation.
    /// </param>
    /// <returns>
    ///   The invocation instance, for method chaining.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="cmdlet"/> is <see langword="null"/>.
    /// </exception>
    public Invocation UseTaskExtractingRedirection(PSCmdlet cmdlet)
    {
        new TaskExtractingRedirector(_output, _powershell.Streams, cmdlet);

        _fixup = new DefaultStreamsFixup(cmdlet);

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
    ///   Sets a custom host to use for the invocation.
    /// </summary>
    /// <param name="host">
    ///   The custom host to use.
    /// </param>
    /// <returns>
    ///   The invocation instance, for method chaining.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="host"/> is <see langword="null"/>.
    /// </exception>
    public Invocation UseHost(PSHost host)
    {
        if (host is null)
            throw new ArgumentNullException(nameof(host));

        _settings.Host = host;
        return this;
    }

    /// <summary>
    ///   Executes the invocation.
    /// </summary>
    public void Invoke()
    {
        _powershell.Invoke(input: None, _output, _settings);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(managed: true);
        GC.SuppressFinalize(this);
    }

    // For testing
    internal void SimulateFinalizer()
    {
        Dispose(managed: false);
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    protected virtual void Dispose(bool managed)
    {
        if (!managed)
            return;

        _fixup?    .Dispose();
        _output    .Dispose();
        _powershell.Dispose();
    }
}
