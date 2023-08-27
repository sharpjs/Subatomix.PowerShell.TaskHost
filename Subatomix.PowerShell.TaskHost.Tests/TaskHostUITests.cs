// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Security;

namespace Subatomix.PowerShell.TaskHost;

[TestFixture]
[TestFixtureSource(nameof(Bools))]
public class TaskHostUITests : TestHarnessBase
{
    private const ConsoleColor
        TimeFg = ConsoleColor.DarkGray,
        TaskFg = ConsoleColor.DarkBlue,
        MoreFg = ConsoleColor.DarkGray,
        Bg     = ConsoleColor.Black;

    public static bool[] Bools = { false, true };

    private TaskHostUI                   UI          { get; }
    private Mock<PSHostUserInterface>    InnerUI     { get; }
    private Mock<PSHostRawUserInterface> InnerRawUI  { get; }
    private TaskInfo                     Task1       { get; }
    private TaskInfo                     Task2       { get; }
    private bool                         WithElapsed { get; }

    public TaskHostUITests(bool withElapsed)
    {
        InnerUI    = Mocks.Create<PSHostUserInterface>();
        InnerRawUI = Mocks.Create<PSHostRawUserInterface>();

        InnerUI.Setup(u => u.RawUI).Returns(InnerRawUI.Object);

        UI = new(InnerUI.Object, withElapsed ? Stopwatch.StartNew() : null);

        Task1 = new("1"); Task1.Retain();
        Task2 = new("2"); Task2.Retain();

        WithElapsed = withElapsed;
    }

    protected override void CleanUp(bool managed)
    {
        Task1.Release();
        Task2.Release();

        base.CleanUp(managed);
    }

