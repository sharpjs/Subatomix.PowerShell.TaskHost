// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Management.Automation.Language;
using Microsoft.PowerShell.Commands;

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
    public void UseTaskHost_NullCmdlet()
    {
        using var invocation = new Invocation();

        invocation
            .Invoking(i => i.UseTaskHost(default!))
            .Should().Throw<ArgumentNullException>();
    }

    private PSCmdlet GetACmdlet()
    {
        using var ps = Sma.PowerShell.Create(RunspaceMode.CurrentRunspace);

        return ps
            .AddScript("function f { [CmdletBinding()] param() $PSCmdlet }; f")
            .Invoke<PSCmdlet>()
            .First();
    }

    [Test]
    public void UseTask_NullCmdlet()
    {
        using var invocation = new Invocation();

        invocation
            .Invoking(i => i.UseTask(default!))
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
    public void AddReinvocation_NullInvocationInfo()
    {
        using var invocation = new Invocation();

        invocation
            .Invoking(i => i.AddReinvocation(default!))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void AddReinvocation_Normal()
    {
        _runspace.Open();

        using var invocation = new Invocation();

        var info = InvocationInfo.Create(
            new CmdletInfo("Set-Variable", typeof(SetVariableCommand)),
            Mock.Of<IScriptExtent>()
        );

        info.BoundParameters .Add("Name",  "TestVariable");
        info.BoundParameters .Add("Scope", "global");
        info.UnboundArguments.Add("TestValue");

        invocation.AddReinvocation(info).Invoke();

        _runspace.SessionStateProxy.GetVariable("TestVariable").Should().Be("TestValue");
    }

#if ASYNC_SUPPORT
    // Waiting on PowerShell support.  Currently, InvokeAsnc throws:
    // 
    //   PSInvalidOperationException: Nested PowerShell instances cannot be
    //   invoked asynchronously. Use the Invoke method.

    [Test]
    public async Task InvokeAsync()
    {
        using var invocation = new Invocation();

        invocation.AddScript(ScriptBlock.Create("$global:X = 42"));

        await invocation.InvokeAsync();
    }
#endif

    [Test]
    public void Dispose_Unmanaged()
    {
        using var invocation = new Invocation();

        invocation.SimulateFinalizer(); // does nothing
    }
}
