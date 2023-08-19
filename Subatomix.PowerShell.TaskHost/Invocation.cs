// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

public class Invocation : IDisposable
{
    private const PSDataCollection<PSObject?>? None = null;

    private readonly Runspace                    _runspace;
    private readonly Sma.PowerShell              _powershell;
    private readonly PSDataCollection<PSObject?> _output;

    public Invocation(string runspaceName)
    {
        if (runspaceName is null)
            throw new ArgumentNullException(nameof(runspaceName));

        _runspace               = RunspaceFactory.CreateRunspace();
        _runspace.Name          = runspaceName;
        _runspace.ThreadOptions = PSThreadOptions.UseCurrentThread;

        _powershell = Sma.PowerShell.Create(_runspace);

        _output = new();
    }

    public Invocation ImportModules(PSModuleInfo[] modules)
    {
        if (modules is null)
            throw new ArgumentNullException(nameof(modules));

        foreach (var module in modules)
            ImportModule(module);

        return this;
    }

    public Invocation ImportModule(PSModuleInfo module)
    {
        if (module is null)
            throw new ArgumentNullException(nameof(module));

        _runspace.InitialSessionState.ImportPSModule(module.Path);

        return this;
    }

    public Invocation ImportModule(string name)
    {
        if (name is null)
            throw new ArgumentNullException(nameof(name));

        _runspace.InitialSessionState.ImportPSModule(name);

        return this;
    }

    public Invocation SetVariables(PSVariable[] variables)
    {
        if (variables is null)
            throw new ArgumentNullException(nameof(variables));

        foreach (var variable in variables)
            SetVariable(variable);

        return this;
    }

    public Invocation SetVariable(PSVariable variable)
    {
        if (variable is null)
            throw new ArgumentNullException(nameof(variable));

        _runspace.InitialSessionState.Variables.Add(new SessionStateVariableEntry(
            variable.Name,
            variable.Value,
            variable.Description,
            variable.Options,
            variable.Attributes
        ));

        return this;
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
        _powershell.AddScript(script.ToString());

        return this;
    }

    public void Invoke()
    {
        var settings = new PSInvocationSettings
        {
            ErrorActionPreference = ActionPreference.Stop
        };

        _runspace.Open();

        _powershell.Invoke<PSObject, PSObject>(input: None!, _output!, settings);
    }

    public void Dispose()
    {
        Dispose(managed: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool managed)
    {
        if (!managed)
            return;

        _output    .Dispose();
        _powershell.Dispose();
        _runspace  .Dispose();
    }
}
