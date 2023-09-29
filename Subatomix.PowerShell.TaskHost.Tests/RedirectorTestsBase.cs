// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

using static TaskEncoding;

public abstract class RedirectorTestsBase : TestHarnessBase
{
    public const char
        L = StartDelimiter,
        R = EndDelimiter;

    private   Sma.PowerShell              Shell   { get; }
    protected PSDataCollection<PSObject?> Output  { get; }
    protected Mock<ICommandRuntime2>      Runtime { get; }
    protected Cmdlet                      Cmdlet  { get; }

    protected PSDataStreams Streams => Shell.Streams;

    public RedirectorTestsBase()
    {
        Shell   = Sma.PowerShell.Create();
        Output  = new();
        Runtime = Mocks.Create<ICommandRuntime2>();
        Cmdlet  = new TestCmdlet() { CommandRuntime = Runtime.Object };
    }

    protected override void CleanUp(bool managed)
    {
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
        var (input, output) = SetUpErrorRecordFlow();

        Runtime
            .Setup(r => r.WriteError(It.IsAny<ErrorRecord>()))
            .Callback(Assert)
            .Verifiable();

        Streams.Error.Add(input);

        void Assert(ErrorRecord record)
        {
            record.Should().BeEquivalentTo(output);
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
        var (input, output) = SetUpInformationRecordFlow();

        Runtime
            .Setup(r => r.WriteInformation(It.IsAny<InformationRecord>()))
            .Callback(Assert)
            .Verifiable();

        Streams.Information.Add(input);

        void Assert(InformationRecord record)
        {
            record.Should().BeEquivalentTo(output, _ => _.Excluding(x => x!.TimeGenerated));
            AssertStateInWrite();
        }
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

    protected virtual void AssertStateInWrite() { }

    private class TestCmdlet : Cmdlet { }
}
