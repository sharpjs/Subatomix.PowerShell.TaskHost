// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost.Commands;

[Cmdlet(VerbsLifecycle.Invoke, "Task", ConfirmImpact = ConfirmImpact.Low)]
public class InvokeTaskCommand : Command
{
    // -Name
    [Parameter(Mandatory = true, Position = -1)]
    [AllowNull, AllowEmptyString]
    public string? Name { get; set; }

    protected override void Configure(Invocation invocation)
    {
        invocation.UseTask(this, Name);
    }
}
