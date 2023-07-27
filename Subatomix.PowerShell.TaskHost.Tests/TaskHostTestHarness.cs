// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

internal class TaskHostTestHarness : TestHarnessBase
{
    public TaskHostFactory Factory { get; }

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

        Factory = new(Host.Object, withElapsed);
    }
}
