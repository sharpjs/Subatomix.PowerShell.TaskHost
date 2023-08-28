// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

[TestFixture]
public class IntegrationTests : TestHarnessBase
{
    [Test]
    public void Foo()
    {
        var host  = Mocks.Create<PSHost>();
        var ui    = Mocks.Create<PSHostUserInterface>();
        var rawUI = Mocks.Create<PSHostRawUserInterface>();

        ui  .Setup(u => u.RawUI).Returns(rawUI.Object);
        host.Setup(h => h.Name) .Returns(TestContext.CurrentContext.Test.Name);
        host.Setup(h => h.UI)   .Returns(ui.Object);

        var (output, exception) = ScriptExecutor.Execute(
            host.Object,
            """
            Use-TaskHost {
                Invoke-Task {
                }
            }
            """
        );

        output   .Should().BeEmpty();
        exception.Should().BeNull();
    }
}
