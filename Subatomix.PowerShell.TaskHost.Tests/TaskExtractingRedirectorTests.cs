// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

[TestFixture]
public class TaskExtractingRedirectorTests : RedirectorTestsBase
{
    protected TaskInfo Task { get; }

    public TaskExtractingRedirectorTests()
    {
        // Assume that TaskInjectingRedictor encoded a task id into the stream
        // content and incremented the task's retain count

        Task = new TaskInfo(GetType().Name);
        Task.Retain();

        new TaskExtractingRedirector(Output, Streams, Cmdlet);
    }

    protected override (PSObject? input, object? output) SetUpObjectFlow()
    {
        var obj    = new object();
        var input  = new PSObject(new TaskOutput(Task, obj));
        var output = obj;

        return (input, output);
    }

    protected override (string? input, string? output) SetUpTextFlow()
    {
        var input  = $"{L}{Task.Id}{R}text";
        var output =                 "text";

        return (input, output);
    }

    protected override void AssertStateInWrite()
    {
        // The extracting redirector should decode the task from the stream
        // content and make it current before invoking the underlying write

        TaskInfo.Current.Should().BeSameAs(Task);
        Task.RetainCount.Should().Be(2);
    }

    protected override void Verify()
    {
        // The extracting redirector should decrement the current task's retain
        // count and restore the previous current task after invoking the
        // underlying write

        TaskInfo.Current.Should().BeNull();
        Task.RetainCount.Should().Be(1);

        base.Verify();
    }

    protected override void CleanUp(bool managed)
    {
        TaskInfo.Current = null;
        Task.ReleaseAll();

        base.CleanUp(managed);
    }
}
