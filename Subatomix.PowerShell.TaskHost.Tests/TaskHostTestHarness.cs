using System.Management.Automation.Host;
using Moq;
using Subatomix.Testing;

namespace Subatomix.PowerShell.TaskHost
{
    internal class TaskHostTestHarness : TestHarnessBase
    {
        public TaskHostFactory Factory { get; }

        public Mock<PSHost>              Host { get; }
        public Mock<PSHostUserInterface> UI   { get; }

        public TaskHostTestHarness()
        {
            Factory = new();

            Host = Mocks.Create<PSHost>();
            UI   = Mocks.Create<PSHostUserInterface>();

            Host.Setup(h => h.Name).Returns("MockHost");
            Host.Setup(h => h.UI  ).Returns(UI.Object);
        }
    }
}
