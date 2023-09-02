// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

public class Invocation : IDisposable
{
    private const PSDataCollection<PSObject?>? None = null;

    private readonly Sma.PowerShell              _powershell;
    private readonly PSInvocationSettings        _settings;
    private readonly PSDataCollection<PSObject?> _output;

    public Invocation()
    {
        _powershell = Sma.PowerShell.Create(RunspaceMode.CurrentRunspace);
        _settings   = new();
        _output     = new();
    }

    public Invocation UseTaskInjectingRedirection(PSCmdlet cmdlet)
    {
        new TaskInjectingRedirector(_output, _powershell.Streams, cmdlet);

        return this;
    }

    public Invocation UseTaskExtractingRedirection(PSCmdlet cmdlet)
    {
        new TaskExtractingRedirector(_output, _powershell.Streams, cmdlet);

        return this;
    }

    public Invocation AddScript(ScriptBlock script)
    {
        if (script is null)
            throw new ArgumentNullException(nameof(script));

        _powershell.AddScript(script.ToString());

        return this;
    }

    public Invocation UseHost(PSHost host)
    {
        if (host is null)
            throw new ArgumentNullException(nameof(host));

        _settings.Host = host;
        return this;
    }

    public void Invoke()
    {
        _powershell.Invoke(input: None, _output, _settings);
    }

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

    protected virtual void Dispose(bool managed)
    {
        if (!managed)
            return;

        _output    .Dispose();
        _powershell.Dispose();
    }
}
