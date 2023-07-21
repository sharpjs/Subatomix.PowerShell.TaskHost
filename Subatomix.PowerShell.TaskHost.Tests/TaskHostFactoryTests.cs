// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

[TestFixture]
public class TaskHostFactoryTests
{
    [Test]
    public void Version_Get()
    {
        TaskHostFactory.Version
            .Should().NotBeNull().And.NotBe(new Version());
    }

    [Test]
    public void Construct_NullHost()
    {
        Invoking(() => new TaskHostFactory(null!))
            .Should().ThrowExactly<ArgumentNullException>()
            .Where(e => e.ParamName == "host");
    }

    [Test]
    public void Construct_NullHostUI()
    {
        var host = Mock.Of<PSHost>(); // .UI == null

        Invoking(() => new TaskHostFactory(host))
            .Should().ThrowExactly<ArgumentNullException>()
            .Where(e => e.ParamName == "host.UI");
    }
}
