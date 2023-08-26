// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

public static class TaskExtractingRedirectorTests
{
    [TestFixture]
    public class Normal : RedirectorTestsBase
    {
        public Normal()
        {
            // Assume that TaskInjectingRedictor encoded a task id into the
            // stream content and incremented the task's retain count
            Task.Retain();

            new TaskExtractingRedirector(Output, Streams, Cmdlet);
        }

        protected override (PSObject? input, object? output) SetUpObjectFlow()
        {
            var output = new object();
            var input  = new PSObject(new TaskOutput(Task, output));

            return (input, output);
        }

        protected override (string input, string output) SetUpTextFlow()
        {
            var input  = $"{L}{Task.Id}{R}text";
            var output =                 "text";

            return (input, output);
        }

        protected override void AssertStateInWrite()
        {
            // The redirector should extract the task and make it current
            TaskInfo.Current.Should().BeSameAs(Task);
            Task.RetainCount.Should().Be(2);
        }

        protected override void Verify()
        {
            // The redirector should restore the previous current task
            TaskInfo.Current.Should().BeNull();
            Task.RetainCount.Should().Be(1);
        }
    }

    [TestFixture]
    public class NoTaskId : RedirectorTestsBase
    {
        public NoTaskId()
        {
            new TaskExtractingRedirector(Output, Streams, Cmdlet);
        }

        protected override void AssertStateInWrite()
        {
            // The redirector should not extract any task or make one current
            TaskInfo.Current.Should().BeNull();
            Task.RetainCount.Should().Be(1);
        }
    }

    [TestFixture]
    public class TaskNotFound : RedirectorTestsBase
    {
        public TaskNotFound()
        {
            new TaskExtractingRedirector(Output, Streams, Cmdlet);
        }

        protected override (string input, string output) SetUpTextFlow()
        {
            var input  = $"{L}999999999999999999{R}text";
            var output =                          "text";

            return (input, output);
        }

        protected override void AssertStateInWrite()
        {
            // The redirector should not extract any task or make one current
            TaskInfo.Current.Should().BeNull();
            Task.RetainCount.Should().Be(1);
        }
    }
}
