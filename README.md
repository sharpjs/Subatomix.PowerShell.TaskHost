# Subatomix.PowerShell.TaskHost

A PowerShell
[`PSHost`](https://docs.microsoft.com/en-us/dotnet/api/system.management.automation.host.pshost?view=powershellsdk-7.0.0)
wrapper to improve the clarity of output from parallel tasks.

The wrapper adds a prefix to each output line, identifying which task produced
the output.

```
╭────────────────┬──────────────────────────╮
│ Before         │ After                    │
╞════════════════╪══════════════════════════╡
│ Example output │ [Task 1]: Example output │
│ Example output │ [Task 2]: Example output │
│ Example output │ [Task 3]: Example output │
│ Example output │ [Task 4]: Example output │
╰────────────────┴──────────────────────────╯
```

The prefix affects all output *except* the following:
- Normal object output from PowerShell commands.
- Progress messages.
- Prompts for information.

## Status

[![Build](https://github.com/sharpjs/Subatomix.PowerShell.TaskHost/workflows/Build/badge.svg)](https://github.com/sharpjs/Subatomix.PowerShell.TaskHost/actions)
[![NuGet](https://img.shields.io/nuget/v/Subatomix.PowerShell.TaskHost.svg)](https://www.nuget.org/packages/Subatomix.PowerShell.TaskHost)
[![NuGet](https://img.shields.io/nuget/dt/Subatomix.PowerShell.TaskHost.svg)](https://www.nuget.org/packages/Subatomix.PowerShell.TaskHost)

- **Stable(ish):** A prior version has been in private use for years with no
                   reported defects, but a few changes have been made in
                   preparation for public release.  No significant changes
                   are planned.
- **Tested:**      100% coverage by automated tests.
- **Documented:**  IntelliSense on everything.  Quick-start guide below.

## Installation

Install
[this NuGet Package](https://www.nuget.org/packages/Subatomix.PowerShell.TaskHost)
in your project.

## Usage

### The Basics

These examples assume the existence of variables `host` and `scriptBlock` which
are a PowerShell `PSHost` and `ScriptBlock`, respectively.  Such a situation
might arise, for instance, when writing a PowerShell cmdlet.

First, import the namespace.

```csharp
using Subatomix.PowerShell.TaskHost;
```

Create a single factory instance.

```csharp
var factory = new TaskHostFactory(host);
```

**For each parallel task**, use the factory to create a host wrapper for that
task.

```csharp
var taskHost = factory.Create();
```

Then use the host wrapper with a new `PowerShell` instance for that task.

```csharp
using (var shell = PowerShell.Create())
{
    var settings = new PSInvocationSettings { Host = taskHost };
    shell.AddScript(scriptBlock).Invoke(null, settings);
}
```

### Advanced Usage

The host wrapper exposes a `Header` property to enable a task to examine and
change the header that appears in square brackets `[ ]` before each line of
output.  This property is modifiable from the task script itself.

```powershell
$Host.UI.Header = "new header"
```

Note that some PowerShell commands-that-run-commands, most notably
`ForEach-Object -Parallel`, expose their own wrappers via the `$Host` constant,
which can complicate the preceding example.  See
[this script](https://github.com/sharpjs/Subatomix.PowerShell.TaskHost/blob/main/Subatomix.PowerShell.TaskHost/Test-TaskHost.ps1)
for workarounds.

<!--
  Copyright 2023 Subatomix Research Inc.
  SPDX-License-Identifier: ISC
-->
