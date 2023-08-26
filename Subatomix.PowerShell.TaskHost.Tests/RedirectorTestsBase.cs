// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

public abstract class RedirectorTestsBase : TestHarnessBase
{
    public const char
        L = '\x1', // left  delimiter of task id
        R = '\x2'; // right delimiter of task id

    private   Sma.PowerShell              Shell   { get; }
    protected PSDataCollection<PSObject?> Output  { get; }
    protected Mock<ICommandRuntime2>      Runtime { get; }
    protected Cmdlet                      Cmdlet  { get; }
    protected TaskInfo                    Task    { get; }

    protected PSDataStreams Streams => Shell.Streams;

    public RedirectorTestsBase()
    {
        Shell   = Sma.PowerShell.Create();
        Output  = new();
        Runtime = Mocks.Create<ICommandRuntime2>();
        Cmdlet  = new TestCmdlet() { CommandRuntime = Runtime.Object };

        Task = new TaskInfo(GetType().Name);
        Task.Retain();
    }

    protected override void CleanUp(bool managed)
    {
        TaskInfo.Current = null;

        while (Task.RetainCount > 0)
            Task.Release();

        Output.Dispose();
        Shell .Dispose();

        base.CleanUp(managed);
    }

    [Test]
    public void WriteOutput()
    {
        var (input, output) = SetUpObjectFlow();

        Runtime
            .Setup(r => r.WriteObject(It.IsAny<object?>()))
            .Callback(Assert)
            .Verifiable();

        Output.Add(input);

        void Assert(object? obj)
        {
            obj.Should().BeEquivalentTo(output);
            AssertStateInWrite();
        }
    }

    [Test]
    public void WriteError()
    {
        var (input, output) = SetUpTextFlow();

        var record = new ErrorRecord(new(), "id", ErrorCategory.InvalidOperation, "target")
        {
            ErrorDetails = new(input)
        };

        Runtime
            .Setup(r => r.WriteError(It.IsNotNull<ErrorRecord>()))
            .Callback(Assert)
            .Verifiable();

        Streams.Error.Add(record);

        void Assert(ErrorRecord r)
        {
            r.ErrorDetails.Message.Should().Be(output);
            AssertStateInWrite();
        }
    }

    [Test]
    public void WriteWarning()
    {
        var (input, output) = SetUpTextFlow();

        Runtime
            .Setup(r => r.WriteWarning(It.IsAny<string?>()))
            .Callback(Assert)
            .Verifiable();

        Streams.Warning.Add(new(input));

        void Assert(string? message)
        {
            message.Should().Be(output);
            AssertStateInWrite();
        }
    }

    [Test]
    public void WriteVerbose()
    {
        var (input, output) = SetUpTextFlow();

        Runtime
            .Setup(r => r.WriteVerbose(It.IsAny<string?>()))
            .Callback(Assert)
            .Verifiable();

        Streams.Verbose.Add(new(input));

        void Assert(string? message)
        {
            message.Should().Be(output);
            AssertStateInWrite();
        }
    }

    [Test]
    public void WriteDebug()
    {
        var (input, output) = SetUpTextFlow();

        Runtime
            .Setup(r => r.WriteDebug(It.IsAny<string?>()))
            .Callback(Assert)
            .Verifiable();

        Streams.Debug.Add(new(input));

        void Assert(string? message)
        {
            message.Should().Be(output);
            AssertStateInWrite();
        }
    }

    [Test]
    public void WriteInformation()
    {
        var (input, output) = SetUpTextFlow();

        var record = new InformationRecord("messageData", input);

        Runtime
            .Setup(r => r.WriteInformation(It.IsNotNull<InformationRecord>()))
            .Callback(Assert)
            .Verifiable();

        Streams.Information.Add(record);

        void Assert(InformationRecord record)
        {
            record.Source.Should().Be(output);
            AssertStateInWrite();
        }
    }

    protected virtual (PSObject? input, object? output) SetUpObjectFlow()
    {
        var obj = new PSObject();
        return (obj, obj);
    }

    protected virtual (string input, string output) SetUpTextFlow()
    {
        return ("text", "text");
    }

    protected virtual void AssertStateInWrite() { }

    private class TestCmdlet : Cmdlet { }
}
