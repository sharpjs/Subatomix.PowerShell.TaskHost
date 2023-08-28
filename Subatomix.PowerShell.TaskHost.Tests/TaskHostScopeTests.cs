// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Globalization;

namespace Subatomix.PowerShell.TaskHost;

[TestFixture]
public class TaskHostScopeTests
{
    [Test]
    public void Construct_NullHost()
    {
        Invoking(() => new TaskHostScope(null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void ConstructAndDispose_HostDoesNotHaveExternalHostProperty()
    {
        using var scope = new TaskHostScope(new FakeHost());

        scope.WasTaskHostActivated.Should().BeFalse();
    }

    [Test]
    public void ConstructAndDispose_HostDoesNotHaveSetHostRefMethod()
    {
        using var scope = new TaskHostScope(new FakeInternalHostImpostor0());

        scope.WasTaskHostActivated.Should().BeFalse();
    }

    [Test]
    public void ConstructAndDispose_HostDoesNotHaveRevertHostRefMethod()
    {
        using var scope = new TaskHostScope(new FakeInternalHostImpostor1());

        scope.WasTaskHostActivated.Should().BeTrue(); // live with it
    }

    [Test]
    public void ConstructAndDispose_HostThrows()
    {
        using var scope = new TaskHostScope(new FakeInternalHost() { ShouldMethodsThrow = true });

        scope.WasTaskHostActivated.Should().BeFalse();
    }

    [Test]
    public void ConstructAndDispose_HostAlreadyHasTaskHost()
    {
        var host = new FakeInternalHost();

        var taskHost = new TaskHost(new FakeHost());
        host.SetHostRef(taskHost);

        using var scope = new TaskHostScope(host);

        scope.WasTaskHostActivated.Should().BeFalse();
        host.ExternalHost         .Should().BeSameAs(taskHost);
    }

    [Test]
    public void ConstructAndDispose_Normal()
    {
        var host = new FakeInternalHost();

        using (var scope = new TaskHostScope(host))
        {
            scope.WasTaskHostActivated.Should().BeTrue();
            host.ExternalHost         .Should().BeOfType<TaskHost>();
        }

        host.ExternalHost.Should().BeSameAs(host.OriginalExternalHost);
    }

    [Test]
    public void Restore_NotActivated()
    {
        using var scope = new TaskHostScope(new FakeHost());

        scope.WasTaskHostActivated.Should().BeFalse();

        scope.Restore(); // does nothing
    }

    [Test]
    public void Restore_NotChanged()
    {
        var host = new FakeInternalHost();

        using var scope = new TaskHostScope(host);

        var taskHost = host.ExternalHost;

        scope.Restore();

        host.ExternalHost.Should().BeSameAs(taskHost);
    }

    [Test]
    public void Restore_Normal()
    {
        var host = new FakeInternalHost();

        using var scope = new TaskHostScope(host);

        var taskHost  = host.ExternalHost;
        var otherHost = new FakeHost();
        host.SetHostRef(otherHost);

        host.ExternalHost.Should().BeSameAs(otherHost);

        scope.Restore();

        host.ExternalHost.Should().BeSameAs(taskHost);
    }

    [Test]
    public void Restore_Exception()
    {
        var host = new FakeInternalHost();

        using var scope = new TaskHostScope(host);

        var taskHost  = host.ExternalHost;
        var otherHost = new FakeHost();
        host.SetHostRef(otherHost);
        host.ExternalHost.Should().BeSameAs(otherHost);

        host.ShouldMethodsThrow = true;

        scope.Restore();

        host.ExternalHost.Should().BeSameAs(otherHost);
    }

    private class FakeInternalHostImpostor0 : FakeHost
    {
        internal PSHost ExternalHost { get; } = new FakeHost();

        // No SetHostRef method
        // No RevertHostRef method
    }

    private class FakeInternalHostImpostor1 : FakeHost
    {
        internal PSHost ExternalHost { get; private set; } = new FakeHost();

        internal void SetHostRef(PSHost host) => ExternalHost = host;

        // No RevertHostRef method
    }

    private class FakeInternalHost : FakeHost
    {
        public PSHost OriginalExternalHost { get; } = new FakeHost();
        public bool   ShouldMethodsThrow   { get; set; }

        internal PSHost ExternalHost { get; private set; }

        public FakeInternalHost()
        {
            ExternalHost = OriginalExternalHost;
        }

        internal void SetHostRef(PSHost host)
        {
            if (ShouldMethodsThrow) throw new NotSupportedException();
            ExternalHost = host;
        }

        internal void RevertHostRef()
        {
            if (ShouldMethodsThrow) throw new NotSupportedException();
            ExternalHost = OriginalExternalHost;
        }
    }

    private class FakeHost : PSHost
    {
        public override PSHostUserInterface? UI { get; }
            = Mock.Of<PSHostUserInterface>(u => u.RawUI == Mock.Of<PSHostRawUserInterface>());

        public override string      Name             => GetType().Name;
        public override Guid        InstanceId       => throw new NotImplementedException();
        public override Version     Version          => throw new NotImplementedException();
        public override CultureInfo CurrentCulture   => throw new NotImplementedException();
        public override CultureInfo CurrentUICulture => throw new NotImplementedException();

        public override void EnterNestedPrompt()         => throw new NotImplementedException();
        public override void ExitNestedPrompt()          => throw new NotImplementedException();
        public override void NotifyBeginApplication()    => throw new NotImplementedException();
        public override void NotifyEndApplication()      => throw new NotImplementedException();
        public override void SetShouldExit(int exitCode) => throw new NotImplementedException();
    }
}
