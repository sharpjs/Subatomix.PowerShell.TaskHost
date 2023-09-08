// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using Microsoft.PowerShell.Commands;

namespace Subatomix.PowerShell.TaskHost;

[TestFixture]
public class DefaultStreamsFixupTests : IDisposable
{
    private readonly Runspace _runspace;

    private DefaultStreamsFixup? _fixup;

    public DefaultStreamsFixupTests()
    {
        _runspace = RunspaceFactory.CreateRunspace();
        _runspace.Open();

        Runspace.DefaultRunspace = _runspace;
    }

    public void Dispose()
    {
        Runspace.DefaultRunspace = null;

        _fixup?  .Dispose();
        _runspace.Dispose();
    }

    [Test]
    public void Construct_NullCmdlet()
    {
        Invoking(() => new DefaultStreamsFixup(null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Construct_NullCommandRuntime()
    {
        TestFixup(runtime: null);
    }

    [Test]
    public void Construct() // happy path
    {
        var runtime = new FakeRuntime();

        TestFixup(runtime);

        runtime.AssertFixups(output: true, errors: true);
    }

    [Test]
    public void Construct_ErrorsMerged() // 2>&1
    {
        var runtime = new FakeRuntime
        {
            ErrorMergeTo = MergeTo.Something
        };

        TestFixup(runtime);

        runtime.AssertFixups(output: true, errors: false);
    }

    [Test]
    public void Construct_OutputRedirectedToNull() // > $null
    {
        var runtime = new FakeRuntime
        {
            OutputPipe = new FakePipe { NullPipe = true }
        };

        TestFixup(runtime);

        runtime.AssertFixups(output: false, errors: true);
    }

    [Test]
    public void Construct_ErrorsRedirectedToNull() // 2> $null
    {
        var runtime = new FakeRuntime
        {
            ErrorOutputPipe = new FakePipe { NullPipe = true }
        };

        TestFixup(runtime);

        runtime.AssertFixups(output: true, errors: false);
    }

    [Test]
    public void Construct_OutputRedirectedToFile() // > file
    {
        var runtime = new FakeRuntime
        {
            OutputPipe = new FakePipe { PipelineProcessor = new() }
        };

        TestFixup(runtime);

        runtime.AssertFixups(output: false, errors: true);
    }

    [Test]
    public void Construct_ErrorsRedirectedToFile() // 2> file
    {
        var runtime = new FakeRuntime
        {
            ErrorOutputPipe = new FakePipe { PipelineProcessor = new() }
        };

        TestFixup(runtime);

        runtime.AssertFixups(output: true, errors: false);
    }

    [Test]
    public void Construct_ReflectionFail_ErrorMergeTo_Missing()
    {
        var runtime = new FakeRuntime_NoErrorMergeTo();

        TestFixup(runtime);

        runtime.AssertFixups(output: true, errors: false);
    }

    [Test]
    public void Construct_ReflectionFail_ErrorMergeTo_Null()
    {
        var runtime = new FakeRuntime
        {
            ErrorMergeTo = null
        };

        TestFixup(runtime);

        runtime.AssertFixups(output: true, errors: false);
    }

    [Test]
    public void Construct_ReflectionFail_ErrorMergeTo_NotEnum()
    {
        var runtime = new FakeRuntime
        {
            ErrorMergeTo = new object()
        };

        TestFixup(runtime);

        runtime.AssertFixups(output: true, errors: false);
    }

    [Test]
    public void Construct_ReflectionFail_OutputPipe_Missing()
    {
        var runtime = new FakeRuntime_NoOutputPipe();

        TestFixup(runtime);

        runtime.AssertFixups(output: false, errors: true);
    }

    [Test]
    public void Construct_ReflectionFail_ErrorOutputPipe_Missing()
    {
        var runtime = new FakeRuntime_NoErrorOutputPipe();

        TestFixup(runtime);

        runtime.AssertFixups(output: true, errors: false);
    }

    [Test]
    public void Construct_ReflectionFail_OutputPipe_Null()
    {
        var runtime = new FakeRuntime
        {
            OutputPipe = null
        };

        TestFixup(runtime);

        runtime.AssertFixups(output: false, errors: true);
    }

    [Test]
    public void Construct_ReflectionFail_ErrorOutputPipe_Null()
    {
        var runtime = new FakeRuntime
        {
            ErrorOutputPipe = null
        };

        TestFixup(runtime);

        runtime.AssertFixups(output: true, errors: false);
    }

    [Test]
    public void Construct_ReflectionFail_OutputPipe_NoDefaultConstructor()
    {
        var runtime = new FakeRuntime
        {
            OutputPipe = new FakePipe_NoDefaultConstructor(null)
        };

        TestFixup(runtime);

        runtime.AssertFixups(output: false, errors: true);
    }

    [Test]
    public void Construct_ReflectionFail_ErrorOutputPipe_NoDefaultConstructor()
    {
        var runtime = new FakeRuntime
        {
            // Prevent reuse of output pipe as error pipe
            OutputPipe      = new FakePipe { NullPipe = true },
            ErrorOutputPipe = new FakePipe_NoDefaultConstructor(null)
        };

        TestFixup(runtime);

        runtime.AssertFixups(output: false, errors: false);
    }

    [Test]
    public void Construct_ReflectionFail_NullPipe_Missing()
    {
        var runtime = new FakeRuntime
        {
            ErrorOutputPipe = new FakePipe_NoNullPipe()
        };

        TestFixup(runtime);

        runtime.AssertFixups(output: true, errors: false);
    }

    [Test]
    public void Construct_ReflectionFail_NullPipe_Null()
    {
        var runtime = new FakeRuntime
        {
            ErrorOutputPipe = new FakePipe { NullPipe = null }
        };

        TestFixup(runtime);

        runtime.AssertFixups(output: true, errors: false);
    }

    [Test]
    public void Construct_ReflectionFail_NullPipe_UnexpectedType()
    {
        var runtime = new FakeRuntime
        {
            ErrorOutputPipe = new FakePipe { NullPipe = new object() }
        };

        TestFixup(runtime);

        runtime.AssertFixups(output: true, errors: false);
    }

    [Test]
    public void Construct_ReflectionFail_PipelineProcessor_Missing()
    {
        var runtime = new FakeRuntime
        {
            ErrorOutputPipe = new FakePipe_NoPipelineProcessor()
        };

        TestFixup(runtime);

        runtime.AssertFixups(output: true, errors: false);
    }

    [Test]
    public void Construct_ReflectionFail_ExternalWriter_Missing()
    {
        var runtime = new FakeRuntime
        {
            // Prevent reuse of output pipe as error pipe
            OutputPipe      = new FakePipe { NullPipe = true },
            ErrorOutputPipe = new FakePipe_NoExternalWriter()
        };

        TestFixup(runtime);

        runtime.AssertFixups(output: false, errors: false);
    }

    [Test]
    public void Construct_ReflectionFail_ExternalWriter_ReadOnly()
    {
        var runtime = new FakeRuntime
        {
            // Prevent reuse of output pipe as error pipe
            OutputPipe      = new FakePipe { NullPipe = true },
            ErrorOutputPipe = new FakePipe_ReadOnlyExternalWriter()
        };

        TestFixup(runtime);

        runtime.AssertFixups(output: false, errors: false);
    }

    private void TestFixup(FakeRuntimeBase? runtime)
    {
        runtime?.CaptureOriginalValues();

        var cmdlet = new TestCmdlet { CommandRuntime = runtime };

        _fixup = new DefaultStreamsFixup(cmdlet);
    }

    private class TestCmdlet : Cmdlet { }

    private class FakeRuntime : FakeRuntimeBase
    {
        internal object? OutputPipe      { get; set; }
        internal object? ErrorOutputPipe { get; set; }
        internal object? ErrorMergeTo    { get; set; } = MergeTo.None;
    }

    private class FakeRuntime_NoOutputPipe : FakeRuntimeBase
    {
        //absent object? OutputPipe      { get; set; }
        internal object? ErrorOutputPipe { get; set; }
        internal object? ErrorMergeTo    { get; set; } = MergeTo.None;
    } 

    private class FakeRuntime_NoErrorOutputPipe : FakeRuntimeBase
    {
        internal object? OutputPipe      { get; set; }
        //absent object? ErrorOutputPipe { get; set; }
        internal object? ErrorMergeTo    { get; set; } = MergeTo.None;
    } 

    private class FakeRuntime_NoErrorMergeTo : FakeRuntimeBase
    {
        internal object? OutputPipe      { get; set; }
        internal object? ErrorOutputPipe { get; set; }
        //absent object? ErrorMergeTo    { get; set; } = MergeTo.None;
    }

    private class FakePipe
    {
        // DownstreamCmdlet must be null in new instances of this type to match
        // the behavior of PowerShell's Pipe class, because the happy path
        // through the code under test will create a new instance of this type.
        // The other FakePipe_* types are for unhappy paths and do not face the
        // same requirement.

        internal object? DownstreamCmdlet  { get; set; } // null; see note above
        internal object? NullPipe          { get; set; } = false;
        internal object? PipelineProcessor { get; set; }
        internal object? ExternalWriter    { get; set; }
    }

    private class FakePipe_NoDownstreamCmdlet
    {
        //absent object? DownstreamCmdlet  { get; set; } = new FakeCommandProcessor();
        internal object? NullPipe          { get; set; } = false;
        internal object? PipelineProcessor { get; set; }
        internal object? ExternalWriter    { get; set; }
    }

    private class FakePipe_NoNullPipe
    {
        internal object? DownstreamCmdlet  { get; set; } = new FakeCommandProcessor();
        //absent object? NullPipe          { get; set; } = false;
        internal object? PipelineProcessor { get; set; }
        internal object? ExternalWriter    { get; set; }
    }

    private class FakePipe_NoPipelineProcessor
    {
        internal object? DownstreamCmdlet  { get; set; } = new FakeCommandProcessor();
        internal object? NullPipe          { get; set; } = false;
        //absent object? PipelineProcessor { get; set; }
        internal object? ExternalWriter    { get; set; }
    }

    private class FakePipe_NoExternalWriter
    {
        internal object? DownstreamCmdlet  { get; set; } = new FakeCommandProcessor();
        internal object? NullPipe          { get; set; } = false;
        internal object? PipelineProcessor { get; set; }
        //absent object? ExternalWriter    { get; set; }
    }

    private class FakePipe_ReadOnlyExternalWriter
    {
        internal object? DownstreamCmdlet  { get; set; } = new FakeCommandProcessor();
        internal object? NullPipe          { get; set; } = false;
        internal object? PipelineProcessor { get; set; }
        internal object? ExternalWriter    { get; /**/ } // read-only
    }

    private class FakePipe_NoDefaultConstructor : FakePipe
    {
        public FakePipe_NoDefaultConstructor(object? _)
        {
            DownstreamCmdlet = new FakeCommandProcessor();
        }
    }

    private class FakeCommandProcessor
    {
        internal object? Command { get; set; } = new OutDefaultCommand();
    }

    private enum MergeTo { None, Something }

    private class FakeRuntimeBase : ICommandRuntime
    {
        public FakeRuntimeBase()
        {
            CurrentOutputPipe      = CreateDefaultPipe();
            CurrentErrorOutputPipe = CreateDefaultPipe();
        }

        private object? OriginalOutputPipe      { get; set; }
        private object? OriginalErrorOutputPipe { get; set; }

        private object? CurrentOutputPipe
        {
            get => this.GetPropertyValue("OutputPipe");
            set => this.SetPropertyValue("OutputPipe", value);
        }

        private object? CurrentErrorOutputPipe
        {
            get => this.GetPropertyValue("ErrorOutputPipe");
            set => this.SetPropertyValue("ErrorOutputPipe", value);
        }

        private static FakePipe CreateDefaultPipe()
        {
            return new()
            {
                DownstreamCmdlet = new FakeCommandProcessor()
            };
        }

        public void CaptureOriginalValues()
        {
            OriginalOutputPipe      = CurrentOutputPipe;
            OriginalErrorOutputPipe = CurrentErrorOutputPipe;
        }

        public void AssertFixups(bool output, bool errors)
        {
            var outputPipe      = CurrentOutputPipe;
            var errorOutputPipe = CurrentErrorOutputPipe;

            AssertInvariants();
            AssertFixup(outputPipe,      OriginalOutputPipe,      output, "OutputPipe");
            AssertFixup(errorOutputPipe, OriginalErrorOutputPipe, errors, "ErrorOutputPipe");

            if (output && errors)
                outputPipe.Should().BeSameAs(errorOutputPipe);
        }

        private void AssertInvariants()
        {
            if (OriginalErrorOutputPipe is FakePipe originalErrorOutputPipe)
                originalErrorOutputPipe.ExternalWriter.Should().BeNull();

            if (OriginalOutputPipe is FakePipe originalOutputPipe)
                originalOutputPipe.ExternalWriter.Should().BeNull();
        }

        private void AssertFixup(object? currentPipe, object? originalPipe, bool expected, string s)
        {
            var because = $"{s} {(expected ? "should" : "should not")} be fixed up";

            if (expected)
                currentPipe.Should().NotBeSameAs(originalPipe, because)
                    .And.BeOfType<FakePipe>(because)
                    .Which.ExternalWriter.Should().BeOfType<OutDefaultWriter>(because);
            else
                currentPipe.Should().BeSameAs(originalPipe, because);
        }

        #region ICommandRuntime stub

        public PSTransactionContext CurrentPSTransaction
            => throw new NotImplementedException();

        public PSHost Host
            => throw new NotImplementedException();

        public bool ShouldContinue(string query, string caption)
            => throw new NotImplementedException();

        public bool ShouldContinue(string query, string caption, ref bool yesToAll, ref bool noToAll)
            => throw new NotImplementedException();

        public bool ShouldProcess(string target)
            => throw new NotImplementedException();

        public bool ShouldProcess(string target, string action)
            => throw new NotImplementedException();

        public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption)
            => throw new NotImplementedException();

        public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption, out ShouldProcessReason shouldProcessReason)
            => throw new NotImplementedException();

        public void ThrowTerminatingError(ErrorRecord errorRecord)
            => throw new NotImplementedException();

        public bool TransactionAvailable()
            => throw new NotImplementedException();

        public void WriteCommandDetail(string text)
            => throw new NotImplementedException();

        public void WriteDebug(string text)
            => throw new NotImplementedException();

        public void WriteError(ErrorRecord errorRecord)
            => throw new NotImplementedException();

        public void WriteObject(object sendToPipeline)
            => throw new NotImplementedException();

        public void WriteObject(object sendToPipeline, bool enumerateCollection)
            => throw new NotImplementedException();

        public void WriteProgress(long sourceId, ProgressRecord progressRecord)
            => throw new NotImplementedException();

        public void WriteProgress(ProgressRecord progressRecord)
            => throw new NotImplementedException();

        public void WriteVerbose(string text)
            => throw new NotImplementedException();

        public void WriteWarning(string text)
            => throw new NotImplementedException();

        #endregion
    }
}
