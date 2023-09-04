// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

public static class TaskInfoTests
{
    public abstract class Invariant
    {
        protected TaskInfo  Task       { get; }
        protected TaskInfo? ParentTask { get; }

        public Invariant()
        {
            (Task, ParentTask) = CreateTask();
        }

        protected abstract (TaskInfo task, TaskInfo? parentTask) CreateTask();

        [Test]
        public void Current_Get()
        {
            TaskInfo.Current.Should().BeNull();
        }

        [Test]
        public void Parent_Get()
        {
            Task.Parent.Should().BeSameAs(ParentTask);
        }

        [Test]
        public void RetainCount_Get()
        {
            Task.RetainCount.Should().Be(0);
        }

        [Test]
        public void IsAtBol_Get()
        {
            Task.IsAtBol.Should().BeTrue();
        }

        [Test]
        public void IsAtBol_Set()
        {
            Task.IsAtBol = false;
            Task.IsAtBol.Should().BeFalse();
        }

        [Test]
        public void Name_Set_Null()
        {
            Task.Invoking(t => t.Name = null!)
                .Should().Throw<ArgumentNullException>();
        }
    }

    [TestFixture]
    public class Virtual : Invariant
    {
        // Also tests nonvirtual tasks with empty names

        protected override (TaskInfo task, TaskInfo? parentTask) CreateTask()
            => (new(), null);

        [Test]
        public void Id_Get()
        {
            Task.Id.Should().Be(-1);
        }

        [Test]
        public void Name_Get()
        {
            Task.Name.Should().BeEmpty().And.BeSameAs(Task.Name);
        }

        [Test]
        public void FullName_Get()
        {
            Task.FullName.Should().BeEmpty().And.BeSameAs(Task.FullName);
        }

        [Test]
        public void FormattedName_Get()
        {
            Task.FormattedName.Should().BeEmpty().And.BeSameAs(Task.FormattedName);
        }

        [Test]
        public void ToString_Invoke()
        {
            Task.ToString().Should().Be("#-1").And.BeSameAs(Task.ToString());
        }

        [Test]
        public void All_Get()
        {
            TaskInfo.All.Should().NotBeNull().And.NotContain(Task.Id, Task);
        }

        [Test]
        public void Get()
        {
            TaskInfo.Get(Task.Id).Should().BeNull();
        }
    }

    public abstract class Nonvirtual : Invariant
    {
        [Test]
        public void All_Get()
        {
            TaskInfo.All.Should().NotBeNull().And.Contain(Task.Id, Task);
        }

        [Test]
        public void Id_Get()
        {
            Task.Id.Should().BePositive();
        }

        [Test]
        public void Name_Set()
        {
            Task.Name = "tseT";
            Task.Name.Should().Be("tseT").And.BeSameAs(Task.Name);
        }

        [Test]
        public void Get()
        {
            TaskInfo.Get(Task.Id).Should().BeSameAs(Task);
        }

        [Test]
        public void Retain()
        {
            Task.Retain(); // retain count => 1

            Task.RetainCount     .Should().Be(1);
            TaskInfo.Get(Task.Id).Should().BeSameAs(Task);
            TaskInfo.All         .Should().Contain(Task.Id, Task);
        }

        [Test]
        public void Release()
        {
            Task.Release(); // retain count => -1; task removed from All

            Task.RetainCount     .Should().Be(-1);
            TaskInfo.Get(Task.Id).Should().BeNull();
            TaskInfo.All         .Should().NotContain(Task.Id, Task);
        }

        [Test]
        public void RetainThenRelease()
        {
            Task.Retain();  // retain count => 1
            Task.Release(); // retain count => 0

            Task.RetainCount     .Should().Be(0);
            TaskInfo.Get(Task.Id).Should().BeNull();
            TaskInfo.All         .Should().NotContain(Task.Id, Task);
        }

        [Test]
        public void ReleaseThenRetain()
        {
            Task.Release(); // retain count => -1; task removed from All
            Task.Retain();  // retain count =>  0
            Task.Retain();  // retain count => +1

            Task.RetainCount     .Should().Be(1);
            TaskInfo.Get(Task.Id).Should().BeNull();
            TaskInfo.All         .Should().NotContain(Task.Id, Task);
        }

        [TearDown]
        public void TearDown()
        {
            Task.ReleaseAll();
        }
    }

    [TestFixture]
    public class WithDefaultName : Invariant
    {
        protected override (TaskInfo task, TaskInfo? parentTask) CreateTask()
            => (new(name: null), null);

        [Test]
        public void Name_Get()
        {
            Task.Name.Should().MatchRegex("^Task [1-9][0-9]*$").And.BeSameAs(Task.Name);
        }
    }

    [TestFixture]
    public class WithCustomName : Invariant
    {
        protected override (TaskInfo task, TaskInfo? parentTask) CreateTask()
            => (new(name: "Test"), null);

        [Test]
        public void Name_Get()
        {
            Task.Name.Should().Be("Test").And.BeSameAs(Task.Name);
        }
    }

    [TestFixture]
    public class WithParent : Invariant
    {
        protected override (TaskInfo task, TaskInfo? parentTask) CreateTask()
        {
            var task       = default(TaskInfo);
            var parentTask = default(TaskInfo);
            try
            {
                parentTask = new("Parent");
                TaskInfo.Current = parentTask;
                task = new("Child");
                return (task, parentTask);
            }
            catch
            {
                task      ?.ReleaseAll();
                parentTask?.ReleaseAll();
                throw;
            }
            finally
            {
                TaskInfo.Current = null;
            }
        }

        [Test]
        public void Name_Get()
        {
            Task.Name.Should().Be("Child").And.BeSameAs(Task.Name);
        }

        [Test]
        public void FullName_Get()
        {
            Task.FullName.Should().Be("Parent|Child").And.BeSameAs(Task.FullName);
        }

        [Test]
        public void FormattedName_Get()
        {
            Task.FormattedName.Should().Be("[Parent|Child]: ").And.BeSameAs(Task.FormattedName);
        }

        [Test]
        public void ToString_Invoke()
        {
            Task.ToString()
                .Should().Be(Invariant($"#{Task.Id}: Parent|Child"))
                .And.BeSameAs(Task.ToString());
        }

        [Test]
        public void Name_Set()
        {
            Name_Get();
            FullName_Get();
            FormattedName_Get();

            Task.Name = "Changed";

            AssertNames("Changed", "Parent|Changed");
        }

        [Test]
        public void Name_Set_Empty()
        {
            Name_Get();
            FullName_Get();
            FormattedName_Get();

            Task.Name = "";

            AssertNames("", "Parent");
        }

        [Test]
        public void ParentName_Set()
        {
            Name_Get();
            FullName_Get();
            FormattedName_Get();

            ParentTask!.Name = "Changed";

            AssertNames("Child", "Changed|Child");
        }

        [Test]
        public void ParentName_Set_Empty()
        {
            Name_Get();
            FullName_Get();
            FormattedName_Get();

            ParentTask!.Name = "";

            AssertNames("Child", "Child");
        }

        private void AssertNames(string name, string fullName)
        {
            Task.Name    .Should().Be(name)    .And.BeSameAs(Task.Name);
            Task.FullName.Should().Be(fullName).And.BeSameAs(Task.FullName);

            Task.FormattedName
                .Should().Be(Invariant($"[{fullName}]: "))
                .And.BeSameAs(Task.FormattedName);

            Task.ToString()
                .Should().Be(Invariant($"#{Task.Id}: {fullName}"))
                .And.BeSameAs(Task.ToString());
        }
    }
}
