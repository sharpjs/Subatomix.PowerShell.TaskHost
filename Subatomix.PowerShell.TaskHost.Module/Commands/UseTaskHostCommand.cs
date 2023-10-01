// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost.Commands;

[Cmdlet(VerbsOther.Use, "TaskHost", ConfirmImpact = ConfirmImpact.Low)]
public class UseTaskHostCommand : Command
{
    // -WithElapsed
    [Parameter()]
    public SwitchParameter WithElapsed { get; set; }

    private static int  _depth;
    private        bool _isNested;

    protected override bool ShouldBypass
        // Avoid duplicate hosts
        => _isNested || base.ShouldBypass;

    protected override void ProcessRecord()
    {
        _isNested = Interlocked.Increment(ref _depth) > 1;

        try
        {
            base.ProcessRecord();
        }
        finally
        {
            Interlocked.Decrement(ref _depth);
        }
    }

    protected override void Configure(Invocation invocation)
    {
        invocation.UseTaskExtractingRedirection(this);

        // Typically, the value of $Host is an instance of InternalPSHost whose
        // ExternalHost property exposes the actual host.
        var host = Host.GetPropertyValue("ExternalHost") as PSHost ?? Host;

        if (host.UI is not null)
            invocation.UseHost(new TaskHost(Host, WithElapsed));
        else
            WriteWarning("A Use-TaskHost command will have no effect because $Host.UI is null.");
    }
}
