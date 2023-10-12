// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Globalization;

namespace Subatomix.PowerShell.TaskHost;

[TestFixture]
[TestFixtureSource(nameof(Booleans))]
public class TaskHostTests : TestHarnessBase
{
    public static bool[] Booleans = { false, true };

    private TaskHost                     Host       { get; }

    private Mock<PSHost>                 InnerHost  { get; }
    private Mock<PSHostUserInterface>    InnerUI    { get; }
    private Mock<PSHostRawUserInterface> InnerRawUI { get; }

    public TaskHostTests(bool withElapsed)
    {
        InnerHost  = Mocks.Create<PSHost>();
        InnerUI    = Mocks.Create<PSHostUserInterface>();
        InnerRawUI = Mocks.Create<PSHostRawUserInterface>();

        InnerUI.Setup(u => u.RawUI).Returns(InnerRawUI.Object);

        InnerHost.Setup(h => h.Name).Returns("MockHost");
        InnerHost.Setup(h => h.UI  ).Returns(InnerUI.Object);

        Host = new TaskHost(InnerHost.Object, withElapsed);
    }

    [Test, NonParallelizable]
    public void Current_Get()
    {
        TaskHost.Current.Should().BeNull();
    }

    [Test, NonParallelizable]
    public void GetOrSetCurrent()
    {
        var host0 = Host; // from test setup
        var host1 = new TaskHost(InnerHost.Object, withElapsed: false);
        TaskHost host;

        // Root case:
        // - no TaskHost is current
        // - host0 becomes current
        // - ref host argument acts as an input
        host = host0;
        using (TaskHost.GetOrSetCurrent(ref host))
        {
            host            .Should().BeSameAs(host0);
            TaskHost.Current.Should().BeSameAs(host0);

            // Nested case:
            // - host0 is current
            // - host1 does *not* become current
            // - ref host argument acts as an output, is set to host0
            host = host1;
            using (TaskHost.GetOrSetCurrent(ref host))
            {
                host            .Should().BeSameAs(host0); // *not* host1
                TaskHost.Current.Should().BeSameAs(host0); // *not* host1
            }

            TaskHost.Current.Should().BeSameAs(host0);
        }

        TaskHost.Current.Should().BeNull();
    }

    [Test]
    public void Initialize()
    {
        TaskHost.Initialize();
    }

    [Test]
    public void Construct_NullHost()
    {
        Invoking(() => new TaskHost(null!, false))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void InstanceId_Get()
    {
        Host.InstanceId.Should().NotBeEmpty();
    }

    [Test]
    public void Name_Get()
    {
        Host.Name.Should().Be("TaskHost<MockHost>");
    }

    [Test]
    public void Version_Get()
    {
        Host.Version.Should().Be(typeof(TaskHost).Assembly.GetName().Version);
    }

    [Test]
    public void UI_Get()
    {
        Host.UI.Should().BeOfType<TaskHostUI>();
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void DebuggerEnabled_Get(bool value)
    {
        InnerHost
            .Setup(h => h.DebuggerEnabled)
            .Returns(value)
            .Verifiable();

        Host.DebuggerEnabled.Should().Be(value);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void DebuggerEnabled_Set(bool value)
    {
        InnerHost
            .SetupSet(h => h.DebuggerEnabled = value)
            .Verifiable();

        Host.DebuggerEnabled = value;
    }

    [Test]
    public void CurrentCulture_Get()
    {
        var expected = CultureInfo.GetCultureInfo("kl-GL");

        InnerHost
            .Setup(h => h.CurrentCulture)
            .Returns(expected)
            .Verifiable();

        Host.CurrentCulture.Should().BeSameAs(expected);
    }

    [Test]
    public void CurrentUICulture_Get()
    {
        var expected = CultureInfo.GetCultureInfo("kl-GL");

        InnerHost
            .Setup(h => h.CurrentUICulture)
            .Returns(expected)
            .Verifiable();

        Host.CurrentUICulture.Should().BeSameAs(expected);
    }

    [Test]
    public void PrivateData_Get()
    {
        var expected = new PSObject("test private data");

        InnerHost
            .Setup(h => h.PrivateData)
            .Returns(expected)
            .Verifiable();

        Host.PrivateData.Should().BeSameAs(expected);
    }

    [Test]
    public void EnterNestedPrompt()
    {
        InnerHost
            .Setup(h => h.EnterNestedPrompt())
            .Verifiable();

        Host.EnterNestedPrompt();
    }

    [Test]
    public void ExitNestedPrompt()
    {
        InnerHost
            .Setup(h => h.ExitNestedPrompt())
            .Verifiable();

        Host.ExitNestedPrompt();
    }

    [Test]
    public void NotifyBeginApplication()
    {
        InnerHost
            .Setup(h => h.NotifyBeginApplication())
            .Verifiable();

        Host.NotifyBeginApplication();
    }

    [Test]
    public void NotifyEndApplication()
    {
        InnerHost
            .Setup(h => h.NotifyEndApplication())
            .Verifiable();

        Host.NotifyEndApplication();
    }

    [Test]
    public void SetShouldExit()
    {
        var code = Random.Next();

        InnerHost
            .Setup(h => h.SetShouldExit(code))
            .Verifiable();

        Host.SetShouldExit(code);
    }
}
