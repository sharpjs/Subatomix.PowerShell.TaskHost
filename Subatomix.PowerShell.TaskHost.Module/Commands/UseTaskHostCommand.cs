// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost.Commands;

[Cmdlet(VerbsOther.Use, "TaskHost", ConfirmImpact = ConfirmImpact.Low)]
public class UseTaskHostCommand : Command
{
    // -WithElapsed
    [Parameter()]
    public SwitchParameter WithElapsed { get; set; }

    protected override void ProcessRecord()
    {
        InvokeCore("Use-TaskHost");
    }

    protected override void Configure(Invocation invocation)
    {
        var host = new TaskHost(Host, WithElapsed);

        invocation
            .UseTaskExtractingRedirection(this)
            .UseHost(host);

        ErrorFlowFixup.Configure(this, host);
    }
}
