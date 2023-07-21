#Requires -Version 7
using namespace Subatomix.PowerShell.TaskHost

<#
.SYNOPSIS
    Demonstrates usage of Subatomix.PowerShell.TaskHost.

.DESCRIPTION
    The Test-TaskHost script demonstrates the usage of the Subatomix.PowerShell.TaskHost package.

    The script uses 'ForEach-Object -Parallel' internally to illustrate specific difficulties and workarounds for those difficulties.

.NOTES
    Copyright 2023 Subatomix Research Inc.
    SPDX-License-Identifier: ISC
#>
[CmdletBinding()]
param (
    # Script block to execute in each parallel task.
    [Parameter(Position = 0)]
    [scriptblock] $ScriptBlock = { Write-Host "This is example output" }
,
    # Count of parallel tasks to run.
    [Parameter()]
    [int] $Count = 4
,
    # Maximum number of tasks that
    [Parameter()]
    [int] $ThrottleLimit = [Environment]::ProcessorCount
)

process {
    # Create the factory for per-task hosts.
    $Factory = [TaskHostFactory]::new($Host)

    # HACK: ForEach-Object -Parallel { $using:SomeScriptBlock } is unsupported.
    # Must stringify the -ScriptBlock here, then reconstitute it in each task.
    $ScriptText = $ScriptBlock.ToString()

    # Do some parallel tasks
    1..$Count | ForEach-Object -ThrottleLimit $ThrottleLimit -Parallel {
        # Create the host for this task.
        $TaskHost = ($using:Factory).Create()

        # Create invocation settings that will use the host.
        $Settings = [System.Management.Automation.PSInvocationSettings]::new()
        $Settings.Host = $TaskHost

        # HACK: ForEach-Object -Parallel returns its own wrapper as the $Host.
        # Provide access to the task host via a separate $TaskHost constant.
        $State = [InitialSessionState]::CreateDefault2()
        $State.Variables.Add(
            [System.Management.Automation.Runspaces.SessionStateVariableEntry]::new(
                "TaskHost", $TaskHost, $null, "Constant,AllScope"
            )
        )

        # HACK: Reconstitute the -ScriptBlock parameter value from string.
        $ScriptBlock = [ScriptBlock]::Create($using:ScriptText)

        # Finally, invoke the script block.
        $Shell = [PowerShell]::Create($State)
        try {
            $Shell.AddScript($ScriptBlock).Invoke($null, $Settings)
        }
        catch {
            $Shell.Dispose()
        }
    }
}
