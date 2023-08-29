// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost.Commands;

[TestFixture]
public class CommandTests
{
    // Tests omitted where coverage provided by IntegrationTests

    private TestCommand Command { get; }

    public CommandTests()
    {
        Command = new TestCommand();
    }

    [Test]
    public void ScriptBlock_Get()
    {
        Command.ScriptBlock           .Should().NotBeNull();
        Command.ScriptBlock.ToString().Should().BeEmpty();
    }

    private class TestCommand : Command
    {
        protected override void Configure(Invocation invocation) { }
    }
}
