using System;
using System.Management.Automation.Host;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Subatomix.PowerShell.TaskHost
{
    [TestFixture]
    public class TaskHostFactoryTests
    {
        [Test]
        public void Version_Get()
        {
            TaskHostFactory.Version
                .Should().NotBeNull().And.NotBe(new Version());
        }

        [Test]
        public void Create_NullHost()
        {
            using var my = new TaskHostTestHarness();

            my.Factory.Invoking(f => f.Create(null!))
                .Should().ThrowExactly<ArgumentNullException>()
                .Where(e => e.ParamName == "host");
        }

        [Test]
        public void Create_NullHostUI()
        {
            using var my = new TaskHostTestHarness();

            var host = Mock.Of<PSHost>(); // .UI == null

            my.Factory.Invoking(f => f.Create(host))
                .Should().ThrowExactly<ArgumentNullException>()
                .Where(e => e.ParamName == "host.UI");
        }
    }
}
