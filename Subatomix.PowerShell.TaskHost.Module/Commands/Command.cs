// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost.Commands;

public abstract class Command : PSCmdlet
{
    internal const string
        BypassVariableName = "TaskHostBypass";

    private ScriptBlock? _scriptBlock;

    // -ScriptBlock
    [Parameter(Mandatory = true, Position = 0)]
    [ValidateNotNull]
    public ScriptBlock ScriptBlock
    {
        get => _scriptBlock ??= ScriptBlock.Create("");
        set => _scriptBlock   = value;
    }

    protected virtual bool ShouldBypass
        => GetVariableValue(BypassVariableName) is not (null or false);

    protected override void ProcessRecord()
    {
        using var invocation = new Invocation();

        invocation.AddScript(ScriptBlock);

        if (ShouldBypass)
            invocation.UseVerbatimRedirection(this);
        else
            Configure(invocation);

        invocation.Invoke();
    }

    protected abstract void Configure(Invocation invocation);
}
