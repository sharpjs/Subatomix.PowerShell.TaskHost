// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

[TestFixture]
internal class InvocationTests : IDisposable
{
    private readonly Runspace _runspace;

    // Tests omitted where coverage provided by IntegrationTests

    public InvocationTests()
    {
        _runspace = RunspaceFactory.CreateRunspace();
        Runspace.DefaultRunspace = _runspace;
    }

    public void Dispose()
    {
        Runspace.DefaultRunspace = null;
        _runspace.Dispose();
    }

    [Test]
    public void UseHost_NullHost()
    {
        using var invocation = new Invocation();

        invocation
            .Invoking(i => i.UseHost(default!))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void AddScript_NullScriptBlock()
    {
        using var invocation = new Invocation();

        invocation
            .Invoking(i => i.AddScript(default!))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Dispose_Unmanaged()
    {
        using var invocation = new Invocation();

        invocation.SimulateFinalizer(); // does nothing
    }
}
