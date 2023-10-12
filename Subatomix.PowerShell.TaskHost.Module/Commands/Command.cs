// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost.Commands;

public abstract class Command : PSCmdlet
{
    private ScriptBlock? _scriptBlock;

    // -ScriptBlock
    [Parameter(Mandatory = true, Position = 0)]
    [ValidateNotNull]
    public ScriptBlock ScriptBlock
    {
        get => _scriptBlock ??= ScriptBlock.Create("");
        set => _scriptBlock   = value;
    }

    protected override void ProcessRecord()
    {
        using var invocation = new Invocation();

        invocation.AddScript(ScriptBlock);

        Configure(invocation);

        invocation.Invoke();
    }

    protected abstract void Configure(Invocation invocation);
}
