// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

[TestFixture]
public class TaskOutputTests
{
    [Test]
    public void Construct_NullTask()
    {
        Invoking(() => new TaskOutput(null!, new()))
            .Should().Throw<ArgumentNullException>();
    }
}
