#Requires -Version 7
#Requires -Module TaskHost

<#
.SYNOPSIS
    Demonstrates usage of Subatomix.PowerShell.TaskHost.

.DESCRIPTION
    The Test-TaskHost script demonstrates the usage of the TaskHost module.

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
    # - The `$using:` prefix is required to reference variables defined outside
    #   the -Parallel script block.
    #
    # - The `$using:` prefix does not support variables that themselves contain
    #   script blocks.  Convert such variables to strings before invoking
    #   ForEach-Object, then convert back to script blocks once inside the
    #   -Parallel script block.
    #
    # - ForEach-Object -Parallel flows its output streams via a cross-thread
    #   producer-consumer queue that does not preserve execution context and
    #   thus forgets the current task.  Use-TaskHost and Invoke-Task internally
    #   implement a workaround for this.  Invoke-Task injects the current task
    #   id into the output before it is enqueued.  Use-TaskHost extracts that
    #   id and restores the current task.

    # Set up the host
    Use-TaskHost -WithElapsed {
        # TRAP: ForEach-Object -Parallel does not support $using:ScriptBlock.
        # Must stringify the script block here, then reconstitute it in each task.
        $ScriptText = $ScriptBlock.ToString()

        # TRAP: ForEach-Object -Parallel uses a new runspace with no loaded modules
        # Must reimport desired modules in each task.
        $ModulePath = Get-Module TaskHost | ForEach-Object Path | Split-Path | Join-Path -ChildPath TaskHost.psd1

        # Do some parallel tasks
        1..$Count | ForEach-Object -ThrottleLimit $ThrottleLimit -Parallel {
            # Reconstitute the -ScriptBlock parameter value from string.
            $ScriptBlock = [ScriptBlock]::Create($using:ScriptText)

            # Reimport desired modules
            Import-Module $using:ModulePath

            # Run the task, restoring the desired host
            Invoke-Task -Name "Task $_" $ScriptBlock
        }
    }
}
