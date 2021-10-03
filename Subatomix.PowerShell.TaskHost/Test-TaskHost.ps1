#Requires -Version 7
using namespace System.Management.Automation

[CmdletBinding()]
param (
    [Parameter(Mandatory, Position = 0)]
    [scriptblock] $ScriptBlock,

    [Parameter()]
    [string] $Header
)

begin {
    Add-Type -Path (Join-Path $PSScriptRoot Subatomix.PowerShell.TaskHost.dll)
}

process {
    $Factory = [Subatomix.PowerShell.TaskHost.TaskHostFactory]::new()

    $Settings = [PSInvocationSettings]::new()
    $Settings.Host                  = $Factory.Create($Host, "foo")
    $Settings.ErrorActionPreference = [ActionPreference]::Stop

    $Shell = [PowerShell]::Create()
    try {
        $Shell.AddScript($ScriptBlock).Invoke($null, $Settings)
    }
    catch {
        $Shell.Dispose()
    }
}
