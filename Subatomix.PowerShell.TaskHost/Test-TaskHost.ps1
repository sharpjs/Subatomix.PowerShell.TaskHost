<#
    Copyright 2022 Jeffrey Sharp

    Permission to use, copy, modify, and distribute this software for any
    purpose with or without fee is hereby granted, provided that the above
    copyright notice and this permission notice appear in all copies.

    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
    WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
    MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
    ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
    WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
    ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
    OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
#>

#Requires -Version 7
using namespace Subatomix.PowerShell.TaskHost

[CmdletBinding()]
param (
    [Parameter(Position = 0)]
    [scriptblock] $ScriptBlock = { Write-Host "This is example output" }
)

process {
    $Script  = $ScriptBlock.ToString()
    $Factory = [TaskHostFactory]::new($Host)

    1..4 | ForEach-Object -Parallel {
        $ScriptBlock   = [ScriptBlock]::Create($using:Script)
        $Settings      = [System.Management.Automation.PSInvocationSettings]::new()
        $Settings.Host = ($using:Factory).Create()

        $Shell = [PowerShell]::Create()
        try {
            $Shell.AddScript($ScriptBlock).Invoke($null, $Settings)
        }
        catch {
            $Shell.Dispose()
        }
    }
}
