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
            // The extracting redirector should decrement the current task's
            // retain count and restore the previous current task after
            // invoking the underlying write
            TaskInfo.Current.Should().BeNull();
            Task.RetainCount.Should().Be(1);
        }
    }

    [TestFixture]
    public class Nulls : RedirectorTestsBase
    {
        public Nulls()
        {
            new TaskExtractingRedirector(Output, Streams, Cmdlet);
        }

        protected override (PSObject? input, object? output) SetUpObjectFlow()
        {
            return (null, null);
        }

        protected override (ErrorRecord? input, ErrorRecord? output) SetUpErrorRecordFlow()
        {
            return (null, null);
        }

        protected override (InformationRecord? input, InformationRecord? output) SetUpInformationRecordFlow()
        {
            return (null, null);
        }

        protected override (string? input, string? output) SetUpTextFlow()
        {
            return (null, null);
        }
    }

    [TestFixture]
    public class NoTaskId : RedirectorTestsBase
    {
        public NoTaskId()
        {
            new TaskExtractingRedirector(Output, Streams, Cmdlet);
        }
    }

    [TestFixture]
    public class TaskNotFound : RedirectorTestsBase
    {
        public TaskNotFound()
        {
            new TaskExtractingRedirector(Output, Streams, Cmdlet);
        }

        protected override (string? input, string? output) SetUpTextFlow()
        {
            var input  = $"{L}999999999999999999{R}text";
            var output =                          "text";

            return (input, output);
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

            var ok = TaskEncoding.TryExtractId(ref text, out var id);

            ok  .Should().Be(expectedOk);
            text.Should().Be(expectedText);
            id  .Should().Be(expectedId);
        }
    }
}
