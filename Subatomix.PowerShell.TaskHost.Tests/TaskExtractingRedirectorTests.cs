// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

using static RedirectorTestsBase;

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

    [TestFixture]
    public class Invariants
    {
        [Test]
        [TestCase(null,   null,  false, 0)]
        [TestCase("1",    "1",   false, 0)]
        [TestCase("<1",   "<1",  false, 0)]
        [TestCase("<>",   "<>",  false, 0)]
        [TestCase("<x>",  "<x>", false, 0)]
        [TestCase("<1>",  "",    true,  1)]
        [TestCase("<1>x", "x",   true,  1)]
        public void TryExtractId(string? text, string? expectedText, bool expectedOk, long expectedId)
        {
            text         = text?        .Replace('<', L).Replace('>', R);
            expectedText = expectedText?.Replace('<', L).Replace('>', R);

            var ok = TaskExtractingRedirector.TryExtractId(ref text, out var id);

            ok  .Should().Be(expectedOk);
            text.Should().Be(expectedText);
            id  .Should().Be(expectedId);
        }
    }
}
