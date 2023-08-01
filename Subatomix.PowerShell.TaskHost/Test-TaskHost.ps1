#Requires -Version 7
using namespace Subatomix.PowerShell.TaskHost

<#
.SYNOPSIS
    Demonstrates usage of Subatomix.PowerShell.TaskHost.

.DESCRIPTION
    The Test-TaskHost script demonstrates the usage of the Subatomix.PowerShell.TaskHost package.

    The script uses 'ForEach-Object -Parallel' to illustrate workarounds specific to that command.

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
    # Difficulties with the `ForEach-Object -Parallel { ... }` command:
    #
    # - Imported namespaces are not recognized inside the -Parallel script
    #   block.  Use fully-qualified type names instead.
    #
    # - The `using namespace` statement is invalid inside the -Parallel script
    #   block.  Use fully-qualified type names instead.
    #
    # - The `$using:` prefix is required to reference variables defined outside
    #   the -Parallel script block.
    #
    # - The `$using:` prefix does not support variables that themselves contain
    #   script blocks.  Convert such variables to strings before invoking
    #   ForEach-Object, then convert back to script blocks once inside the
    #   -Parallel script block.
    #
    # - Inside the -Parallel script block, the `$Host` constant returns the
    #   ForEach-Object command's own host wrapper, not the TaskHostFactory's
    #   wrapper.  This prevents the task from setting its own header text.  To
    #   restore this capability, assign the task host to a different constant.

    # Create the factory for per-task hosts.
    $Factory = [TaskHostFactory]::new($Host, <#withElapsed:#> $true)

    # HACK: ForEach-Object -Parallel does not support $using:ScriptBlock.
    # Must stringify the script block here, then reconstitute it in each task.
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
        finally {
            $Shell.Dispose()
            $TaskHost.Dispose()
        }
    }
}
