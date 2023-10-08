// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

using static TaskEncoding;

public static partial class TaskEncodingTests
{
    public const char
        L = StartDelimiter,
        R = EndDelimiter;

    #region Injection

    [TestFixture]
    public class Inject_NoCurrentTask : Inject_Base
    {
        // When there is no current task, injection should have no effect: it
        // should not modify the target object and should not increment any
        // task's retain count.
    }

    [TestFixture]
    public class Inject_Normal : Inject_WithCurrentTask
    {
        // In the normal case, injection should encode the current task's id
        // into the target object and increment the task's retain count.

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
    }

    [TestFixture]
    public class Inject_Nulls : Inject_WithCurrentTask
    {
        // In the null-target case, where possible, injection should encode the
        // current task's id into a synthesized target object and increment the
        // task's retain count.

        protected override (PSObject? input, object? output) SetUpObjectFlow()
        {
            return (null, new TaskOutput(Task, null));
        }

        protected override (string? input, string? output) SetUpTextFlow()
        {
            return (null, $"{L}{Task.Id}{R}");
        }

        protected override (ErrorRecord? input, ErrorRecord? output) SetUpErrorRecordFlow()
        {
            // Assume PowerShell does not send a null record; on violation, no action
            ExpectedRetainCount--;
            return (null, null);
        }

        protected override (InformationRecord? input, InformationRecord? output) SetUpInformationRecordFlow()
        {
            // Assume PowerShell does not send a null record; on violation, no action
            ExpectedRetainCount--;
            return (null, null);
        }

        protected override (InformationalRecord? input, InformationalRecord? output) SetUpInformationalRecordFlow()
        {
            // Assume PowerShell does not send a null record; on violation, no action
            ExpectedRetainCount--;
            return (null, null);
        }
    }

    [TestFixture]
    public class Inject_Nested : Inject_WithCurrentTask
    {
        // When a target object already has an injected task id, a further
        // injection has no effect: it does not modify the target object and
        // does not increment the current task's retain count.

        public TaskInfo OtherTask { get; }

        public Inject_Nested()
        {
            OtherTask = new TaskInfo("Other");

            ExpectedRetainCount--;
        }

        protected override void Verify()
        {
            OtherTask.RetainCount.Should().Be(1);

            base.Verify();
        }

        protected override void CleanUp(bool managed)
        {
            OtherTask.ReleaseAll();

            base.CleanUp(managed);
        }

        protected override (PSObject? input, object? output) SetUpObjectFlow()
        {
            var input  = new PSObject(new TaskOutput(OtherTask, new PSObject()));
            var output = input;

            return (input, output);
        }

        protected override (string? input, string? output) SetUpTextFlow()
        {
            var input  = $"{L}{OtherTask.Id}{R}text";
            var output = input;

            return (input, output);
        }
    }

    [TestFixture]
    public class Inject_InternalNulls : Inject_WithCurrentTask
    {
        // When the target object has some internal null, injection behaves
        // identically to the normal case.

        protected override (PSObject? input, object? output) SetUpObjectFlow()
        {
            // Same as Nulls case
            return (null, new TaskOutput(Task, null));
        }

        protected override (string? input, string? output) SetUpTextFlow()
        {
            // Same as Nulls case
            return (null, $"{L}{Task.Id}{R}");
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
    }

    public abstract class Inject_WithCurrentTask : Inject_Base
    {
        protected TaskInfo Task { get; }

        protected int ExpectedRetainCount { get; set; }

        public Inject_WithCurrentTask()
        {
            Task = new TaskInfo(GetType().Name);
            TaskInfo.Current = Task;

            // By default, expect injection to increment the task's retain count
            ExpectedRetainCount = 2;
        }

        protected override void Verify()
        {
            Task.RetainCount.Should().Be(ExpectedRetainCount);

            base.Verify();
        }

        protected override void CleanUp(bool managed)
        {
            TaskInfo.Current = null;
            Task.ReleaseAll();

            base.CleanUp(managed);
        }
    }

    public abstract class Inject_Base : TestHarnessBase
    {
        [Test]
        public void Inject_Object()
        {
            var (input, output) = SetUpObjectFlow();

            Inject(input).Should().BeEquivalentTo(output);
        }

        [Test]
        public void Inject_String()
        {
            var (input, output) = SetUpTextFlow();

            Inject(input).Should().Be(output);
        }

        [Test]
        public void Inject_ErrorRecord()
        {
            var (input, output) = SetUpErrorRecordFlow();

            Inject(input).Should().BeEquivalentTo(output);
        }

        [Test]
        public void Inject_InformationRecord()
        {
            var (input, output) = SetUpInformationRecordFlow();

            Inject(input).Should().BeEquivalentTo(output, _ => _.Excluding(r => r!.TimeGenerated));
        }

        [Test]
        public void Inject_InformationalRecord() // warning, verbose, debug
        {
            var (input, output) = SetUpInformationalRecordFlow();

            Inject(input).Should().BeEquivalentTo(output);
        }

