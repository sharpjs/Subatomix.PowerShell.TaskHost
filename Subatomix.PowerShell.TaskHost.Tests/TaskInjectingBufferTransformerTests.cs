// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Collections.ObjectModel;
using System.Globalization;
using System.Security;

namespace Subatomix.PowerShell.TaskHost;

[TestFixture]
public class TaskInjectingBufferTransformerTests : TestHarnessBase
{
    [Test]
    public void Construct_NullHost()
    {
        Invoking(() => new TaskInjectingBufferTransformer(null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Construct_NullUI()
    {
        var host = new FakeHost(ui: null);

        using var _ = new TaskInjectingBufferTransformer(host);
    }

    [Test]
    public void Construct_UnrecognizedUI()
    {
        var host = new FakeHost(new FakeUI_NoBuffers());

        using var _ = new TaskInjectingBufferTransformer(host);
    }

    [Test]
    public void Construct_NullBuffers()
    {
        var host = new FakeHost(new FakeUI(buffers: null));

        using var _ = new TaskInjectingBufferTransformer(host);
    }

    [Test]
    public void Construct_UnrecognizedBuffers()
    {
        var host = new FakeHost(new FakeUI(buffers: new()));

        using var _ = new TaskInjectingBufferTransformer(host);
    }

    [Test]
    public void Construct_NullBuffer()
    {
        var buffers = new FakeBuffers(); // all buffers null
        var host    = new FakeHost(new FakeUI(buffers));

        using var _ = new TaskInjectingBufferTransformer(host);
    }

    [Test]
    public void Construct_UnrecognizedBuffer()
    {
        var buffers = new FakeBuffers
        {
            Warning     = new(),
            Information = new(),
            Debug       = new(),
            Verbose     = new(),
        };

        var host = new FakeHost(new FakeUI(buffers));

        using var _ = new TaskInjectingBufferTransformer(host);
    }

    [Test]
    public void Transform_Warning()
    {
        Test_InformationalRecord<WarningRecord>(buffer => new() { Warning = buffer });
    }

    [Test]
    public void Transform_Verbose()
    {
        Test_InformationalRecord<VerboseRecord>(buffer => new() { Verbose = buffer });
    }

    [Test]
    public void Transform_Debug()
    {
        Test_InformationalRecord<DebugRecord>(buffer => new() { Debug = buffer });
    }

    private static void Test_InformationalRecord<T>(Func<object, FakeBuffers> setup)
        where T : InformationalRecord
    {
        var buffer = new PSDataCollection<T>();
        var host   = new FakeHost(new FakeUI(buffers: setup(buffer)));

        using var scope = TaskScope.Begin(TestContext.CurrentContext.Test.FullName);

        var originalRecord    = (T) Activator.CreateInstance(typeof(T), "a")!;
        var transformedRecord = (T) Activator.CreateInstance(typeof(T), scope.Task.EncodedId + "a")!;
        var unaffectedRecord  = (T) Activator.CreateInstance(typeof(T), "b")!;

        using (new TaskInjectingBufferTransformer(host))
        using (new TaskInjectingBufferTransformer(host)) // To verify that tranformer avoids nesting
            buffer.Add(originalRecord);

        buffer.Add(unaffectedRecord);

        buffer.ReadAll().Should().SatisfyRespectively(
            r => r.Should().BeEquivalentTo(transformedRecord),
            r => r.Should().BeEquivalentTo(unaffectedRecord)
        );
    }

    [Test]
    public void Transform_Information()
    {
        var buffer  = new PSDataCollection<InformationRecord>();
        var buffers = new FakeBuffers { Information = buffer };
        var host    = new FakeHost(new FakeUI(buffers));

        using var scope = TaskScope.Begin(TestContext.CurrentContext.Test.FullName);

        var originalRecord    = new InformationRecord("a", "x");
        var transformedRecord = new InformationRecord("a", scope.Task.EncodedId + "x");
        var unaffectedRecord  = new InformationRecord("b", "y");

        using (new TaskInjectingBufferTransformer(host))
        using (new TaskInjectingBufferTransformer(host)) // To verify that tranformer avoids nesting
            buffer.Add(originalRecord);

        buffer.Add(unaffectedRecord);

        buffer.ReadAll().Should().SatisfyRespectively(
            r => r.Should().BeEquivalentTo(transformedRecord, _ => _.Excluding(x => x.TimeGenerated)),
            r => r.Should().BeEquivalentTo(unaffectedRecord)
        );
    }

    private class FakeHost : FakeHostBase
    {
        private readonly PSHostUserInterface? _ui;

        public FakeHost(PSHostUserInterface? ui = null) => _ui = ui;

        public override PSHostUserInterface? UI => _ui;
    }

    private class FakeUI_NoBuffers : FakeUIBase
    {
        // no changes
    }

    private class FakeUI : FakeUIBase
    {
        private readonly object? _buffers;

        public FakeUI(object? buffers = null) => _buffers = buffers;

        internal object? GetInformationalMessageBuffers() => _buffers;
    }

    private class FakeBuffers
    {
        public object? Warning     { get; set; }
        public object? Information { get; set; }
        public object? Verbose     { get; set; }
        public object? Debug       { get; set; }
    }

    #region PSHost stubs

    private abstract class FakeHostBase : PSHost
    {
        public override PSHostUserInterface? UI               => throw new NotImplementedException();
        public override CultureInfo          CurrentCulture   => throw new NotImplementedException();
        public override CultureInfo          CurrentUICulture => throw new NotImplementedException();
        public override Guid                 InstanceId       => throw new NotImplementedException();
        public override string               Name             => throw new NotImplementedException();
        public override Version              Version          => throw new NotImplementedException();

        public override void EnterNestedPrompt()         => throw new NotImplementedException();
        public override void ExitNestedPrompt()          => throw new NotImplementedException();
        public override void NotifyBeginApplication()    => throw new NotImplementedException();
        public override void NotifyEndApplication()      => throw new NotImplementedException();
        public override void SetShouldExit(int exitCode) => throw new NotImplementedException();
    }

    private abstract class FakeUIBase : PSHostUserInterface
    {
        public override PSHostRawUserInterface RawUI
            => throw new NotImplementedException();

        public override void Write(string value)
            => throw new NotImplementedException();

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
            => throw new NotImplementedException();

        public override void WriteLine(string value)
            => throw new NotImplementedException();

        public override void WriteErrorLine(string value)
            => throw new NotImplementedException();

        public override void WriteWarningLine(string message)
            => throw new NotImplementedException();

        public override void WriteVerboseLine(string message)
            => throw new NotImplementedException();

        public override void WriteDebugLine(string message)
            => throw new NotImplementedException();

        public override void WriteProgress(long sourceId, ProgressRecord record)
            => throw new NotImplementedException();

        public override string ReadLine()
            => throw new NotImplementedException();

        public override SecureString ReadLineAsSecureString()
            => throw new NotImplementedException();

        public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
            => throw new NotImplementedException();

        public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
            => throw new NotImplementedException();

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
            => throw new NotImplementedException();

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
            => throw new NotImplementedException();
    }

    #endregion
}
