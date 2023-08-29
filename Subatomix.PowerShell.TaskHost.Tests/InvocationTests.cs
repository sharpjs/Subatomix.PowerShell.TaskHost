// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

[TestFixture]
internal class InvocationTests
{
    // Tests omitted where coverage provided by IntegrationTests

    [Test]
    public void Construct_NullRunspaceName()
    {
        Invoking(() => new Invocation(null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void ImportModules_NullModuleInfoArray()
    {
        using var invocation = CreateInvocation();

        invocation
            .Invoking(i => i.ImportModules(default!))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void ImportModule_NullModuleInfo()
    {
        using var invocation = CreateInvocation();

        invocation
            .Invoking(i => i.ImportModule(default(PSModuleInfo)!))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void ImportModule_NullModuleName()
    {
        using var invocation = CreateInvocation();

        invocation
            .Invoking(i => i.ImportModule(default(string)!))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void SetVariables_NullVariableInfoArray()
    {
        using var invocation = CreateInvocation();

        invocation
            .Invoking(i => i.SetVariables(default!))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void SetVariable_NullVariableInfo()
    {
        using var invocation = CreateInvocation();

        invocation
            .Invoking(i => i.SetVariable(default!))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void AddScript_NullScriptBlock()
    {
        using var invocation = CreateInvocation();

        invocation
            .Invoking(i => i.AddScript(default!))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Dispose_Unmanaged()
    {
        using var invocation = CreateInvocation();

        invocation.SimulateFinalizer(); // does nothing
    }

    private static Invocation CreateInvocation()
    {
        return new Invocation(TestContext.CurrentContext.Test.Name);
    }
}
