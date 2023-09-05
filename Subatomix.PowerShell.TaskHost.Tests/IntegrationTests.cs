// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

[TestFixture]
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

            Use-TaskHost -Module (Get-Module TestModule*) -Variable (Get-Variable TestVar*) {
                Invoke-Task TaskX -Module (Get-Module TestModule*) -Variable (Get-Variable TestVar*) {
                    Use-TestModuleA
                    Use-TestModuleB
                    $TestVarA
                    $TestVarB
                }
            }
            """
        );

        output   .Should().NotBeEmpty();
        exception.Should().BeNull();
    }
}
