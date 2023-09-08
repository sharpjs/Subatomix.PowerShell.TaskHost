// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost.Commands;

[Cmdlet(VerbsLifecycle.Invoke, "Task", ConfirmImpact = ConfirmImpact.Low)]
public class InvokeTaskCommand : Command
{
    // -Name
    [Parameter(Mandatory = true, Position = -1)]
    [AllowNull]
    [AllowEmptyString]
    public string? Name { get; set; }

    // -UseHost
    [Parameter()]
    [ValidateNotNullOrEmpty]
    public PSHost? UseHost { get; set; }

    protected override void ProcessRecord()
    {
        using var scope = TaskScope.Begin(Name);

        base.ProcessRecord();
    }

    protected override void Configure(Invocation invocation)
    {
        invocation.UseTaskInjectingRedirection(this);

        if (UseHost is not null)
            invocation.UseHost(UseHost);
    }
}
