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

using System.Globalization;

namespace Subatomix.PowerShell.TaskHost;

[TestFixture]
public class TaskHostTests
{
    [Test]
    public void InstanceId_Get()
    {
        using var my = new TaskHostTestHarness();

        var host1 = my.Factory.Create();
        var host2 = my.Factory.Create();

        host1.InstanceId.Should().NotBeEmpty();
        host2.InstanceId.Should().NotBeEmpty().And.NotBe(host1.InstanceId);
    }

    [Test]
    public void Name_Get()
    {
        using var my = new TaskHostTestHarness();

        var host1 = my.Factory.Create();
        var host2 = my.Factory.Create();

        host1.Name.Should().Be("TaskHost<MockHost>#1");
        host2.Name.Should().Be("TaskHost<MockHost>#2");
    }

    [Test]
    public void Version_Get()
    {
        using var my = new TaskHostTestHarness();

        var host = my.Factory.Create();

        host.Version.Should().BeSameAs(TaskHostFactory.Version);
    }

    [Test]
    public void TaskHostUI_Get()
    {
        using var my = new TaskHostTestHarness();

        var host = my.Factory.Create();

        host.TaskHostUI.Should().BeOfType<TaskHostUI>();
    }

    [Test]
    public void UI_Get()
    {
        using var my = new TaskHostTestHarness();

        var host = my.Factory.Create();

        host.UI.Should().BeOfType<TaskHostUI>();
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void DebuggerEnabled_Get(bool value)
    {
        using var my = new TaskHostTestHarness();

        var host = my.Factory.Create();

        my.Host
            .Setup(h => h.DebuggerEnabled)
            .Returns(value)
            .Verifiable();

        host.DebuggerEnabled.Should().Be(value);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void DebuggerEnabled_Set(bool value)
    {
        using var my = new TaskHostTestHarness();

        var host = my.Factory.Create();

        my.Host
            .SetupSet(h => h.DebuggerEnabled = value)
            .Verifiable();

        host.DebuggerEnabled = value;
    }

    [Test]
    public void CurrentCulture_Get()
    {
        using var my = new TaskHostTestHarness();

        var host     = my.Factory.Create();
        var expected = CultureInfo.GetCultureInfo("kl-GL");

        my.Host
            .Setup(h => h.CurrentCulture)
            .Returns(expected)
            .Verifiable();

        host.CurrentCulture.Should().BeSameAs(expected);
    }

    [Test]
    public void CurrentUICulture_Get()
    {
        using var my = new TaskHostTestHarness();

        var host     = my.Factory.Create();
        var expected = CultureInfo.GetCultureInfo("kl-GL");

        my.Host
            .Setup(h => h.CurrentUICulture)
            .Returns(expected)
            .Verifiable();

        host.CurrentUICulture.Should().BeSameAs(expected);
    }

    [Test]
    public void PrivateData_Get()
    {
        using var my = new TaskHostTestHarness();

        var host     = my.Factory.Create();
        var expected = new PSObject("test private data");

        my.Host
            .Setup(h => h.PrivateData)
            .Returns(expected)
            .Verifiable();

        host.PrivateData.Should().BeSameAs(expected);
    }

    [Test]
    public void EnterNestedPrompt()
    {
        using var my = new TaskHostTestHarness();

        var host = my.Factory.Create();

        my.Host
            .Setup(h => h.EnterNestedPrompt())
            .Verifiable();

        host.EnterNestedPrompt();
    }

    [Test]
    public void ExitNestedPrompt()
    {
        using var my = new TaskHostTestHarness();

        var host = my.Factory.Create();

        my.Host
            .Setup(h => h.ExitNestedPrompt())
            .Verifiable();

        host.ExitNestedPrompt();
    }

    [Test]
    public void NotifyBeginApplication()
    {
        using var my = new TaskHostTestHarness();

        var host = my.Factory.Create();

        my.Host
            .Setup(h => h.NotifyBeginApplication())
            .Verifiable();

        host.NotifyBeginApplication();
    }

    [Test]
    public void NotifyEndApplication()
    {
        using var my = new TaskHostTestHarness();

        var host = my.Factory.Create();

        my.Host
            .Setup(h => h.NotifyEndApplication())
            .Verifiable();

        host.NotifyEndApplication();
    }

    [Test]
    public void SetShouldExit()
    {
        using var my = new TaskHostTestHarness();

        var host = my.Factory.Create();
        var code = my.Random.Next();

        my.Host
            .Setup(h => h.SetShouldExit(code))
            .Verifiable();

        host.SetShouldExit(code);
    }
}