        protected virtual (PSObject? input, object? output) SetUpObjectFlow()
        {
            var obj = new PSObject();

            return (obj, obj);
        }

        protected virtual (string? input, string? output) SetUpTextFlow()
        {
            return ("text", "text");
        }

        protected virtual (ErrorRecord? input, ErrorRecord? output) SetUpErrorRecordFlow()
        {
            var (inputText, outputText) = SetUpTextFlow();

            var input = new ErrorRecord(new(), "id", ErrorCategory.InvalidOperation, "target")
            {
                ErrorDetails = new(inputText)
            };

            var output = new ErrorRecord(new(), "id", ErrorCategory.InvalidOperation, "target")
            {
                ErrorDetails = new(outputText)
            };

            return (input, output);
        }

        protected virtual (InformationRecord? input, InformationRecord? output) SetUpInformationRecordFlow()
        {
            var (inputText, outputText) = SetUpTextFlow();

            var input  = new InformationRecord("messageData", inputText);
            var output = new InformationRecord("messageData", outputText);

            return (input, output);
        }

        protected virtual (InformationalRecord? input, InformationalRecord? output) SetUpInformationalRecordFlow()
        {
            var (inputText, outputText) = SetUpTextFlow();

            var input  = new VerboseRecord(inputText);
            var output = new VerboseRecord(outputText);

            return (input, output);
        }
    }

    #endregion
    #region Extraction

    [TestFixture]
    public class Extract_NoTaskId : Extract_Base
    {
        // When the target object carries no encoded task id, extraction should
        // have no effect: it should not modify the target object and should
        // not return a task scope.
    }

    [TestFixture]
    public class Extract_TaskNotFound : Extract_Base
    {
        // In the task-id-not-found case, extraction should remove the task id
        // from the target object but should not return a task scope.

        protected override (string? input, string? output) SetUpTextFlow()
        {
            var input  = $"{L}999999999999999999{R}text";
            var output =                          "text";

            return (input, output);
        }
    }

    [TestFixture]
    public class Extract_Nulls : Extract_Base
    {
        // In the null-target case, extraction should have no effect: it should
        // not modify any target object and should not return a task scope.

        protected override (object? input, object? output) SetUpObjectFlow()
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
    public class Extract_Normal : Extract_Base
    {
        // In the normal case, extraction should remove the task id from the
        // target object and return a scope referencing the resolved task.

        public TaskInfo Task { get; }

        public Extract_Normal()
        {
            Task = new TaskInfo(GetType().Name);
            Task.Retain();
        }

        protected override void CleanUp(bool managed)
        {
            TaskInfo.Current = null;
            Task.ReleaseAll();

            base.CleanUp(managed);
        }

        protected override (object? input, object? output) SetUpObjectFlow()
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

        protected override void AssertScope(TaskScope? scope)
        {
            scope      .Should().NotBeNull();
            scope!.Task.Should().BeSameAs(Task);
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

    public abstract class Extract_Base : TestHarnessBase
    {
        [Test]
        public void Extract_Object()
        {
            var (input, output) = SetUpObjectFlow();

            using var scope = Extract(ref input);

            AssertScope(scope);

            input.Should().BeEquivalentTo(output);
        }

        [Test]
        public void Extract_String()
        {
            var (input, output) = SetUpTextFlow();

            using var scope = Extract(ref input);

            AssertScope(scope);

            input.Should().Be(output);
        }

        [Test]
        public void Extract_ErrorRecord()
        {
            var (input, output) = SetUpErrorRecordFlow();

            using var scope = Extract(ref input);

            AssertScope(scope);

            input.Should().BeEquivalentTo(output);
        }

        [Test]
        public void Extract_InformationRecord()
        {
            var (input, output) = SetUpInformationRecordFlow();

            using var scope = Extract(ref input);

            AssertScope(scope);

            input.Should().BeEquivalentTo(output, _ => _.Excluding(r => r!.TimeGenerated));
        }

        protected virtual (object? input, object? output) SetUpObjectFlow()
        {
            var obj = new PSObject();

            return (obj, obj);
        }

        protected virtual (string? input, string? output) SetUpTextFlow()
        {
            return ("text", "text");
        }

        protected virtual (ErrorRecord? input, ErrorRecord? output) SetUpErrorRecordFlow()
        {
            var (inputText, outputText) = SetUpTextFlow();

            var input = new ErrorRecord(new(), "id", ErrorCategory.InvalidOperation, "target")
            {
                ErrorDetails = new(inputText)
            };

            var output = new ErrorRecord(new(), "id", ErrorCategory.InvalidOperation, "target")
            {
                ErrorDetails = new(outputText)
            };

            return (input, output);
        }

        protected virtual (InformationRecord? input, InformationRecord? output) SetUpInformationRecordFlow()
        {
            var (inputText, outputText) = SetUpTextFlow();

            var input  = new InformationRecord("messageData", inputText);
            var output = new InformationRecord("messageData", outputText);

            return (input, output);
        }

        protected virtual void AssertScope(TaskScope? scope)
        {
            scope.Should().BeNull();
        }
    }

    #endregion
}
