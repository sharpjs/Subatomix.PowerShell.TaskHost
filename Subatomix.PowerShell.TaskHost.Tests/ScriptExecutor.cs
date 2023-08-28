// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Management.Automation.Remoting.Internal;
using System.Runtime.InteropServices;
using Microsoft.PowerShell;

namespace Subatomix.PowerShell.TaskHost;

internal static class ScriptExecutor
{
    private static string
        TestPath => TestContext.CurrentContext.TestDirectory;

    private static readonly InitialSessionState
        InitialState = CreateInitialSessionState();

    private static InitialSessionState CreateInitialSessionState()
    {
        var state = InitialSessionState.CreateDefault();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            state.ExecutionPolicy = ExecutionPolicy.RemoteSigned;

        state.ImportPSModule(Path.Combine(TestPath, "TaskHost.psd1"));

        return state;
    }

    internal static (IReadOnlyList<PSObject?>, Exception?) Execute(PSHost host, string script)
    {
        if (script is null)
            throw new ArgumentNullException(nameof(script));

        var settings = new PSInvocationSettings
        {
            ErrorActionPreference = ActionPreference.Stop,
            Host                  = host
        };

        var output    = new List<PSObject?>();
        var exception = null as Exception;

        using var shell = Sma.PowerShell.Create(InitialState);

        Redirect(shell.Streams, output);

        shell
            .AddCommand("Set-Location").AddParameter("LiteralPath", TestPath)
            .AddScript(script);

        try
        {
            shell.Invoke(input: null, output, settings);
        }
        catch (Exception e)
        {
            exception = e;
        }

        return (output, exception);
    }

    private static void Redirect(PSDataStreams streams, List<PSObject?> output)
    {
        streams.Warning.DataAdding += (_, data) => StoreWarning (data, output);
        streams.Error  .DataAdding += (_, data) => StoreError   (data, output);
    }

    private static void StoreWarning(DataAddingEventArgs data, List<PSObject?> output)
    {
        var written = (WarningRecord) data.ItemAdded;
        var message = new PSWarning(written.Message);
        output.Add(new PSObject(message));
    }

    private static void StoreError(DataAddingEventArgs data, List<PSObject?> output)
    {
        var written = (ErrorRecord) data.ItemAdded;
        var message = new PSError(written.Exception.Message);
        output.Add(new PSObject(message));
    }
}

internal record PSWarning (string Message);
internal record PSError   (string Message);
