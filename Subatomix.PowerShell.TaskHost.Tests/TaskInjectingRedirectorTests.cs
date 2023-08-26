// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

[TestFixture]
public class TaskInjectingRedirectorTests : RedirectorTestsBase
{
    public TaskInjectingRedirectorTests()
    {
        TaskInfo.Current = Task;

        new TaskInjectingRedirector(Output, Streams, Cmdlet);
    }

    protected override (PSObject? input, object? output) SetUpObjectFlow()
    {
        var input  = new PSObject();
        var output = new TaskOutput(Task, input);

        return (input, output);
    }

    protected override (string input, string output) SetUpTextFlow()
    {
        var input  =                 "text";
        var output = $"{L}{Task.Id}{R}text";

        return (input, output);
    }

    protected override void Verify()
    {
        // The redirector should encode the task id into the stream content and
        // increment the task's retain count
        Task.RetainCount.Should().Be(2);

        base.Verify();
    }
}
