// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

[TestFixture]
public class ErrorFlowFixupTests : TestHarnessBase
{
    public ErrorFlowFixupTests()
    {
        Cmdlet = Mocks.Create<Cmdlet>().Object;
        Host   = Mocks.Create<PSHost>().Object;
    }

    public Cmdlet Cmdlet { get; }
    public PSHost Host   { get; }

    [Test]
    public void Configure_NullCmdlet()
    {
        Invoking(() => ErrorFlowFixup.Configure(null!, Host))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Configure_NullHost()
    {
        Invoking(() => ErrorFlowFixup.Configure(Cmdlet, null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Configure_NullCommandRuntime()
    {
        TestConfigure(runtime: null);
    }

    [Test]
    public void Configure_ErrorsMerged() // 2>&1
    {
        var pipe    = new FakePipe { };
        var runtime = new FakeRuntime { ErrorOutputPipe = pipe, ErrorMergeTo = MergeTo.Something };

        TestConfigure(runtime);

        runtime.ErrorOutputPipe.Should().BeSameAs(pipe);
        pipe   .ExternalWriter .Should().BeNull();
    }

    [Test]
    public void Configure_ErrorsRedirectedToNull() // 2> $null
    {
        var pipe    = new FakePipe { NullPipe = true };
        var runtime = new FakeRuntime { ErrorOutputPipe = pipe };

        TestConfigure(runtime);

        runtime.ErrorOutputPipe.Should().BeSameAs(pipe);
        pipe   .ExternalWriter .Should().BeNull();
    }

    [Test]
    public void Configure_ErrorsRedirectedToFile() // 2> file
    {
        var pipe    = new FakePipe { PipelineProcessor = new() };
        var runtime = new FakeRuntime { ErrorOutputPipe = pipe };

        TestConfigure(runtime);

        runtime.ErrorOutputPipe.Should().BeSameAs(pipe);
        pipe   .ExternalWriter .Should().BeNull();
    }

    [Test]
    public void Configure() // happy path
    {
        var pipe    = new FakePipe { };
        var runtime = new FakeRuntime { ErrorOutputPipe = pipe };

        TestConfigure(runtime);

        runtime.ErrorOutputPipe
            .Should().NotBeSameAs(pipe)
            .And.BeOfType<FakePipe>()
            .Which.ExternalWriter.Should().BeOfType<HostWriter>();

        pipe.ExternalWriter.Should().BeNull();
    }

    [Test]
    public void Configure_ReflectionFail_ErrorMergeTo_Missing()
    {
        var pipe    = new FakePipe { };
        var runtime = new FakeRuntime_NoErrorMergeTo { ErrorOutputPipe = pipe };

        TestConfigure(runtime);

        runtime.ErrorOutputPipe.Should().BeSameAs(pipe);
        pipe   .ExternalWriter .Should().BeNull();
    }

    [Test]
    public void Configure_ReflectionFail_ErrorMergeTo_Null()
    {
        var pipe    = new FakePipe { };
        var runtime = new FakeRuntime { ErrorOutputPipe = pipe, ErrorMergeTo = null };

        TestConfigure(runtime);

        runtime.ErrorOutputPipe.Should().BeSameAs(pipe);
        pipe   .ExternalWriter .Should().BeNull();
    }

    [Test]
    public void Configure_ReflectionFail_ErrorMergeTo_NotEnum()
    {
        var pipe    = new FakePipe { };
        var runtime = new FakeRuntime { ErrorOutputPipe = pipe, ErrorMergeTo = new object() };

        TestConfigure(runtime);

        runtime.ErrorOutputPipe.Should().BeSameAs(pipe);
        pipe   .ExternalWriter .Should().BeNull();
    }

    [Test]
    public void Configure_ReflectionFail_ErrorOutputPipe_Missing()
    {
        var runtime = new FakeRuntime_NoErrorOutputPipe { };

        TestConfigure(runtime);
    }

    [Test]
    public void Configure_ReflectionFail_ErrorOutputPipe_Null()
    {
        var runtime = new FakeRuntime { };

        TestConfigure(runtime);
    }

    [Test]
    public void Configure_ReflectionFail_ErrorOutputPipe_NoDefaultConstructor()
    {
        var pipe    = new FakePipe_NoDefaultConstructor(null) { };
        var runtime = new FakeRuntime { ErrorOutputPipe = pipe };

        TestConfigure(runtime);

        runtime.ErrorOutputPipe.Should().BeSameAs(pipe);
        pipe   .ExternalWriter .Should().BeNull();
    }

    [Test]
    public void Configure_ReflectionFail_NullPipe_Missing()
    {
        var pipe    = new FakePipe_NoNullPipe { };
        var runtime = new FakeRuntime { ErrorOutputPipe = pipe };

        TestConfigure(runtime);

        runtime.ErrorOutputPipe.Should().BeSameAs(pipe);
        pipe   .ExternalWriter .Should().BeNull();
    }

    [Test]
    public void Configure_ReflectionFail_NullPipe_Null()
    {
        var pipe    = new FakePipe { NullPipe = null };
        var runtime = new FakeRuntime { ErrorOutputPipe = pipe };

        TestConfigure(runtime);

        runtime.ErrorOutputPipe.Should().BeSameAs(pipe);
        pipe   .ExternalWriter .Should().BeNull();
    }

    [Test]
    public void Configure_ReflectionFail_NullPipe_UnexpectedType()
    {
        var pipe    = new FakePipe { NullPipe = new object() };
        var runtime = new FakeRuntime { ErrorOutputPipe = pipe };

        TestConfigure(runtime);

        runtime.ErrorOutputPipe.Should().BeSameAs(pipe);
        pipe   .ExternalWriter .Should().BeNull();
    }

    [Test]
    public void Configure_ReflectionFail_PipelineProcessor_Missing()
    {
        var pipe    = new FakePipe_NoPipelineProcessor { };
        var runtime = new FakeRuntime { ErrorOutputPipe = pipe };

        TestConfigure(runtime);

        runtime.ErrorOutputPipe.Should().BeSameAs(pipe);
        pipe.ExternalWriter.Should().BeNull();
    }

    [Test]
    public void Configure_ReflectionFail_ExternalWriter_Missing()
    {
        var pipe    = new FakePipe_NoExternalWriter { };
        var runtime = new FakeRuntime { ErrorOutputPipe = pipe };

        TestConfigure(runtime);

        runtime.ErrorOutputPipe.Should().BeSameAs(pipe);
    }

    [Test]
    public void Configure_ReflectionFail_ExternalWriter_ReadOnly()
    {
        var pipe    = new FakePipe_ReadOnlyExternalWriter { };
        var runtime = new FakeRuntime { ErrorOutputPipe = pipe };

        TestConfigure(runtime);

        runtime.ErrorOutputPipe.Should().BeSameAs(pipe);
        pipe   .ExternalWriter .Should().BeNull();
    }

    private void TestConfigure(ICommandRuntime? runtime)
    {
        Cmdlet.CommandRuntime = runtime;
        ErrorFlowFixup.Configure(Cmdlet, Host);
    }

    private class FakeRuntime : FakeRuntimeBase
    {
        internal object? ErrorMergeTo    { get; init; } = MergeTo.None;
        internal object? ErrorOutputPipe { get; init; }
    }

    private class FakeRuntime_NoErrorMergeTo : FakeRuntimeBase
    {
        //absent MergeTo ErrorMergeTo    { get; init; }
        internal object? ErrorOutputPipe { get; init; }
    }

    private class FakeRuntime_NoErrorOutputPipe : FakeRuntimeBase
    {
        internal MergeTo ErrorMergeTo    { get; init; }
        //absent object? ErrorOutputPipe { get; init; }
    } 

    private class FakePipe
    {
        internal object? NullPipe          { get; init; } = false;
        internal object? PipelineProcessor { get; init; }
        internal object? ExternalWriter    { get; set;  }
    }

    private class FakePipe_NoDefaultConstructor : FakePipe
    {
        public FakePipe_NoDefaultConstructor(object? _) { }
    }

    private class FakePipe_NoNullPipe
    {
        //absent object? NullPipe          { get; init; } = false;
        internal object? PipelineProcessor { get; init; }
        internal object? ExternalWriter    { get; set;  }
    }

    private class FakePipe_NoPipelineProcessor
    {
        internal object? NullPipe          { get; init; } = false;
        //absent object? PipelineProcessor { get; init; }
        internal object? ExternalWriter    { get; set;  }
    }

    private class FakePipe_NoExternalWriter
    {
        internal object? NullPipe          { get; init; } = false;
        internal object? PipelineProcessor { get; init; }
        //absent object? ExternalWriter    { get; set;  }
    }

    private class FakePipe_ReadOnlyExternalWriter
    {
        internal object? NullPipe          { get; init; } = false;
        internal object? PipelineProcessor { get; init; }
        internal object? ExternalWriter    { get; /*!*/ }
    }

    private enum MergeTo { None, Something }

    private class FakeRuntimeBase : ICommandRuntime
    {
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
    }
}
