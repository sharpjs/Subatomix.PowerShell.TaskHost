// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost.Commands;

[Cmdlet(VerbsLifecycle.Invoke, "Task", ConfirmImpact = ConfirmImpact.Low)]
public class InvokeTaskCommand : Command
{
    // -Name
    [Parameter()]
    [ValidateNotNullOrEmpty]
    public string? Name { get; set; }

    // -Host
    [Parameter()]
    [ValidateNotNullOrEmpty]
    public PSHost? OverrideHost { get; set; }

    protected override void ProcessRecord()
    {
        using var scope = TaskScope.Begin(Name);

        base.ProcessRecord();
    }

    protected override void Configure(Invocation invocation)
    {
        invocation.UseTaskInjectingRedirection(this);

        if (OverrideHost is not null)
            invocation.UseHost(OverrideHost);
    }
}
