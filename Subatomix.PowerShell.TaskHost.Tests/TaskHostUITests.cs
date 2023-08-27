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

#if TEMPORARILY_EXCLUDED_DURING_REWORK
    [Test]
    public void Header_Get_Default()
    {
        using var my = new TaskHostTestHarness();

        my.Factory.Create().UI
            .Should().BeOfType<TaskHostUI>().AssignTo(out var ui1);

        my.Factory.Create().UI
            .Should().BeOfType<TaskHostUI>().AssignTo(out var ui2);

        ui1.Header.Should().Be("Task 1");
        ui2.Header.Should().Be("Task 2");
    }

    [Test]
    public void Header_Get_Explicit()
    {
        using var my = new TaskHostTestHarness();

        var header = my.Random.GetString();

        my.Factory.Create(header).UI
            .Should().BeOfType<TaskHostUI>().AssignTo(out var UI);

        UI.Header.Should().BeSameAs(header);
    }

    [Test]
    public void Header_Set()
    {
        using var my = new TaskHostTestHarness();

        var header = my.Random.GetString();

        my.Factory.Create(header).UI
            .Should().BeOfType<TaskHostUI>().AssignTo(out var UI);

        UI.Header = header;
        UI.Header.Should().BeSameAs(header);
    }

    [Test]
    public void Header_Set_Null()
    {
        using var my = new TaskHostTestHarness();

        my.Factory.Create().UI
            .Should().BeOfType<TaskHostUI>().AssignTo(out var UI);

        UI.Invoking(u => u.Header = null!)
            .Should().ThrowExactly<ArgumentNullException>();
    }
#endif

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

