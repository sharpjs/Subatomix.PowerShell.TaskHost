# Subatomix.PowerShell.TaskHost

A thread-safe PowerShell
[`PSHost`](https://learn.microsoft.com/en-us/dotnet/api/system.management.automation.host.pshost)
wrapper to improve the clarity of output from long-running, possibly parallel
tasks.

The wrapper adds a prefix to each output line, identifying the elapsed time and
which task produced the output.  Both parts are optional.

```
╭────────────────┬──────────────────────────────────────╮
│ Before         │ After                                │
╞════════════════╪══════════════════════════════════════╡
│ Example output │ [+00:00:00] [Task 1]: Example output │
│ Example output │ [+00:00:07] [Task 2]: Example output │
│ Example output │ [+00:00:42] [Task 3]: Example output │
│ Example output │ [+00:01:23] [Task 4]: Example output │
╰────────────────┴──────────────────────────────────────╯
```

The prefix affects all output *except* the following:
- Normal object output from PowerShell commands.
- Progress messages.
- Prompts for information.

## Status

[![Build](https://github.com/sharpjs/Subatomix.PowerShell.TaskHost/workflows/Build/badge.svg)](https://github.com/sharpjs/Subatomix.PowerShell.TaskHost/actions)
[![Build](https://img.shields.io/badge/coverage-100%25-brightgreen.svg)](https://github.com/sharpjs/Subatomix.PowerShell.TaskHost/actions)
[![NuGet](https://img.shields.io/nuget/v/Subatomix.PowerShell.TaskHost.svg)](https://www.nuget.org/packages/Subatomix.PowerShell.TaskHost)
[![NuGet](https://img.shields.io/nuget/dt/Subatomix.PowerShell.TaskHost.svg)](https://www.nuget.org/packages/Subatomix.PowerShell.TaskHost)

- **Stable:**      A prior version has been in private use for years with no
                   reported defects.
- **Tested:**      100% coverage by automated tests.
- **Documented:**  IntelliSense on everything.  Quick-start guide below.

## Installation

Install
[this NuGet Package](https://www.nuget.org/packages/Subatomix.PowerShell.TaskHost)
in your project.

## Usage

### The Basics

This package is usable from PowerShell script, from PowerShell binary modules,
or from applications that host PowerShell.

First, import the namespace.

```ps1
# PowerShell (must be at top of file)
using namespace Subatomix.PowerShell.TaskHost
using namespace System.Management.Automation
```

```cs
// C#
using Subatomix.PowerShell.TaskHost;
using System.Management.Automation;
```

Create the variables that later examples need.

```ps1
# PowerShell
$ScriptBlock = {
    # The commands you want to run go here
}
```

```cs
// C#
var host        = ...; // a PSHost
var scriptBlock = ...; // a ScriptBlock
```

Create a single factory instance.

```ps1
# PowerShell
$Factory = [TaskHostFactory]::new($Host, <#withElapsed:#> $true)
```

```cs
// C#
var factory = new TaskHostFactory(host, withElapsed: true);
```

**For each task**, use the factory to create a host wrapper for that
task.

```ps1
# PowerShell
$TaskHost = $Factory.Create()
```

```cs
// C#
var taskHost = factory.Create();
```

Then use the host wrapper with a new `PowerShell` instance for that task.

```ps1
# PowerShell
$Shell = [PowerShell]::Create()
try {
    $Settings = [PSInvocationSettings]::new()
    $Settings.Host = $TaskHost
    $Shell.AddScript($ScriptBlock).Invoke($null, $Settings)
}
finally {
    $Shell.Dispose()
}
```

```cs
// C#
using (var shell = PowerShell.Create())
{
    var settings = new PSInvocationSettings { Host = taskHost };
    shell.AddScript(scriptBlock).Invoke(null, settings);
}
```

### Advanced Usage

The PowerShell `ForEach-Object -Parallel` command complicates the preceding
example. See
[this script](https://github.com/sharpjs/Subatomix.PowerShell.TaskHost/blob/main/Subatomix.PowerShell.TaskHost/Test-TaskHost.ps1)
for a complete example.

The host wrapper exposes a `Header` property to enable a task to examine and
change the header that appears in square brackets `[ ]` before each line of
output.  This property is modifiable from the task script itself.

```ps1
$Host.UI.Header = "new header"
```

Setting the property to an empty string disables the header.

```ps1
$Host.UI.Header = ""
```

<!--
  Copyright 2023 Subatomix Research Inc.
  SPDX-License-Identifier: ISC
-->
