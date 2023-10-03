// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost.Internal;

/// <summary>
///   Handlers for module lifecycle events.
/// </summary>
public class ModuleLifecycleEvents : IModuleAssemblyInitializer
{
    /// <summary>
    ///   Invoked by PowerShell when the module is imported into a runspace.
    /// </summary>
    public void OnImport()
    {
        // Ensure that dependency assembly is loaded
        TaskHost.Initialize();
    }
}
