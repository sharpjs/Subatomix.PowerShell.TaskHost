// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

[assembly: Parallelizable(ParallelScope.All)]
[assembly: FixtureLifeCycle(LifeCycle.InstancePerTestCase)]

#if ENFORCE_GOOD_HYGIENE
namespace Subatomix.PowerShell.TaskHost;

[SetUpFixture]
public static class TestSuite
{
    [OneTimeTearDown]
    public static void TearDown()
    {
        TaskInfo.All.Should().BeEmpty();
    }
}
#endif
