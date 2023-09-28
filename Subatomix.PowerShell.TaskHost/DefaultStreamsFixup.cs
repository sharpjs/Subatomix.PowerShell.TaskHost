// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using Microsoft.PowerShell.Commands;

namespace Subatomix.PowerShell.TaskHost;

/// <summary>
///   A helper to correct the routing of the output and error streams of a
///   <see cref="Cmdlet"/> that has activated a custom <see cref="PSHost"/>.
/// </summary>
/// <remarks>
///   ⚠ This class uses a PowerShell internal API.  It is possible that some
///   future version of PowerShell changes that API, breaking this class.  In
///   that case, this class takes care to fail gracefully: the output and error
///   streams retain default routing, which might bypass the current host.
/// </remarks>
internal sealed class DefaultStreamsFixup : IDisposable
{
    private OutDefaultWriter? _writer;

    /// <summary>
    ///   Initializes a new <see cref="DefaultStreamsFixup"/> instance which
    ///   overrides the default routing of the output and error streams of the
    ///   specified <see cref="Cmdlet"/>, directing each stream to the current
    ///   <see cref="PSHost"/> unless that stream is already subject to
    ///   merging, redirection, or piping.
    /// </summary>
    /// <param name="cmdlet">
    ///   The cmdlet whose output and error streams should be directed to the
    ///   current host.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="cmdlet"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    ///   ⚠ This method uses a PowerShell internal API.  It is possible that
    ///   some future version of PowerShell changes that API, breaking this
    ///   method.  In that case, this class takes care to fail gracefully: the
    ///   <paramref name="cmdlet"/> output and error streams retain default
    ///   routing, which might bypass the current host.
    /// </remarks>
    public DefaultStreamsFixup(Cmdlet cmdlet)
    {
        if (cmdlet is null)
            throw new ArgumentNullException(nameof(cmdlet));

        // Skip when cmdlet does not have a runtime yet
        if (cmdlet.CommandRuntime is not { } runtime)
            return;

        // By default, the output and error streams forward to a hidden
        // Out-Default command.  PowerShell tends to construct this command
        // before a custom host becomes current.  The hidden command caches and
        // uses the previous host, and thus output objects and errors do not
        // flow to the custom host.  To resolve, replace the output and error
        // pipes with ones that forward to a new Out-Default command instance
        // that is constructed *after* the custom host becomes current.  Do not
        // modify the existing pipes, as PowerShell tends to reuse them.
        //
        // This method (ab)uses PowerShell internals to provide a merely
        // nice-to-have effect.  If those internals throw, the exception
        // probably is neither useful nor actionable.  Just ignore it in the
        // interest of graceful degradation.

        var pipe = null as object;
        try { ConfigureOutput(runtime, ref pipe); } catch { /* fail gracefully */ }
        try { ConfigureErrors(runtime, ref pipe); } catch { /* fail gracefully */ }

        // Experiment result: Setting the other pipes (ex: WarningOutputPipe)
        // does not persist to a child PowerShell instance.  A command executed
        // by the child shell has a new CommandRuntime whose WarningOutputPipe
        // is null.
    }

    private void ConfigureOutput(ICommandRuntime runtime, ref object? pipe)
    {
        Configure(runtime, "OutputPipe", ref pipe);
    }

    private void ConfigureErrors(ICommandRuntime runtime, ref object? pipe)
    {
        // Skip when 2>&1
        if (!VerifyErrorsNotMerged(runtime))
            return;

        Configure(runtime, "ErrorOutputPipe", ref pipe);
    }

    private void Configure(ICommandRuntime runtime, string name, ref object? newPipe)
    {
        // Get existing pipe
        if (runtime.GetPropertyValue(name) is not { } oldPipe)
            return;

        // Skip when: > $null
        if (!VerifyNotRedirectedToNull(oldPipe))
            return;

        // Skip when: > file
        if (!VerifyNotRedirectedToFile(oldPipe))
            return;

        // Skip when: | command
        if (!VerifyNotPiped(oldPipe))
            return;

        // Get replacement pipe
        if (!TryGetOrCreateForwardingPipe(oldPipe, ref newPipe))
            return;

        // Replace the default pipe
        runtime.SetPropertyValue(name, newPipe);
    }

    private static bool VerifyErrorsNotMerged(object runtime)
    {
        return runtime.GetPropertyValue("ErrorMergeTo")?.InvokeMethod("GetValue") is 0;
    }

    private static bool VerifyNotRedirectedToNull(object pipe)
    {
        return pipe.IsPropertyValue("NullPipe", v => v is false);
    }

    private static bool VerifyNotRedirectedToFile(object pipe)
    {
        return pipe.IsPropertyValue("PipelineProcessor", v => v is null);
    }

    private static bool VerifyNotPiped(object oldPipe)
    {
        return oldPipe.GetPropertyValue("DownstreamCmdlet") is { } processor
            && processor.IsPropertyValue("Command", x => x is OutDefaultCommand);
    }

    private bool TryGetOrCreateForwardingPipe(
                                object  oldPipe,
        [NotNullWhen(true)] ref object? newPipe)
    {
        // Check if already created new pipe
        if (newPipe is not null)
            return true;

        // Create new pipe
        newPipe = oldPipe.GetType().CreateInstance()!; // null only if oldPipe is Nullable<T>

        // Create forwarding writer
        _writer = new OutDefaultWriter();

        // Try to configure new pipe with forwarding writer
        return newPipe.SetPropertyValue("ExternalWriter", _writer);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _writer?.Dispose();
        _writer = null;
    }
}
