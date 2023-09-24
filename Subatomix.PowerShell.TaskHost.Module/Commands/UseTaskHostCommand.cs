// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost.Commands;

[Cmdlet(VerbsOther.Use, "TaskHost", ConfirmImpact = ConfirmImpact.Low)]
public class UseTaskHostCommand : Command
{
    // -WithElapsed
    [Parameter()]
    public SwitchParameter WithElapsed { get; set; }

    protected override void Configure(Invocation invocation)
    {
        // Typically, the value of $Host is an instance of InternalPSHost whose
        // ExternalHost property exposes the actual host.  For advanced cases,
        // recognize a $_TaskHostIsActive variable as an alternative way for
        // scripts to indicate the presence of an existing TaskHost.

        var host = Host.GetPropertyValue("ExternalHost") as PSHost ?? Host;
        if (host is TaskHost)
            return;

        if (GetVariableValue("_TaskHostIsActive") is true)
            return;

        if (host.UI is null)
        {
            WriteWarning("A Use-TaskHost command will have no effect because $Host.UI is null.");
            return;
        }

        host = new TaskHost(Host, WithElapsed);

        invocation
            .UseTaskExtractingRedirection(this)
            .UseHost(host);
    }
}
