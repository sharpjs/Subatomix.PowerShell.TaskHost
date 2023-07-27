// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Collections.ObjectModel;
using System.Security;

namespace Subatomix.PowerShell.TaskHost;

using static It;

[TestFixture]
public class TaskHostUITests
{
    private const ConsoleColor
        StampFg  = ConsoleColor.DarkGray,
        HeaderFg = ConsoleColor.DarkBlue,
        MoreFg   = ConsoleColor.DarkGray,
        Bg       = ConsoleColor.Black;

    private const string
        StampRe = @"^\[\+[0-9]{2}:[0-9]{2}:[0-9]{2}\] $";

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
            .Should().BeOfType<TaskHostUI>().AssignTo(out var ui);

        ui.Header.Should().BeSameAs(header);
    }

    [Test]
    public void Header_Set()
    {
        using var my = new TaskHostTestHarness();

        var header = my.Random.GetString();

        my.Factory.Create(header).UI
            .Should().BeOfType<TaskHostUI>().AssignTo(out var ui);

        ui.Header = header;
        ui.Header.Should().BeSameAs(header);
    }

    [Test]
    public void Header_Set_Null()
    {
        using var my = new TaskHostTestHarness();

        my.Factory.Create().UI
            .Should().BeOfType<TaskHostUI>().AssignTo(out var ui);

        ui.Invoking(u => u.Header = null!)
            .Should().ThrowExactly<ArgumentNullException>();
    }

    [Test]
    public void RawUI_Get()
    {
        using var my = new TaskHostTestHarness();

        var ui = my.Factory.Create().UI;

        ui.RawUI.Should().BeOfType<TaskHostRawUI>();
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SupportsVirtualTerminal_Get(bool value)
    {
        using var my = new TaskHostTestHarness();

        var ui = my.Factory.Create().UI;

        my.UI.Setup(u => u.SupportsVirtualTerminal)
            .Returns(value)
            .Verifiable();

        ui.SupportsVirtualTerminal.Should().Be(value);
    }

    [Test]
    public void Write()
    {
        using var my = new TaskHostTestHarness();

        var ui1 = my.Factory.Create("1").UI;
        var ui2 = my.Factory.Create("2").UI;

        var s = new MockSequence();
        my.UI.InSequence(s).Setup(u => u.Write    (HeaderFg, Bg, "[1]: "         )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (                   "a"        )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (                    null      )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (                    "\n"      )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (HeaderFg, Bg, "[1]: "         )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (                    null      )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (                   "b"        )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine(                              )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (HeaderFg, Bg, "[2]: "         )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (                   "c"        )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine(                              )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (HeaderFg, Bg, "[1]: "         )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (MoreFg,   Bg,      "(...) "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (                         "d"  )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (                          "\n")).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (HeaderFg, Bg, "[2]: "         )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (MoreFg,   Bg,      "(...) "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (                         "e"  )).Verifiable();

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
    public void Write_WithElapsed()
    {
        using var my = new TaskHostTestHarness(withElapsed: true);

        var ui1 = my.Factory.Create("1").UI;
        var ui2 = my.Factory.Create("2").UI;

        var s = new MockSequence();
        my.UI.InSequence(s).Setup(u => u.Write    (StampFg,  Bg, IsRegex(StampRe))).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (HeaderFg, Bg, "[1]: "         )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (                   "a"        )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (                    null      )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (                    "\n"      )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (StampFg,  Bg, IsRegex(StampRe))).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (HeaderFg, Bg, "[1]: "         )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (                    null      )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (                   "b"        )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine(                              )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (StampFg,  Bg, IsRegex(StampRe))).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (HeaderFg, Bg, "[2]: "         )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (                   "c"        )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine(                              )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (StampFg,  Bg, IsRegex(StampRe))).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (HeaderFg, Bg, "[1]: "         )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (MoreFg,   Bg,      "(...) "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (                         "d"  )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (                          "\n")).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (StampFg,  Bg, IsRegex(StampRe))).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (HeaderFg, Bg, "[2]: "         )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (MoreFg,   Bg,      "(...) "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (                         "e"  )).Verifiable();

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
    public void Write_EmptyHeader()
    {
        using var my = new TaskHostTestHarness();

        var ui1 = my.Factory.Create("1").TaskHostUI;
        var ui2 = my.Factory.Create("2").TaskHostUI;

        ui1.Header = "";
        ui2.Header = "";

        var s = new MockSequence();
        my.UI.InSequence(s).Setup(u => u.Write    (            "a"        )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (             null      )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (             "\n"      )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (             null      )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (            "b"        )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine(                       )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (            "c"        )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine(                       )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (MoreFg, Bg, "(...) "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (                  "d"  )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (                   "\n")).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (MoreFg, Bg, "(...) "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (                  "e"  )).Verifiable();

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
        my.UI.InSequence(s).Setup(u => u.Write    (StampFg, Bg, IsRegex(StampRe))).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (             "a"             )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (              null           )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (              "\n"           )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (StampFg, Bg, IsRegex(StampRe))).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (             null            )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (             "b"             )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine(                             )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (StampFg, Bg, IsRegex(StampRe))).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (             "c"             )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine(                             )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (StampFg, Bg, IsRegex(StampRe))).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (MoreFg,  Bg, "(...) "        )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (                   "d"       )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (                    "\n"     )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (StampFg, Bg, IsRegex(StampRe))).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (MoreFg,  Bg, "(...) "        )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (                   "e"       )).Verifiable();

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
        my.UI.InSequence(s).Setup(u => u.Write    (HeaderFg, Bg, "[1]: "         )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (fg,       bg,      "a"        )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (fg,       bg,       null      )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (fg,       bg,       "\n"      )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (HeaderFg, Bg, "[1]: "         )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (fg,       bg,      null       )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (fg,       bg,      "b"        )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine(                              )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (HeaderFg, Bg, "[2]: "         )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (fg,       bg,      "c"        )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine(                              )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (HeaderFg, Bg, "[1]: "         )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (MoreFg,   Bg,      "(...) "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (fg,       bg,            "d"  )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (fg,       bg,             "\n")).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (HeaderFg, Bg, "[2]: "         )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (MoreFg,   Bg,      "(...) "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (fg,       bg,            "e"  )).Verifiable();

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
        my.UI.InSequence(s).Setup(u => u.Write    (HeaderFg, Bg, "[1]: " )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine(                      )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (HeaderFg, Bg, "[1]: " )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (                   "b")).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine(                      )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (HeaderFg, Bg, "[1]: " )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (                   "c")).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine(                      )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (HeaderFg, Bg, "[2]: " )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine(                      )).Verifiable();

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
        my.UI.InSequence(s).Setup(u => u.Write    (HeaderFg, Bg, "[1]: "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine(                   "a"  )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (HeaderFg, Bg, "[1]: "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine(                    null)).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (HeaderFg, Bg, "[1]: "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (                   "b"  )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine(                    "c" )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (HeaderFg, Bg, "[1]: "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (                   "d"  )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine(                        )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (HeaderFg, Bg, "[2]: "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine(                   "e"  )).Verifiable();

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
        my.UI.InSequence(s).Setup(u => u.Write    (HeaderFg, Bg, "[1]: "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine(fg,       bg,      "a"  )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (HeaderFg, Bg, "[1]: "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine(fg,       bg,       null)).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (HeaderFg, Bg, "[1]: "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (fg,       bg,      "b"  )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine(fg,       bg,       "c" )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (HeaderFg, Bg, "[1]: "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (fg,       bg,      "d"  )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine(                        )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (HeaderFg, Bg, "[2]: "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine(fg,       bg,      "e"  )).Verifiable();

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
        my.UI.InSequence(s).Setup(u => u.Write         (HeaderFg, Bg, "[1]: "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteDebugLine(                   "a"  )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write         (HeaderFg, Bg, "[1]: "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteDebugLine(                    null)).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write         (HeaderFg, Bg, "[1]: "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write         (                   "b"  )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteDebugLine(                    "c" )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write         (HeaderFg, Bg, "[1]: "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write         (                   "d"  )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine     (                        )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write         (HeaderFg, Bg, "[2]: "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteDebugLine(                   "e"  )).Verifiable();

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
        my.UI.InSequence(s).Setup(u => u.Write           (HeaderFg, Bg, "[1]: "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteVerboseLine(                   "a"  )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write           (HeaderFg, Bg, "[1]: "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteVerboseLine(                    null)).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write           (HeaderFg, Bg, "[1]: "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write           (                   "b"  )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteVerboseLine(                    "c" )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write           (HeaderFg, Bg, "[1]: "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write           (                   "d"  )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine       (                        )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write           (HeaderFg, Bg, "[2]: "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteVerboseLine(                   "e"  )).Verifiable();

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
        my.UI.InSequence(s).Setup(u => u.Write           (HeaderFg, Bg, "[1]: "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteWarningLine(                   "a"  )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write           (HeaderFg, Bg, "[1]: "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteWarningLine(                    null)).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write           (HeaderFg, Bg, "[1]: "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write           (                   "b"  )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteWarningLine(                    "c" )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write           (HeaderFg, Bg, "[1]: "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write           (                   "d"  )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine       (                        )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write           (HeaderFg, Bg, "[2]: "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteWarningLine(                   "e"  )).Verifiable();

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
        my.UI.InSequence(s).Setup(u => u.Write         (HeaderFg, Bg, "[1]: "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteErrorLine(                   "a"  )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write         (HeaderFg, Bg, "[1]: "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteErrorLine(                    null)).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write         (HeaderFg, Bg, "[1]: "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write         (                   "b"  )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteErrorLine(                    "c" )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write         (HeaderFg, Bg, "[1]: "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write         (                   "d"  )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine     (                        )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write         (HeaderFg, Bg, "[2]: "   )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteErrorLine(                   "e"  )).Verifiable();

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

        var ui     = my.Factory.Create("1").UI;
        var record = new InformationRecord("a", "b");

        my.UI
            .Setup(u => u.WriteInformation(record))
            .Verifiable();

        ui.WriteInformation(record);
    }

    [Test]
    public void WriteProgress()
    {
        using var my = new TaskHostTestHarness();

        var ui     = my.Factory.Create("1").UI;
        var record = new ProgressRecord(42, "a", "b");

        my.UI
            .Setup(u => u.WriteProgress(123, record))
            .Verifiable();

        ui.WriteProgress(123, record);
    }

    [Test]
    public void ReadLine()
    {
        using var my = new TaskHostTestHarness();

        var ui   = my.Factory.Create("1").UI;
        var text = "input";

        var s = new MockSequence();

        my.UI.InSequence(s).Setup(u => u.Write(HeaderFg, Bg, "[1]: "      )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write(                   "before")).Verifiable();
        my.UI.InSequence(s).Setup(u => u.ReadLine()).Returns(text)          .Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write(HeaderFg, Bg, "[1]: "      )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write(                   "after" )).Verifiable();

        ui.Write("before");
        var result = ui.ReadLine();
        ui.Write("after");

        result.Should().BeSameAs(text);
    }

    [Test]
    public void ReadLineAsSecureString()
    {
        using var my   = new TaskHostTestHarness();
        using var text = new SecureString();

        text.AppendChar('x');
        text.MakeReadOnly();

        var ui   = my.Factory.Create("1").UI;

        var s = new MockSequence();
        my.UI.InSequence(s).Setup(u => u.Write(HeaderFg, Bg, "[1]: "      ))    .Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write(                   "before"))    .Verifiable();
        my.UI.InSequence(s).Setup(u => u.ReadLineAsSecureString()).Returns(text).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write(HeaderFg, Bg, "[1]: "      ))    .Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write(                   "after" ))    .Verifiable();

        ui.Write("before");
        var result = ui.ReadLineAsSecureString();
        ui.Write("after");

        result.Should().BeSameAs(text);
    }

    [Test]
    public void Prompt()
    {
        using var my = new TaskHostTestHarness();

        var ui     = my.Factory.Create().UI;
        var fields = new Collection<FieldDescription>();
        var values = new Dictionary<string, PSObject>();

        my.UI
            .Setup(u => u.Prompt("a", "b", fields))
            .Returns(values)
            .Verifiable();

        ui.Prompt("a", "b", fields).Should().BeSameAs(values);
    }

    [Test]
    public void PromptForChoice()
    {
        using var my = new TaskHostTestHarness();

        var ui      = my.Factory.Create().UI;
        var choices = new Collection<ChoiceDescription>();

        my.UI
            .Setup(u => u.PromptForChoice("a", "b", choices, 4))
            .Returns(3)
            .Verifiable();

        var result = ui.PromptForChoice("a", "b", choices, 4);

        result.Should().Be(3);
    }

    [Test]
    public void PromptForCredential()
    {
        using var my       = new TaskHostTestHarness();
        using var password = new SecureString();

        password.AppendChar('y');
        password.MakeReadOnly();

        var ui         = my.Factory.Create().UI;
        var credential = new PSCredential("username", password);

        my.UI
            .Setup(u => u.PromptForCredential("a", "b", "c", "d"))
            .Returns(credential)
            .Verifiable();

        var result = ui.PromptForCredential("a", "b", "c", "d");

        result.Should().BeSameAs(credential);
    }

    [Test]
    public void PromptForCredential_WithTypesAndOptions()
    {
        using var my       = new TaskHostTestHarness();
        using var password = new SecureString();

        password.AppendChar('y');
        password.MakeReadOnly();

        var ui         = my.Factory.Create().UI;
        var credential = new PSCredential("username", password);
        var types      = PSCredentialTypes.Generic;
        var options    = PSCredentialUIOptions.ReadOnlyUserName;

        my.UI
            .Setup(u => u.PromptForCredential("a", "b", "c", "d", types, options))
            .Returns(credential)
            .Verifiable();

        var result = ui.PromptForCredential("a", "b", "c", "d", types, options);

        result.Should().BeSameAs(credential);
    }
}
