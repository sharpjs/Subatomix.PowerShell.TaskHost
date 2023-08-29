// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost.Commands;

public abstract class Command : PSCmdlet
{
    private ScriptBlock?    _scriptBlock;
    private PSModuleInfo[]? _modules;
    private PSVariable[]?   _variables;

    // -ScriptBlock
    [Parameter(Mandatory = true, Position = 0)]
    [ValidateNotNull]
    public ScriptBlock ScriptBlock
    {
        get => _scriptBlock ??= ScriptBlock.Create("");
        set => _scriptBlock   = value;
    }

    // -Module
    [Parameter()]
    [ValidateNotNull]
    [AllowEmptyCollection]
    public PSModuleInfo[] Module
    {
        get => _modules ??= Array.Empty<PSModuleInfo>();
        set => _modules   = value.Sanitize();
    }

    // -Variable
    [Parameter()]
    [ValidateNotNull]
    [AllowEmptyCollection]
    public PSVariable[] Variable
    {
        get => _variables ??= Array.Empty<PSVariable>();
        set => _variables   = value.Sanitize();
    }

    protected void InvokeCore(string? name = null)
    {
        using var invocation = new Invocation(MyInvocation.MyCommand.Name);

        invocation
            .ImportModules(Module)
            .ImportModule(GetThisModulePath())
            .SetVariables(Variable)
            .AddScript(ScriptBlock);

        Configure(invocation);

        invocation.Invoke();
    }

    protected abstract void Configure(Invocation invocation);

    private string GetThisModulePath()
    {
        return Path.Combine(
            Path.GetDirectoryName(typeof(Command).Assembly.Location)!,
            "TaskHost.psd1"
        );
    }
}
