// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

internal class TaskHostTestHarness : TestHarnessBase
{
    public TaskHost Host2 { get; }

    public Mock<PSHost>                 Host  { get; }
    public Mock<PSHostUserInterface>    UI    { get; }
    public Mock<PSHostRawUserInterface> RawUI { get; }

    public TaskHostTestHarness(bool withElapsed = false)
    {
        Host  = Mocks.Create<PSHost>();
        UI    = Mocks.Create<PSHostUserInterface>();
        RawUI = Mocks.Create<PSHostRawUserInterface>();

        UI.Setup(u => u.RawUI).Returns(RawUI.Object);

        Host.Setup(h => h.Name).Returns("MockHost");
        Host.Setup(h => h.UI  ).Returns(UI.Object);

        Host2 = new TaskHost(Host.Object);
    }
}
