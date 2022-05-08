/*
    Copyright 2022 Jeffrey Sharp

    Permission to use, copy, modify, and distribute this software for any
    purpose with or without fee is hereby granted, provided that the above
    copyright notice and this permission notice appear in all copies.

    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
    WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
    MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
    ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
    WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
    ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
    OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
*/

using System.Collections.ObjectModel;
using System.Security;

namespace Subatomix.PowerShell.TaskHost;

[TestFixture]
public class TaskHostUITests
{
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

        var ui    = my.Factory.Create().UI;
        var rawUI = my.Mocks.Create<PSHostRawUserInterface>().Object;

        my.UI.Setup(u => u.RawUI)
            .Returns(rawUI)
            .Verifiable();

        ui.RawUI.Should().BeSameAs(rawUI);
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
        my.UI.InSequence(s).Setup(u => u.Write    ("[1]: a"        )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (      ""        )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (      "\n"      )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    ("[1]: "         )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (     "b"        )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine(                )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    ("[2]: c"        )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine(                )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    ("[1]: (...) d"  )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (            "\n")).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    ("[2]: (...) e"  )).Verifiable();

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
    public void Write_WithColors(
        [Random(1)] ConsoleColor fg,
        [Random(1)] ConsoleColor bg)
    {
        using var my = new TaskHostTestHarness();

        var ui1 = my.Factory.Create("1").UI;
        var ui2 = my.Factory.Create("2").UI;

        var s = new MockSequence();
        my.UI.InSequence(s).Setup(u => u.Write    (fg, bg, "[1]: a"        )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (fg, bg,       ""        )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (fg, bg,       "\n"      )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (fg, bg, "[1]: "         )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (fg, bg,      "b"        )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine(                        )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (fg, bg, "[2]: c"        )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine(                        )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (fg, bg, "[1]: (...) d"  )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (fg, bg,             "\n")).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (fg, bg, "[2]: (...) e"  )).Verifiable();

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
        my.UI.InSequence(s).Setup(u => u.WriteLine("[1]: "  )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    ("[1]: b" )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine(       "")).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    ("[1]: c" )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine(         )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine("[2]: "  )).Verifiable();

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
        my.UI.InSequence(s).Setup(u => u.WriteLine("[1]: a" )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine("[1]: "  )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    ("[1]: b" )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine(      "c")).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    ("[1]: d" )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine(         )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine("[2]: e" )).Verifiable();

        ui1.WriteLine("a");
        ui1.WriteLine(null);
        ui1.Write    ("b");
        ui1.WriteLine("c");
        ui1.Write    ("d");
        ui2.WriteLine("e");
    }

    [Test]
    public void WriteLine_WithColors(
        [Random(1)] ConsoleColor fg,
        [Random(1)] ConsoleColor bg)
    {
        using var my = new TaskHostTestHarness();

        var ui1 = my.Factory.Create("1").UI;
        var ui2 = my.Factory.Create("2").UI;

        var s = new MockSequence();
        my.UI.InSequence(s).Setup(u => u.WriteLine(fg, bg, "[1]: a" )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine(fg, bg, "[1]: "  )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (fg, bg, "[1]: b" )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine(fg, bg,       "c")).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write    (fg, bg, "[1]: d" )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine(                 )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine(fg, bg, "[2]: e" )).Verifiable();

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
        my.UI.InSequence(s).Setup(u => u.WriteDebugLine("[1]: a" )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteDebugLine("[1]: "  )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write         ("[1]: b" )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteDebugLine(      "c")).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write         ("[1]: d" )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine     (         )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteDebugLine("[2]: e" )).Verifiable();

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
        my.UI.InSequence(s).Setup(u => u.WriteVerboseLine("[1]: a" )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteVerboseLine("[1]: "  )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write           ("[1]: b" )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteVerboseLine(      "c")).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write           ("[1]: d" )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine       (         )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteVerboseLine("[2]: e" )).Verifiable();

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
        my.UI.InSequence(s).Setup(u => u.WriteWarningLine("[1]: a" )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteWarningLine("[1]: "  )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write           ("[1]: b" )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteWarningLine(      "c")).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write           ("[1]: d" )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine       (         )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteWarningLine("[2]: e" )).Verifiable();

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
        my.UI.InSequence(s).Setup(u => u.WriteErrorLine("[1]: a" )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteErrorLine("[1]: "  )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write         ("[1]: b" )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteErrorLine(      "c")).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write         ("[1]: d" )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteLine     (         )).Verifiable();
        my.UI.InSequence(s).Setup(u => u.WriteErrorLine("[2]: e" )).Verifiable();

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
        my.UI.InSequence(s).Setup(u => u.Write("[1]: before"))    .Verifiable();
        my.UI.InSequence(s).Setup(u => u.ReadLine()).Returns(text).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write("[1]: after"))     .Verifiable();

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
        my.UI.InSequence(s).Setup(u => u.Write("[1]: before"))                  .Verifiable();
        my.UI.InSequence(s).Setup(u => u.ReadLineAsSecureString()).Returns(text).Verifiable();
        my.UI.InSequence(s).Setup(u => u.Write("[1]: after"))                   .Verifiable();

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
