// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

[TestFixture]
public class TaskInjectingRedirectorTests : RedirectorTestsBase
{
    protected TaskInfo Task { get; }

    public TaskInjectingRedirectorTests()
    {
        Task = new TaskInfo(GetType().Name);
        TaskInfo.Current = Task;

        new TaskInjectingRedirector(Output, Streams, Cmdlet);
    }

    protected override (PSObject? input, object? output) SetUpObjectFlow()
    {
        var input  = new PSObject();
        var output = new TaskOutput(Task, input);

        return (input, output);
    }

    protected override (string? input, string? output) SetUpTextFlow()
    {
        var input  = "text";
        var output = $"{L}{Task.Id}{R}text";

        return (input, output);
    }

    protected override void AssertStateInWrite()
    {
        // The injecting redirector should encode the current task into the
        // stream content and increment the task's retain count before invoking
        // the underlying write

        TaskInfo.Current.Should().BeSameAs(Task);
        Task.RetainCount.Should().Be(2);
    }

    protected override void Verify()
    {
        // The injecting redirector should not modify the current task or its
        // retain count after invoking the underlying write

        AssertStateInWrite();

        base.Verify();
    }

    protected override void CleanUp(bool managed)
    {
        TaskInfo.Current = null;
        Task.ReleaseAll();

        base.CleanUp(managed);
    }
}
