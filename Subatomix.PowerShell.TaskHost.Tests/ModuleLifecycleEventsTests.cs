// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost.Internal;

[TestFixture]
public class ModuleLifecycleEventsTests
{
    [Test]
    public void OnImport()
    {
        new ModuleLifecycleEvents().OnImport();
    }
}
