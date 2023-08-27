// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

public static class TaskInjectingRedirectorTests
{
    [TestFixture]
    public class Normal : RedirectorTestsBase
    {
        public Normal()
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

        protected override (string? input, string? output) SetUpTextFlow()
        {
            var input  =                 "text";
            var output = $"{L}{Task.Id}{R}text";

            return (input, output);
        }

        protected override void AssertStateInWrite()
        {
            // The injecting redirector should encode the current task into the
            // stream content and increment the task's retain count before
            // invoking the underlying write
            TaskInfo.Current.Should().BeSameAs(Task);
            Task.RetainCount.Should().Be(2);
        }

        protected override void Verify()
        {
            // The injecting redirector should not modify the current task or
            // its retain count after invoking the underlying write
            AssertStateInWrite();

            base.Verify();
        }
    }

    [TestFixture]
    public class NoCurrentTask
    {
        [Test]
        public void Construct_NoCurrentTask()
        {
            TaskInfo.Current.Should().BeNull();

            using var shell  = Sma.PowerShell.Create();
            using var output = new PSDataCollection<PSObject?>();
                  var cmdlet = new TestCmdlet();

            Invoking(() => new TaskInjectingRedirector(output, shell.Streams, cmdlet))
                .Should().Throw<InvalidOperationException>();
        }

        private class TestCmdlet : Cmdlet { }
    }

    [TestFixture]
    public class Nulls : RedirectorTestsBase
    {
        public Nulls()
        {
            TaskInfo.Current = Task;

            new TaskInjectingRedirector(Output, Streams, Cmdlet);
        }

        private bool _wasTaskEncoded;

        protected override (PSObject? input, object? output) SetUpObjectFlow()
        {
            _wasTaskEncoded = true;
            return (null, new TaskOutput(Task, null));
        }

        protected override (ErrorRecord? input, ErrorRecord? output) SetUpErrorRecordFlow()
        {
            // Cannot encode current task into null error record
            _wasTaskEncoded = false;
            return (null, null);
        }

        protected override (InformationRecord? input, InformationRecord? output) SetUpInformationRecordFlow()
        {
            // Cannot encode current task into null information record
            _wasTaskEncoded = false;
            return (null, null);
        }

        protected override (string? input, string? output) SetUpTextFlow()
        {
            _wasTaskEncoded = true;
            return (null, $"{L}{Task.Id}{R}");
        }

        protected override void AssertStateInWrite()
        {
            // Where possible, the injecting redirector should encode the
            // current task into the stream content and increment the task's
            // retain count before invoking the underlying write
            TaskInfo.Current.Should().BeSameAs(Task);
            Task.RetainCount.Should().Be(_wasTaskEncoded ? 2 : 1);
        }

        protected override void Verify()
        {
            // The injecting redirector should not modify the current task or
            // its retain count after invoking the underlying write
            AssertStateInWrite();

            base.Verify();
        }
    }

    [TestFixture]
    public class InternalNulls : RedirectorTestsBase
    {
        public InternalNulls()
        {
            TaskInfo.Current = Task;

            new TaskInjectingRedirector(Output, Streams, Cmdlet);
        }

        protected override (PSObject? input, object? output) SetUpObjectFlow()
        {
            return (null, new TaskOutput(Task, null));
        }

        protected override (ErrorRecord? input, ErrorRecord? output) SetUpErrorRecordFlow()
        {
            var (_, outputText) = SetUpTextFlow();

            var input = new ErrorRecord(new(), "id", ErrorCategory.InvalidOperation, "target");

            var output = new ErrorRecord(new(), "id", ErrorCategory.InvalidOperation, "target")
            {
                ErrorDetails = new(outputText)
            };

            return (input, output);
        }

        protected override (string? input, string? output) SetUpTextFlow()
        {
            return (null, $"{L}{Task.Id}{R}");
        }

        protected override void AssertStateInWrite()
        {
            // The injecting redirector should encode the current task into the
            // stream content and increment the task's retain count before
            // invoking the underlying write
            TaskInfo.Current.Should().BeSameAs(Task);
            Task.RetainCount.Should().Be(2);
        }

        protected override void Verify()
        {
            // The injecting redirector should not modify the current task or
            // its retain count after invoking the underlying write
            AssertStateInWrite();

            base.Verify();
        }
    }
}
