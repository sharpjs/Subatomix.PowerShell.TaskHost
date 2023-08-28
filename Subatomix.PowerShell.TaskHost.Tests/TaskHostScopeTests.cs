// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

[TestFixture]
public class TaskHostScopeTests
{
    [Test]
    public void Construct_NullHost()
    {
        Invoking(() => new TaskHostScope(null!))
            .Should().Throw<ArgumentNullException>();
    }
}
