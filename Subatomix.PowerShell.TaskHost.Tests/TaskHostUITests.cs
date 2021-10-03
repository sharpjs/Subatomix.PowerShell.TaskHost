using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Security;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Subatomix.PowerShell.TaskHost
{
    [TestFixture]
    public class TaskHostUITests
    {
        [Test]
        public void Header_Get_Default()
        {
            using var my = new TaskHostTestHarness();

            my.Factory.Create(my.Host.Object).UI
                .Should().BeOfType<TaskHostUI>().AssignTo(out var ui1);

            my.Factory.Create(my.Host.Object).UI
                .Should().BeOfType<TaskHostUI>().AssignTo(out var ui2);

            ui1.Header.Should().Be("Task 1");
            ui2.Header.Should().Be("Task 2");
        }

        [Test]
        public void Header_Get_Explicit()
        {
            using var my = new TaskHostTestHarness();

            var header = my.Random.GetString();

            my.Factory.Create(my.Host.Object, header).UI
                .Should().BeOfType<TaskHostUI>().AssignTo(out var ui);

            ui.Header.Should().BeSameAs(header);
        }

        [Test]
        public void Header_Set()
        {
            using var my = new TaskHostTestHarness();

            var header = my.Random.GetString();

            my.Factory.Create(my.Host.Object, header).UI
                .Should().BeOfType<TaskHostUI>().AssignTo(out var ui);

            ui.Header = header;
            ui.Header.Should().BeSameAs(header);
        }

        [Test]
        public void Header_Set_Null()
        {
            using var my = new TaskHostTestHarness();

            my.Factory.Create(my.Host.Object).UI
                .Should().BeOfType<TaskHostUI>().AssignTo(out var ui);

            ui.Invoking(u => u.Header = null!)
                .Should().ThrowExactly<ArgumentNullException>();
        }

        [Test]
        public void RawUI_Get()
        {
            using var my = new TaskHostTestHarness();

            var ui    = my.Factory.Create(my.Host.Object).UI;
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

            var ui = my.Factory.Create(my.Host.Object).UI;

            my.UI.Setup(u => u.SupportsVirtualTerminal)
                .Returns(value)
                .Verifiable();

            ui.SupportsVirtualTerminal.Should().Be(value);
        }

        [Test]
        public void Write_ConsoleBol()
        {
            using var my = new TaskHostTestHarness();

            var ui1 = my.Factory.Create(my.Host.Object, "1").UI;
            var ui2 = my.Factory.Create(my.Host.Object, "2").UI;

            var s = new MockSequence();
            my.UI.InSequence(s).Setup(u => u.Write("[1]: a"))      .Verifiable();
            my.UI.InSequence(s).Setup(u => u.Write(""))            .Verifiable();
            my.UI.InSequence(s).Setup(u => u.Write("b"))           .Verifiable();
            my.UI.InSequence(s).Setup(u => u.WriteLine())          .Verifiable();
            my.UI.InSequence(s).Setup(u => u.Write("[2]: c"))      .Verifiable();
            my.UI.InSequence(s).Setup(u => u.WriteLine())          .Verifiable();
            my.UI.InSequence(s).Setup(u => u.Write("[1]: (...) d")).Verifiable();
            my.UI.InSequence(s).Setup(u => u.Write("\n"))          .Verifiable();
            my.UI.InSequence(s).Setup(u => u.Write("[2]: (...) e")).Verifiable();
            my.UI.InSequence(s).Setup(u => u.WriteLine())          .Verifiable();
            my.UI.InSequence(s).Setup(u => u.Write("[1]: "))       .Verifiable();
            my.UI.InSequence(s).Setup(u => u.Write("f"))           .Verifiable();

            ui1.Write("a");
            ui1.Write(null);
            ui1.Write("b");
            ui2.Write("c");
            ui1.Write("d");
            ui1.Write("\n");
            ui2.Write("e");
            ui1.Write(null);
            ui1.Write("f");
        }

        [Test]
        public void Prompt()
        {
            using var my = new TaskHostTestHarness();

            var ui     = my.Factory.Create(my.Host.Object).UI;
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

            var ui      = my.Factory.Create(my.Host.Object).UI;
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

            var ui         = my.Factory.Create(my.Host.Object).UI;
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

            var ui         = my.Factory.Create(my.Host.Object).UI;
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
}