#if TEMPORARILY_EXCLUDED_DURING_REWORK
    [Test]
    public void Write_EmptyHeader()
    {
        using var my = new TaskHostTestHarness();

        var ui1 = my.Factory.Create("1").TaskHostUI;
        var ui2 = my.Factory.Create("2").TaskHostUI;

        ui1.Header = "";
        ui2.Header = "";

        var s = new MockSequence();
        InnerUI.InSequence(s).Setup(u => u.Write    (            "a"        )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (             null      )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (             "\n"      )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (             null      )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (            "b"        )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteLine(                       )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (            "c"        )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteLine(                       )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (MoreFg, Bg, "(...) "   )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (                  "d"  )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (                   "\n")).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (MoreFg, Bg, "(...) "   )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (                  "e"  )).Verifiable();

        ui1.Write("a");
        ui1.Write(null);
        ui1.Write("\n");
        ui1.Write(null);
        ui1.Write("b");
        ui2.Write("c");
        ui1.Write("d");
        ui1.Write("\n");
        ui2.Write("e");
    }

    [Test]
    public void Write_EmptyHeader_WithElapsed()
    {
        using var my = new TaskHostTestHarness(withElapsed: true);

        var ui1 = my.Factory.Create("1").TaskHostUI;
        var ui2 = my.Factory.Create("2").TaskHostUI;

        ui1.Header = "";
        ui2.Header = "";

        var s = new MockSequence();
        InnerUI.InSequence(s).Setup(u => u.Write    (StampFg, Bg, IsRegex(StampRe))).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (             "a"             )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (              null           )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (              "\n"           )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (StampFg, Bg, IsRegex(StampRe))).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (             null            )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (             "b"             )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteLine(                             )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (StampFg, Bg, IsRegex(StampRe))).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (             "c"             )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteLine(                             )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (StampFg, Bg, IsRegex(StampRe))).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (MoreFg,  Bg, "(...) "        )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (                   "d"       )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (                    "\n"     )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (StampFg, Bg, IsRegex(StampRe))).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (MoreFg,  Bg, "(...) "        )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (                   "e"       )).Verifiable();

        ui1.Write("a");
        ui1.Write(null);
        ui1.Write("\n");
        ui1.Write(null);
        ui1.Write("b");
        ui2.Write("c");
        ui1.Write("d");
        ui1.Write("\n");
        ui2.Write("e");
    }

    [Test]
    public void Write_WithColors([Random(1)] ConsoleColor fg, [Random(1)] ConsoleColor bg)
    {
        using var my = new TaskHostTestHarness();

        var ui1 = my.Factory.Create("1").UI;
        var ui2 = my.Factory.Create("2").UI;

        var s = new MockSequence();
        InnerUI.InSequence(s).Setup(u => u.Write    (TaskFg, Bg, "[1]: "         )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (fg,       bg,      "a"        )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (fg,       bg,       null      )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (fg,       bg,       "\n"      )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (TaskFg, Bg, "[1]: "         )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (fg,       bg,      null       )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (fg,       bg,      "b"        )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteLine(                              )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (TaskFg, Bg, "[2]: "         )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (fg,       bg,      "c"        )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteLine(                              )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (TaskFg, Bg, "[1]: "         )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (MoreFg,   Bg,      "(...) "   )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (fg,       bg,            "d"  )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (fg,       bg,             "\n")).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (TaskFg, Bg, "[2]: "         )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (MoreFg,   Bg,      "(...) "   )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (fg,       bg,            "e"  )).Verifiable();

        ui1.Write(fg, bg, "a");
        ui1.Write(fg, bg, null);
        ui1.Write(fg, bg, "\n");
        ui1.Write(fg, bg, null);
        ui1.Write(fg, bg, "b");
        ui2.Write(fg, bg, "c");
        ui1.Write(fg, bg, "d");
        ui1.Write(fg, bg, "\n");
        ui2.Write(fg, bg, "e");
    }

    [Test]
    public void WriteLine_Empty()
    {
        using var my = new TaskHostTestHarness();

        var ui1 = my.Factory.Create("1").UI;
        var ui2 = my.Factory.Create("2").UI;

        var s = new MockSequence();
        InnerUI.InSequence(s).Setup(u => u.Write    (TaskFg, Bg, "[1]: " )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteLine(                      )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (TaskFg, Bg, "[1]: " )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (                   "b")).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteLine(                      )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (TaskFg, Bg, "[1]: " )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (                   "c")).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteLine(                      )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (TaskFg, Bg, "[2]: " )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteLine(                      )).Verifiable();

        ui1.WriteLine();
        ui1.Write    ("b");
        ui1.WriteLine();
        ui1.Write    ("c");
        ui2.WriteLine();
    }

    [Test]
    public void WriteLine()
    {
        using var my = new TaskHostTestHarness();

        var ui1 = my.Factory.Create("1").UI;
        var ui2 = my.Factory.Create("2").UI;

        var s = new MockSequence();
        InnerUI.InSequence(s).Setup(u => u.Write    (TaskFg, Bg, "[1]: "   )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteLine(                   "a"  )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (TaskFg, Bg, "[1]: "   )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteLine(                    null)).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (TaskFg, Bg, "[1]: "   )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (                   "b"  )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteLine(                    "c" )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (TaskFg, Bg, "[1]: "   )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (                   "d"  )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteLine(                        )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (TaskFg, Bg, "[2]: "   )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteLine(                   "e"  )).Verifiable();

        ui1.WriteLine("a");
        ui1.WriteLine(null);
        ui1.Write    ("b");
        ui1.WriteLine("c");
        ui1.Write    ("d");
        ui2.WriteLine("e");
    }

    [Test]
    public void WriteLine_WithColors([Random(1)] ConsoleColor fg, [Random(1)] ConsoleColor bg)
    {
        using var my = new TaskHostTestHarness();

        var ui1 = my.Factory.Create("1").UI;
        var ui2 = my.Factory.Create("2").UI;

        var s = new MockSequence();
        InnerUI.InSequence(s).Setup(u => u.Write    (TaskFg, Bg, "[1]: "   )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteLine(fg,       bg,      "a"  )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (TaskFg, Bg, "[1]: "   )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteLine(fg,       bg,       null)).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (TaskFg, Bg, "[1]: "   )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (fg,       bg,      "b"  )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteLine(fg,       bg,       "c" )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (TaskFg, Bg, "[1]: "   )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (fg,       bg,      "d"  )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteLine(                        )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write    (TaskFg, Bg, "[2]: "   )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteLine(fg,       bg,      "e"  )).Verifiable();

        ui1.WriteLine(fg, bg, "a");
        ui1.WriteLine(fg, bg, null);
        ui1.Write    (fg, bg, "b");
        ui1.WriteLine(fg, bg, "c");
        ui1.Write    (fg, bg, "d");
        ui2.WriteLine(fg, bg, "e");
    }

    [Test]
    public void WriteDebugLine()
    {
        using var my = new TaskHostTestHarness();

        var ui1 = my.Factory.Create("1").UI;
        var ui2 = my.Factory.Create("2").UI;

        var s = new MockSequence();
        InnerUI.InSequence(s).Setup(u => u.Write         (TaskFg, Bg, "[1]: "   )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteDebugLine(                   "a"  )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write         (TaskFg, Bg, "[1]: "   )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteDebugLine(                    null)).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write         (TaskFg, Bg, "[1]: "   )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write         (                   "b"  )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteDebugLine(                    "c" )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write         (TaskFg, Bg, "[1]: "   )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write         (                   "d"  )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteLine     (                        )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write         (TaskFg, Bg, "[2]: "   )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteDebugLine(                   "e"  )).Verifiable();

        ui1.WriteDebugLine("a");
        ui1.WriteDebugLine(null);
        ui1.Write         ("b");
        ui1.WriteDebugLine("c");
        ui1.Write         ("d");
        ui2.WriteDebugLine("e");
    }

    [Test]
    public void WriteVerboseLine()
    {
        using var my = new TaskHostTestHarness();

        var ui1 = my.Factory.Create("1").UI;
        var ui2 = my.Factory.Create("2").UI;

        var s = new MockSequence();
        InnerUI.InSequence(s).Setup(u => u.Write           (TaskFg, Bg, "[1]: "   )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteVerboseLine(                   "a"  )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write           (TaskFg, Bg, "[1]: "   )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteVerboseLine(                    null)).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write           (TaskFg, Bg, "[1]: "   )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write           (                   "b"  )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteVerboseLine(                    "c" )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write           (TaskFg, Bg, "[1]: "   )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write           (                   "d"  )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteLine       (                        )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write           (TaskFg, Bg, "[2]: "   )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteVerboseLine(                   "e"  )).Verifiable();

        ui1.WriteVerboseLine("a");
        ui1.WriteVerboseLine(null);
        ui1.Write           ("b");
        ui1.WriteVerboseLine("c");
        ui1.Write           ("d");
        ui2.WriteVerboseLine("e");
    }

    [Test]
    public void WriteWarningLine()
    {
        using var my = new TaskHostTestHarness();

        var ui1 = my.Factory.Create("1").UI;
        var ui2 = my.Factory.Create("2").UI;

        var s = new MockSequence();
        InnerUI.InSequence(s).Setup(u => u.Write           (TaskFg, Bg, "[1]: "   )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteWarningLine(                   "a"  )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write           (TaskFg, Bg, "[1]: "   )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteWarningLine(                    null)).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write           (TaskFg, Bg, "[1]: "   )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write           (                   "b"  )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteWarningLine(                    "c" )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write           (TaskFg, Bg, "[1]: "   )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write           (                   "d"  )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteLine       (                        )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write           (TaskFg, Bg, "[2]: "   )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteWarningLine(                   "e"  )).Verifiable();

        ui1.WriteWarningLine("a");
        ui1.WriteWarningLine(null);
        ui1.Write           ("b");
        ui1.WriteWarningLine("c");
        ui1.Write           ("d");
        ui2.WriteWarningLine("e");
    }

    [Test]
    public void WriteErrorLine()
    {
        using var my = new TaskHostTestHarness();

        var ui1 = my.Factory.Create("1").UI;
        var ui2 = my.Factory.Create("2").UI;

        var s = new MockSequence();
        InnerUI.InSequence(s).Setup(u => u.Write         (TaskFg, Bg, "[1]: "   )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteErrorLine(                   "a"  )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write         (TaskFg, Bg, "[1]: "   )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteErrorLine(                    null)).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write         (TaskFg, Bg, "[1]: "   )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write         (                   "b"  )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteErrorLine(                    "c" )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write         (TaskFg, Bg, "[1]: "   )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write         (                   "d"  )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteLine     (                        )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write         (TaskFg, Bg, "[2]: "   )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.WriteErrorLine(                   "e"  )).Verifiable();

        ui1.WriteErrorLine("a");
        ui1.WriteErrorLine(null);
        ui1.Write         ("b");
        ui1.WriteErrorLine("c");
        ui1.Write         ("d");
        ui2.WriteErrorLine("e");
    }

    [Test]
    public void WriteInformation()
    {
        using var my = new TaskHostTestHarness();

        var UI     = my.Factory.Create("1").UI;
        var record = new InformationRecord("a", "b");

        InnerUI
            .Setup(u => u.WriteInformation(record))
            .Verifiable();

        UI.WriteInformation(record);
    }

    [Test]
    public void WriteProgress()
    {
        using var my = new TaskHostTestHarness();

        var UI     = my.Factory.Create("1").UI;
        var record = new ProgressRecord(42, "a", "b");

        InnerUI
            .Setup(u => u.WriteProgress(123, record))
            .Verifiable();

        UI.WriteProgress(123, record);
    }

    [Test]
    public void ReadLine()
    {
        using var my = new TaskHostTestHarness();

        var UI   = my.Factory.Create("1").UI;
        var text = "input";

        var s = new MockSequence();

        InnerUI.InSequence(s).Setup(u => u.Write(TaskFg, Bg, "[1]: "      )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write(                   "before")).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.ReadLine()).Returns(text)          .Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write(TaskFg, Bg, "[1]: "      )).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write(                   "after" )).Verifiable();

        UI.Write("before");
        var result = UI.ReadLine();
        UI.Write("after");

        result.Should().BeSameAs(text);
    }

    [Test]
    public void ReadLineAsSecureString()
    {
        using var my   = new TaskHostTestHarness();
        using var text = new SecureString();

        text.AppendChar('x');
        text.MakeReadOnly();

        var UI   = my.Factory.Create("1").UI;

        var s = new MockSequence();
        InnerUI.InSequence(s).Setup(u => u.Write(TaskFg, Bg, "[1]: "      ))    .Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write(                   "before"))    .Verifiable();
        InnerUI.InSequence(s).Setup(u => u.ReadLineAsSecureString()).Returns(text).Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write(TaskFg, Bg, "[1]: "      ))    .Verifiable();
        InnerUI.InSequence(s).Setup(u => u.Write(                   "after" ))    .Verifiable();

        UI.Write("before");
        var result = UI.ReadLineAsSecureString();
        UI.Write("after");

        result.Should().BeSameAs(text);
    }
#endif

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

    private void ExpectWriteLine(MockSequence s, string? text)
    {
        InnerUI.InSequence(s).Setup(u => u.WriteLine(text)).Verifiable();
    }

    private void ExpectWriteLine(MockSequence s)
    {
        InnerUI.InSequence(s).Setup(u => u.WriteLine()).Verifiable();
    }

    private void ExpectWriteWarningLine(MockSequence s, string? text)
    {
        InnerUI.InSequence(s).Setup(u => u.WriteWarningLine(text)).Verifiable();
    }

    private void ExpectWriteVerboseLine(MockSequence s, string? text)
    {
        InnerUI.InSequence(s).Setup(u => u.WriteVerboseLine(text)).Verifiable();
    }

    private void ExpectWriteDebugLine(MockSequence s, string? text)
    {
        InnerUI.InSequence(s).Setup(u => u.WriteDebugLine(text)).Verifiable();
    }
}
