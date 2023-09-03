// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

/// <summary>
///   A helper to direct the error stream of a <see cref="Cmdlet"/> to a
///   specific <see cref="PSHost"/>.
/// </summary>
/// <remarks>
///   ⚠ This class uses a PowerShell internal API.  It is possible that some
///   future version of PowerShell changes that API, breaking this class.  In
///   that case, this class takes care to fail gracefully: the error stream
///   retains its default routing and is not redirected to a custom host.
/// </remarks>
internal static class ErrorFlowFixup
{
    /// <summary>
    ///   Directs the error stream of the specified <see cref="Cmdlet"/> to the
    ///   specified <see cref="PSHost"/>, unless the stream is already subject
    ///   to merging or redirection.
    /// </summary>
    /// <param name="cmdlet">
    ///   The cmdlet whose error stream should be directed to
    ///   <paramref name="host"/>.
    /// </param>
    /// <param name="host">
    ///   The host to which to direct the error stream of
    ///   <paramref name="cmdlet"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="cmdlet"/> and/or <paramref name="host"/> is
    ///   <see langword="null"/>.
    /// </exception>
    /// <remarks>
    ///   ⚠ This method uses a PowerShell internal API.  It is possible that
    ///   some future version of PowerShell changes that API, breaking this
    ///   method.  In that case, this class takes care to fail gracefully: the
    ///   <paramref name="cmdlet"/> error stream retains its default routing
    ///   and is not redirected to the <paramref name="host"/>.
    /// </remarks>
    public static void Configure(Cmdlet cmdlet, PSHost host)
    {
        if (cmdlet is null)
            throw new ArgumentNullException(nameof(cmdlet));
        if (host is null)
            throw new ArgumentNullException(nameof(host));

        try
        {
            ConfigureCore(cmdlet, host);
        }
        catch
        {
            // This method (ab)uses PowerShell internals to provide a merely
            // nice-to-have effect.  If those internals throw, the exception
            // probably is neither useful nor actionable.  Just ignore it in
            // the interest of graceful degradation.
        }
    }

    private static void ConfigureCore(Cmdlet cmdlet, PSHost host)
    {
        // The default error pipe forwards to a hidden Out-Default command, but
        // PowerShell tends to construct this command before a custom host is
        // active.  The command caches and uses the previous host, and errors
        // do not flow to the custom host.  To resolve, replace the error pipe
        // with one that forwards to a new Out-Default command instance that is
        // constructed after the custom host becomes active.  The existing pipe
        // must be left unmodified because PowerShell tends to reuse it for
        // more than just the error pipe.

        // Skip when cmdlet does not have a runtime yet
        if (cmdlet.CommandRuntime is not { } runtime)
            return;

        // Skip when 2>&1
        if (!VerifyErrorsNotMerged(runtime))
            return;

        if (GetErrorPipe(runtime) is not { } pipe)
            return;

        // Skip when 2> $null
        if (!VerifyNotRedirectedToNull(pipe))
            return;

        // Skip when 2> file
        if (!VerifyNotRedirectedToFile(pipe))
            return;

        // Create a new pipe that fowards to the custom host
        pipe = CreateNewInstance(pipe)!; // null only if pipe is Nullable<T>
        if (!TryForwardToHost(pipe, host))
            return;

        // Replace the default error pipe with the new pipe
        SetErrorPipe(runtime, pipe);
    }

    private static bool VerifyErrorsNotMerged(object runtime)
    {
        return runtime.GetPropertyValue("ErrorMergeTo")?.InvokeMethod("GetValue") is 0;
    }

    private static object? GetErrorPipe(object runtime)
    {
        return runtime.GetPropertyValue("ErrorOutputPipe");
    }

    private static void SetErrorPipe(object runtime, object pipe)
    {
        runtime.SetPropertyValue("ErrorOutputPipe", pipe);
    }

    private static object? CreateNewInstance(object pipe)
    {
        return pipe.GetType().CreateInstance();
    }

    private static bool VerifyNotRedirectedToNull(object pipe)
    {
        return pipe.IsPropertyValue("NullPipe", v => v is false);
    }

    private static bool VerifyNotRedirectedToFile(object pipe)
    {
        return pipe.IsPropertyValue("PipelineProcessor", v => v is null);
    }

    private static bool TryForwardToHost(object pipe, PSHost host)
    {
        return pipe.SetPropertyValue("ExternalWriter", new HostWriter(host));
    }
}