    [Test]
    public void Construct_NullUi()
    {
        Invoking(() => new TaskHostUI(null!, null))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void RawUI_Get()
    {
        UI.RawUI.Should().BeOfType<TaskHostRawUI>();
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SupportsVirtualTerminal_Get(bool value)
    {
        InnerUI.Setup(u => u.SupportsVirtualTerminal)
            .Returns(value)
            .Verifiable();

        UI.SupportsVirtualTerminal.Should().Be(value);
    }

    [Test]
    public void Write()
    {
        var s = new MockSequence();

        ExpectWriteElapsed     (s);
        ExpectWriteTask1Header (s);
        ExpectWrite            (s, "a");
        ExpectWrite            (s, null);
        ExpectWrite            (s, "\n");

        ExpectWriteElapsed     (s);
        ExpectWriteTask1Header (s);
        ExpectWrite            (s, null);
        ExpectWrite            (s, "b");
        ExpectWriteLine        (s);

        ExpectWriteElapsed     (s);
        ExpectWriteTask2Header (s);
        ExpectWrite            (s, "c");
        ExpectWriteLine        (s);

        ExpectWriteElapsed     (s);
        ExpectWriteTask1Header (s);
        ExpectWriteEllipsis    (s);
        ExpectWrite            (s, "d");
        ExpectWrite            (s, "\n");

        ExpectWriteElapsed     (s);
        ExpectWriteTask2Header (s);
        ExpectWriteEllipsis    (s);
        ExpectWrite            (s, "e");
        ExpectWriteLine        (s);

        ExpectWriteElapsed     (s);
        ExpectWrite            (s, "f");

        using (new TaskScope(Task1))
        {
            UI.Write("a");
            UI.Write(null);
            UI.Write("\n");
            UI.Write(null);
            UI.Write("b");
        }

        using (new TaskScope(Task2))
        {
            UI.Write("c");
        }

        using (new TaskScope(Task1))
        {
            UI.Write("d");
            UI.Write("\n");
        }

        using (new TaskScope(Task2))
        {
            UI.Write("e");
        }

        UI.Write("f");
    }

    [Test]
    public void Write_EmptyHeader()
    {
        Task1.Name = "";
        Task2.Name = "";

        var s = new MockSequence();

        ExpectWriteElapsed     (s);
        ExpectWrite            (s, "a");
        ExpectWrite            (s, null);
        ExpectWrite            (s, "\n");

        ExpectWriteElapsed     (s);
        ExpectWrite            (s, null);
        ExpectWrite            (s, "b");
        ExpectWriteLine        (s);

        ExpectWriteElapsed     (s);
        ExpectWrite            (s, "c");
        ExpectWriteLine        (s);

        ExpectWriteElapsed     (s);
        ExpectWriteEllipsis    (s);
        ExpectWrite            (s, "d");
        ExpectWrite            (s, "\n");

        ExpectWriteElapsed     (s);
        ExpectWriteEllipsis    (s);
        ExpectWrite            (s, "e");
        ExpectWriteLine        (s);

        using (new TaskScope(Task1))
        {
            UI.Write("a");
            UI.Write(null);
            UI.Write("\n");
            UI.Write(null);
            UI.Write("b");
        }

        using (new TaskScope(Task2))
        {
            UI.Write("c");
        }

        using (new TaskScope(Task1))
        {
            UI.Write("d");
            UI.Write("\n");
        }

        using (new TaskScope(Task2))
        {
            UI.Write("e");
        }
    }

    [Test]
    public void Write_WithColors([Random(1)] ConsoleColor fg, [Random(1)] ConsoleColor bg)
    {
        var s = new MockSequence();

        ExpectWriteElapsed     (s);
        ExpectWriteTask1Header (s);
        ExpectWrite            (s, fg, bg, "a");
        ExpectWrite            (s, fg, bg, null);
        ExpectWrite            (s, fg, bg, "\n");

        ExpectWriteElapsed     (s);
        ExpectWriteTask1Header (s);
        ExpectWrite            (s, fg, bg, null);
        ExpectWrite            (s, fg, bg, "b");
        ExpectWriteLine        (s);

        ExpectWriteElapsed     (s);
        ExpectWriteTask2Header (s);
        ExpectWrite            (s, fg, bg, "c");
        ExpectWriteLine        (s);

        ExpectWriteElapsed     (s);
        ExpectWriteTask1Header (s);
        ExpectWriteEllipsis    (s);
        ExpectWrite            (s, fg, bg, "d");
        ExpectWrite            (s, fg, bg, "\n");

        ExpectWriteElapsed     (s);
        ExpectWriteTask2Header (s);
        ExpectWriteEllipsis    (s);
        ExpectWrite            (s, fg, bg, "e");
        ExpectWriteLine        (s);

        using (new TaskScope(Task1))
        {
            UI.Write(fg, bg, "a");
            UI.Write(fg, bg, null);
            UI.Write(fg, bg, "\n");
            UI.Write(fg, bg, null);
            UI.Write(fg, bg, "b");
        }

        using (new TaskScope(Task2))
        {
            UI.Write(fg, bg, "c");
        }

        using (new TaskScope(Task1))
        {
            UI.Write(fg, bg, "d");
            UI.Write(fg, bg, "\n");
        }

        using (new TaskScope(Task2))
        {
            UI.Write(fg, bg, "e");
        }
    }

    [Test]
    public void WriteLine0()
    {
        var s = new MockSequence();

        ExpectWriteElapsed     (s);
        ExpectWriteTask1Header (s);
        ExpectWriteLine        (s);

        ExpectWriteElapsed     (s);
        ExpectWriteTask1Header (s);
        ExpectWrite            (s, "b");
        ExpectWriteLine        (s);

        ExpectWriteElapsed     (s);
        ExpectWriteTask1Header (s);
        ExpectWrite            (s, "c");
        ExpectWriteLine        (s);

        ExpectWriteElapsed     (s);
        ExpectWriteTask2Header (s);
        ExpectWriteLine        (s);

        using (new TaskScope(Task1))
        {
            UI.WriteLine();
            UI.Write    ("b");
            UI.WriteLine();
            UI.Write    ("c");
        }

        using (new TaskScope(Task2))
        {
            UI.WriteLine();
        }
    }

    [Test]
    public void WriteLine1()
    {
        var s = new MockSequence();

        ExpectWriteElapsed     (s);
        ExpectWriteTask1Header (s);
        ExpectWriteLine        (s, "a");

        ExpectWriteElapsed     (s);
        ExpectWriteTask1Header (s);
        ExpectWriteLine        (s, null);

        ExpectWriteElapsed     (s);
        ExpectWriteTask1Header (s);
        ExpectWrite            (s, "b");
        ExpectWriteLine        (s, "c");

        ExpectWriteElapsed     (s);
        ExpectWriteTask1Header (s);
        ExpectWrite            (s, "d");
        ExpectWriteLine        (s);

        ExpectWriteElapsed     (s);
        ExpectWriteTask2Header (s);
        ExpectWriteLine        (s, "e");

        using (new TaskScope(Task1))
        {
            UI.WriteLine("a");
            UI.WriteLine(null);
            UI.Write    ("b");
            UI.WriteLine("c");
            UI.Write    ("d");
        }

        using (new TaskScope(Task2))
        {
            UI.WriteLine("e");
        }
    }

    [Test]
    public void WriteLine1_WithColors([Random(1)] ConsoleColor fg, [Random(1)] ConsoleColor bg)
    {
        var s = new MockSequence();

        ExpectWriteElapsed     (s);
        ExpectWriteTask1Header (s);
        ExpectWriteLine        (s, fg, bg, "a");

        ExpectWriteElapsed     (s);
        ExpectWriteTask1Header (s);
        ExpectWriteLine        (s, fg, bg, null);

        ExpectWriteElapsed     (s);
        ExpectWriteTask1Header (s);
        ExpectWrite            (s, fg, bg, "b");
        ExpectWriteLine        (s, fg, bg, "c");

        ExpectWriteElapsed     (s);
        ExpectWriteTask1Header (s);
        ExpectWrite            (s, fg, bg, "d");
        ExpectWriteLine        (s);

        ExpectWriteElapsed     (s);
        ExpectWriteTask2Header (s);
        ExpectWriteLine        (s, fg, bg, "e");

        using (new TaskScope(Task1))
        {
            UI.WriteLine(fg, bg, "a");
            UI.WriteLine(fg, bg, null);
            UI.Write    (fg, bg, "b");
            UI.WriteLine(fg, bg, "c");
            UI.Write    (fg, bg, "d");
        }

        using (new TaskScope(Task2))
        {
            UI.WriteLine(fg, bg, "e");
        }
    }

    [Test]
    public void WriteDebugLine()
    {
        var s = new MockSequence();

        ExpectWriteElapsed     (s);
        ExpectWriteTask1Header (s);
        ExpectWriteDebugLine   (s, "a");

        ExpectWriteElapsed     (s);
        ExpectWriteTask1Header (s);
        ExpectWriteDebugLine   (s, null);

        ExpectWriteElapsed     (s);
        ExpectWriteTask1Header (s);
        ExpectWrite            (s, "b");
        ExpectWriteDebugLine   (s, "c");

        ExpectWriteElapsed     (s);
        ExpectWriteTask1Header (s);
        ExpectWrite            (s, "d");
        ExpectWriteLine        (s);

        ExpectWriteElapsed     (s);
        ExpectWriteTask2Header (s);
        ExpectWriteDebugLine   (s, "e");

        using (new TaskScope(Task1))
        {
            UI.WriteDebugLine("a");
            UI.WriteDebugLine(null);
            UI.Write         ("b");
            UI.WriteDebugLine("c");
            UI.Write         ("d");
        }

        using (new TaskScope(Task2))
        {
            UI.WriteDebugLine("e");
        }
    }

    [Test]
    public void WriteVerboseLine()
    {
        var s = new MockSequence();

        ExpectWriteElapsed     (s);
        ExpectWriteTask1Header (s);
        ExpectWriteVerboseLine (s, "a");

        ExpectWriteElapsed     (s);
        ExpectWriteTask1Header (s);
        ExpectWriteVerboseLine (s, null);

        ExpectWriteElapsed     (s);
        ExpectWriteTask1Header (s);
        ExpectWrite            (s, "b");
        ExpectWriteVerboseLine (s, "c");

        ExpectWriteElapsed     (s);
        ExpectWriteTask1Header (s);
        ExpectWrite            (s, "d");
        ExpectWriteLine        (s);

        ExpectWriteElapsed     (s);
        ExpectWriteTask2Header (s);
        ExpectWriteVerboseLine (s, "e");

        using (new TaskScope(Task1))
        {
            UI.WriteVerboseLine("a");
            UI.WriteVerboseLine(null);
            UI.Write           ("b");
            UI.WriteVerboseLine("c");
            UI.Write           ("d");
        }

        using (new TaskScope(Task2))
        {
            UI.WriteVerboseLine("e");
        }
    }

    [Test]
    public void WriteWarningLine()
    {
        var s = new MockSequence();

        ExpectWriteElapsed     (s);
        ExpectWriteTask1Header (s);
        ExpectWriteWarningLine (s, "a");

        ExpectWriteElapsed     (s);
        ExpectWriteTask1Header (s);
        ExpectWriteWarningLine (s, null);

        ExpectWriteElapsed     (s);
        ExpectWriteTask1Header (s);
        ExpectWrite            (s, "b");
        ExpectWriteWarningLine (s, "c");

        ExpectWriteElapsed     (s);
        ExpectWriteTask1Header (s);
        ExpectWrite            (s, "d");
        ExpectWriteLine        (s);

        ExpectWriteElapsed     (s);
        ExpectWriteTask2Header (s);
        ExpectWriteWarningLine (s, "e");

        using (new TaskScope(Task1))
        {
            UI.WriteWarningLine("a");
            UI.WriteWarningLine(null);
            UI.Write           ("b");
            UI.WriteWarningLine("c");
            UI.Write           ("d");
        }

        using (new TaskScope(Task2))
        {
            UI.WriteWarningLine("e");
        }
    }

    [Test]
    public void WriteErrorLine()
    {
        var s = new MockSequence();

        ExpectWriteElapsed     (s);
        ExpectWriteTask1Header (s);
        ExpectWriteErrorLine   (s, "a");

        ExpectWriteElapsed     (s);
        ExpectWriteTask1Header (s);
        ExpectWriteErrorLine   (s, null);

        ExpectWriteElapsed     (s);
        ExpectWriteTask1Header (s);
        ExpectWrite            (s, "b");
        ExpectWriteErrorLine   (s, "c");

        ExpectWriteElapsed     (s);
        ExpectWriteTask1Header (s);
        ExpectWrite            (s, "d");
        ExpectWriteLine        (s);

        ExpectWriteElapsed     (s);
        ExpectWriteTask2Header (s);
        ExpectWriteErrorLine   (s, "e");

        using (new TaskScope(Task1))
        {
            UI.WriteErrorLine("a");
            UI.WriteErrorLine(null);
            UI.Write         ("b");
            UI.WriteErrorLine("c");
            UI.Write         ("d");
        }

        using (new TaskScope(Task2))
        {
            UI.WriteErrorLine("e");
        }
    }

    [Test]
    public void WriteInformation()
    {
        var record = new InformationRecord("a", "b");

        InnerUI
            .Setup(u => u.WriteInformation(record))
            .Verifiable();

        UI.WriteInformation(record);
    }

    [Test]
    public void WriteProgress()
    {
        var record = new ProgressRecord(42, "a", "b");

        InnerUI
            .Setup(u => u.WriteProgress(123, record))
            .Verifiable();

        UI.WriteProgress(123, record);
    }

    [Test]
    public void ReadLine()
    {
        var text = "input";

        var s = new MockSequence();
        ExpectWriteElapsed     (s);
        ExpectWriteTask1Header (s);
        ExpectWrite            (s, "before");

        InnerUI
            .InSequence(s)
            .Setup(u => u.ReadLine())
            .Returns(text)
            .Verifiable();

        ExpectWriteElapsed     (s);
        ExpectWriteTask1Header (s);
        ExpectWrite            (s, "after");

        using var scope = new TaskScope(Task1);

        UI.Write("before");
        var result = UI.ReadLine();
        UI.Write("after");

        result.Should().BeSameAs(text);
    }

    [Test]
    public void ReadLineAsSecureString()
    {
        using var text = new SecureString();

        text.AppendChar('x');
        text.MakeReadOnly();

        var s = new MockSequence();

        ExpectWriteElapsed     (s);
        ExpectWriteTask1Header (s);
        ExpectWrite            (s, "before");

        InnerUI
            .InSequence(s)
            .Setup(u => u.ReadLineAsSecureString())
            .Returns(text)
            .Verifiable();

        ExpectWriteElapsed     (s);
        ExpectWriteTask1Header (s);
        ExpectWrite            (s, "after");

        using var scope = new TaskScope(Task1);

        UI.Write("before");
        var result = UI.ReadLineAsSecureString();
        UI.Write("after");

        result.Should().BeSameAs(text);
    }

    [Test]
    public void Prompt()
    {
        var fields = new Collection<FieldDescription>();
        var values = new Dictionary<string, PSObject>();

        InnerUI
            .Setup(u => u.Prompt("a", "b", fields))
            .Returns(values)
            .Verifiable();

        UI.Prompt("a", "b", fields).Should().BeSameAs(values);
    }

    [Test]
    public void PromptForChoice()
    {
        var choices = new Collection<ChoiceDescription>();

        InnerUI
            .Setup(u => u.PromptForChoice("a", "b", choices, 4))
            .Returns(3)
            .Verifiable();

        var result = UI.PromptForChoice("a", "b", choices, 4);

        result.Should().Be(3);
    }

    [Test]
    public void PromptForCredential()
    {
        using var password = new SecureString();

        password.AppendChar('y');
        password.MakeReadOnly();

        var credential = new PSCredential("username", password);

        InnerUI
            .Setup(u => u.PromptForCredential("a", "b", "c", "d"))
            .Returns(credential)
            .Verifiable();

        var result = UI.PromptForCredential("a", "b", "c", "d");

        result.Should().BeSameAs(credential);
    }

    [Test]
    public void PromptForCredential_WithTypesAndOptions()
    {
        using var password = new SecureString();

        password.AppendChar('y');
        password.MakeReadOnly();

        var credential = new PSCredential("username", password);
        var types      = PSCredentialTypes.Generic;
        var options    = PSCredentialUIOptions.ReadOnlyUserName;

        InnerUI
            .Setup(u => u.PromptForCredential("a", "b", "c", "d", types, options))
            .Returns(credential)
            .Verifiable();

        var result = UI.PromptForCredential("a", "b", "c", "d", types, options);

        result.Should().BeSameAs(credential);
    }

    private void ExpectWriteElapsed(MockSequence s)
    {
        const string
            StampRe = @"^\[\+[0-9]{2}:[0-9]{2}:[0-9]{2}\] $";

        if (!WithElapsed)
            return;

        InnerUI.InSequence(s).Setup(u => u.Write(TimeFg, Bg, It.IsRegex(StampRe))).Verifiable();
    }

    private void ExpectWriteTask1Header(MockSequence s)
    {
        ExpectWriteTaskHeader(s, "[1]: ");
    }

    private void ExpectWriteTask2Header(MockSequence s)
    {
        ExpectWriteTaskHeader(s, "[2]: ");
    }

    private void ExpectWriteTaskHeader(MockSequence s, string text)
    {
        InnerUI.InSequence(s).Setup(u => u.Write(TaskFg, Bg, text)).Verifiable();
    }

    private void ExpectWriteEllipsis(MockSequence s)
    {
        InnerUI.InSequence(s).Setup(u => u.Write(MoreFg, Bg, "(...) ")).Verifiable();
    }

    private void ExpectWrite(MockSequence s, string? text)
    {
        InnerUI.InSequence(s).Setup(u => u.Write(text)).Verifiable();
    }

    private void ExpectWrite(MockSequence s, ConsoleColor fg, ConsoleColor bg, string? text)
    {
        InnerUI.InSequence(s).Setup(u => u.Write(fg, bg, text)).Verifiable();
    }

    private void ExpectWriteLine(MockSequence s)
    {
        InnerUI.InSequence(s).Setup(u => u.WriteLine()).Verifiable();
    }

    private void ExpectWriteLine(MockSequence s, string? text)
    {
        InnerUI.InSequence(s).Setup(u => u.WriteLine(text)).Verifiable();
    }

    private void ExpectWriteLine(MockSequence s, ConsoleColor fg, ConsoleColor bg, string? text)
    {
        InnerUI.InSequence(s).Setup(u => u.WriteLine(fg, bg, text)).Verifiable();
    }

    private void ExpectWriteDebugLine(MockSequence s, string? text)
    {
        InnerUI.InSequence(s).Setup(u => u.WriteDebugLine(text)).Verifiable();
    }

    private void ExpectWriteVerboseLine(MockSequence s, string? text)
    {
        InnerUI.InSequence(s).Setup(u => u.WriteVerboseLine(text)).Verifiable();
    }

    private void ExpectWriteWarningLine(MockSequence s, string? text)
    {
        InnerUI.InSequence(s).Setup(u => u.WriteWarningLine(text)).Verifiable();
    }

    private void ExpectWriteErrorLine(MockSequence s, string? text)
    {
        InnerUI.InSequence(s).Setup(u => u.WriteErrorLine(text)).Verifiable();
    }
}
