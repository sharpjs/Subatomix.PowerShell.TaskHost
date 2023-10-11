// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Globalization;

namespace Subatomix.PowerShell.TaskHost;

[TestFixture]
public class ExtensionsTests
{
    [Test]
    public void Sanitize_Null()
    {
        (null as string?[]).Sanitize().Should().BeEmpty();
    }

    [Test]
    public void Sanitize_Empty()
    {
        var array = new string?[0]; // Do not refactor to use Array.Empty

        array.Sanitize().Should().BeSameAs(array);
    }

    [Test]
    public void Sanitize_Normal()
    {
        var array = new string?[] { "a", "b" };

        array.Sanitize().Should().BeSameAs(array);
    }

    [Test]
    public void Sanitize_NullElement()
    {
        var array = new string?[] { "a", null, "b" };

        array.Sanitize().Should().Equal("a", "b");
    }

    [Test]
    public void ShouldByPass_Null()
    {
        Invoking(() => (null as PSCmdlet)!.ShouldBypass())
            .Should().Throw<ArgumentNullException>();
    }

    // ShouldByPass_False: covered via integration tests
    // ShouldByPass_True:  covered via integration tests

    [Test]
    public void Unwrap_Null()
    {
        Invoking(() => (null as PSHost)!.Unwrap())
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Unwrap_NotWrapped()
    {
        var host = new FakeHost();

        host.Unwrap().Should().BeSameAs(host);
    }

    [Test]
    public void Unwrap_WrappedNull()
    {
        var host = new FakeInternalHost();

        host.Unwrap().Should().BeSameAs(host);
    }

    [Test]
    public void Unwrap_Wrapped()
    {
        var inner = new FakeHost();
        var outer = new FakeInternalHost { ExternalHost = inner };

        outer.Unwrap().Should().BeSameAs(inner);
    }

    private class FakeInternalHost : FakeHost
    {
        internal PSHost? ExternalHost { get; set; }
    }

    private class FakeHost : PSHost
    {
        public override PSHostUserInterface? UI               => throw new NotImplementedException();
        public override CultureInfo          CurrentCulture   => throw new NotImplementedException();
        public override CultureInfo          CurrentUICulture => throw new NotImplementedException();
        public override Guid                 InstanceId       => throw new NotImplementedException();
        public override string               Name             => throw new NotImplementedException();
        public override Version              Version          => throw new NotImplementedException();

        public override void EnterNestedPrompt()         => throw new NotImplementedException();
        public override void ExitNestedPrompt()          => throw new NotImplementedException();
        public override void NotifyBeginApplication()    => throw new NotImplementedException();
        public override void NotifyEndApplication()      => throw new NotImplementedException();
        public override void SetShouldExit(int exitCode) => throw new NotImplementedException();
    }
}
