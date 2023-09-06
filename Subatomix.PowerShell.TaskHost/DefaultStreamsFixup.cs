// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Diagnostics.CodeAnalysis;

namespace Subatomix.PowerShell.TaskHost;

/// <summary>
///   A helper to direct the output and error streams of a <see cref="Cmdlet"/>
///   to the current <see cref="PSHost"/>.
/// </summary>
/// <remarks>
///   ⚠ This class uses a PowerShell internal API.  It is possible that some
///   future version of PowerShell changes that API, breaking this class.  In
///   that case, this class takes care to fail gracefully: the output and error
///   streams retains their default routing and are not redirected to the
///   current host.
/// </remarks>
internal static class DefaultStreamsFixup
{
    /// <summary>
    ///   Directs each of the output and error streams of the specified
    ///   <see cref="Cmdlet"/> to the current <see cref="PSHost"/>, unless that
    ///   stream is already subject to merging or redirection.
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
    ///   <paramref name="cmdlet"/> output and error streams retains their
    ///   default routing and are not redirected to the current host.
    /// </remarks>
    public static void Configure(Cmdlet cmdlet)
    {
        if (cmdlet is null)
            throw new ArgumentNullException(nameof(cmdlet));

        // Skip when cmdlet does not have a runtime yet
        if (cmdlet.CommandRuntime is not { } runtime)
            return;

        try
        {
            // By default, the output and error streams forward to a hidden
            // Out-Default command.  PowerShell tends to construct this command
            // before a custom host becomes current.  The hidden command caches
            // and uses the previous host, and errors do not flow to the custom
            // host.  To resolve, replace the error pipe with one that forwards
            // to a new Out-Default command instance that is constructed after
            // the custom host becomes current.  The existing pipe must be left
            // unmodified because PowerShell tends to reuse it.

            var pipe = null as object;
            ConfigureOutput(runtime, ref pipe);
            ConfigureErrors(runtime, ref pipe);
        }
        catch
        {
            // This method (ab)uses PowerShell internals to provide a merely
            // nice-to-have effect.  If those internals throw, the exception
            // probably is neither useful nor actionable.  Just ignore it in
            // the interest of graceful degradation.
        }
    }

    private static void ConfigureOutput(ICommandRuntime runtime, ref object? pipe)
    {
        Configure(runtime, "OutputPipe", ref pipe);
    }

    private static void ConfigureErrors(ICommandRuntime runtime, ref object? pipe)
    {
        // Skip when 2>&1
        if (!VerifyErrorsNotMerged(runtime))
            return;

        Configure(runtime, "ErrorOutputPipe", ref pipe);
    }

    private static void Configure(ICommandRuntime runtime, string name, ref object? newPipe)
    {
        // Get existing pipe
        if (runtime.GetPropertyValue(name) is not { } oldPipe)
            return;

        // Skip when n> $null
        if (!VerifyNotRedirectedToNull(oldPipe))
            return;

        // Skip when n> file
        if (!VerifyNotRedirectedToFile(oldPipe))
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

    private static bool TryGetOrCreateForwardingPipe(
                                object  oldPipe,
        [NotNullWhen(true)] ref object? newPipe)
    {
        if (newPipe is not null)
            return true;

        newPipe = oldPipe.GetType().CreateInstance()!; // null only if oldPipe is Nullable<T>

        var writer = new OutDefaultWriter();

        try
        {
            // TODO: Ensure OutDefaultWriter is exposed when no longer needed
            if (newPipe.SetPropertyValue("ExternalWriter", writer))
                return true;

            writer.Dispose();
            return false;
        }
        catch
        {
            writer.Dispose();
            throw;
        }
    }
}
