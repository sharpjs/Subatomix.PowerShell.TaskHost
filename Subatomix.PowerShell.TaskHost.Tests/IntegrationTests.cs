// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Globalization;
using System.Text.RegularExpressions;

namespace Subatomix.PowerShell.TaskHost;

[TestFixture]
[NonParallelizable]
public class IntegrationTests : TestHarnessBase
{
    private readonly Mock<PSHost>                 _host;
    private readonly Mock<PSHostUserInterface>    _ui;
    private readonly Mock<PSHostRawUserInterface> _rawUI;

    public IntegrationTests()
    {
        _host  = Mocks.Create<PSHost>();
        _ui    = Mocks.Create<PSHostUserInterface>();
        _rawUI = Mocks.Create<PSHostRawUserInterface>();

        _ui  .Setup(u => u.RawUI).Returns(_rawUI.Object);
        _host.Setup(h => h.Name) .Returns(TestContext.CurrentContext.Test.Name);
        _host.Setup(h => h.UI)   .Returns(_ui.Object);
    }

    [Test]
    public void Minimal()
    {
        var (output, exception) = ScriptExecutor.Execute(
            _host.Object,
            """
            Use-TaskHost {
                Invoke-Task "" { }
            }
            """
        );

        output   .Should().BeEmpty();
        exception.Should().BeNull();
    }

    [Test]
    public void Maximal()
    {
        var (output, exception) = ScriptExecutor.Execute(
            _host.Object,
            """
            Import-Module .\TestModuleA.psm1
            Import-Module .\TestModuleB.psm1
            $TestVarA = 42
            $TestVarB = "eh"

            Use-TaskHost -WithElapsed {
                Invoke-Task TaskX {
                    Use-TestModuleA
                    Use-TestModuleB
                    $TestVarA
                    $TestVarB
                }
            }
            """
        );

        output.Should().BeEquivalentTo(new[]
        {
            new PSObject("TestModuleA output"),
            new PSObject("TestModuleB output"),
            new PSObject(42 as object),
            new PSObject("eh")
        });

        exception.Should().BeNull();
    }

    [Test]
    public void NestedUseTaskHost()
    {
        var (output, exception) = ScriptExecutor.Execute(
            _host.Object,
            """
            Use-TaskHost -WithElapsed {
                Use-TaskHost -WithElapsed {
                    Invoke-Task Test { "foo" }
                }
            }
            """
        );

        output.Should().BeEquivalentTo(new[]
        {
            new PSObject("foo")
        });

        exception.Should().BeNull();
    }

    [Test]
    public void Bypassed()
    {
        var (output, exception) = ScriptExecutor.Execute(
            _host.Object,
            """
            $TaskHostBypass = $true # anything except $null or $false
            Use-TaskHost {
                Invoke-Task Test { "foo" }
            }
            """
        );

        output.Should().BeEquivalentTo(new[]
        {
            new PSObject("foo")
        });

        exception.Should().BeNull();
    }

    [Test]
    public void NullUI()
    {
        var warning = null as string;

        _ui.Setup(u => u.WriteWarningLine(It.IsAny<string>()))
            .Callback((string s) => warning = s);

        var (output, exception) = ScriptExecutor.Execute(
            _host.Object,
            """
            1 | ForEach-Object -Parallel {
                Import-Module .\TaskHost.psd1
                Use-TaskHost { }
            }
            """
        );

        output   .Should().BeEmpty();
        exception.Should().BeNull();
        warning  .Should().Be("TaskHost is bypassed because $Host.UI is null.");
    }

    [Test]
    public void Invocation_UseTaskHost_Twice()
    {
        var (output, exception) = ScriptExecutor.Execute(
            _host.Object,
            """
            function f {
                [CmdletBinding()] param ()
                process {
                    $i = [Subatomix.PowerShell.TaskHost.Invocation]::new()
                    try {
                        $i.UseTaskHost($PSCmdlet, $true) > $null
                        $i.UseTaskHost($PSCmdlet, $true) > $null # Should throw
                    }
                    finally {
                        $i.Dispose()
                    }
                }
            }
            f
            """
        );

        output   .Should().BeEmpty();
        exception.Should().BeOfType<MethodInvocationException>()
            .Which.InnerException.Should().BeOfType<InvalidOperationException>()
            .Which.Message       .Should().Match("The invocation has been configured*");
            
    }

    [Test]
    public void Invocation_UseTask_Twice()
    {
        var (output, exception) = ScriptExecutor.Execute(
            _host.Object,
            """
            function f {
                [CmdletBinding()] param ()
                process {
                    $i = [Subatomix.PowerShell.TaskHost.Invocation]::new()
                    try {
                        $i.UseTask($PSCmdlet, "A") > $null
                        $i.UseTask($PSCmdlet, "B") > $null # Should throw
                    }
                    finally {
                        $i.Dispose()
                    }
                }
            }
            f
            """
        );

        output   .Should().BeEmpty();
        exception.Should().BeOfType<MethodInvocationException>()
            .Which.InnerException.Should().BeOfType<InvalidOperationException>()
            .Which.Message       .Should().Match("The invocation has been configured*");
            
    }
}
