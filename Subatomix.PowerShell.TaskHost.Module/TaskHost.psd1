# Copyright 2023 Subatomix Research Inc.
# SPDX-License-Identifier: ISC
@{
    # Identity
    GUID          = 'd75e4bbd-4efd-4bb1-8324-b6d4ae0ed9a9'
    RootModule    = 'TaskHost.dll'
    ModuleVersion = '1.1.0'

    # General
    Description = 'A PowerShell PSHost wrapper to improve the clarity of output from long-running, potentially parallel tasks.'
    Author      = 'Jeffrey Sharp'
    CompanyName = 'Subatomix Research Inc.'
    Copyright   = '© 2023 Subatomix Research Inc.'

    # Requirements
    CompatiblePSEditions = 'Core'
    PowerShellVersion    = '7.0'
    #RequiredModules     = @(...)
    #RequiredAssemblies  = @(...)

    # Initialization
    #ScriptsToProcess = @(...)
    #TypesToProcess   = @(...)
    #FormatsToProcess = @(...)
    #NestedModules    = @(...)

    # Exports
    # NOTE: Use empty arrays to indicate no exports.
    FunctionsToExport    = @()
    VariablesToExport    = @()
    AliasesToExport      = @()
    DscResourcesToExport = @()
    CmdletsToExport      = @(
        "Invoke-Task"
        "Use-TaskHost"
    )

    # Discoverability and URLs
    PrivateData = @{
        PSData = @{
            # Additional metadata
            Prerelease   = 'local'
            ProjectUri   = 'https://github.com/sharpjs/Subatomix.PowerShell.TaskHost'
            ReleaseNotes = "https://github.com/sharpjs/Subatomix.PowerShell.TaskHost/blob/main/CHANGES.md"
            LicenseUri   = 'https://github.com/sharpjs/Subatomix.PowerShell.TaskHost/blob/main/LICENSE.txt'
            IconUri      = 'https://github.com/sharpjs/Subatomix.PowerShell.TaskHost/blob/main/icon.png'
            Tags         = @(
                "PSHost",
                "PSEdition_Core", "Windows", "Linux", "MacOS"
            )
        }
    }
}
