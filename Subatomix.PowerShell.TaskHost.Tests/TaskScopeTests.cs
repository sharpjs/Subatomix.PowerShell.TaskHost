// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

[TestFixture]
public class TaskScopeTests
{
    [Test]
    public void Construct_NullTask()
    {
        Invoking(() => new TaskScope(null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void ConstructAndDispose()
    {
        var task0 = new TaskInfo();
        var task1 = new TaskInfo();

        task0.RetainCount.Should().Be(0);
        task1.RetainCount.Should().Be(0);
        TaskInfo.Current .Should().BeNull();

        using (new TaskScope(task0))
        {
            task0.RetainCount.Should().Be(1);
            task1.RetainCount.Should().Be(0);
            TaskInfo.Current .Should().BeSameAs(task0);

            using (new TaskScope(task1))
            {
                task0.RetainCount.Should().Be(1);
                task1.RetainCount.Should().Be(1);
                TaskInfo.Current .Should().BeSameAs(task1);
            }

            task0.RetainCount.Should().Be(1);
            task1.RetainCount.Should().Be(0);
            TaskInfo.Current .Should().BeSameAs(task0);
        }

        task0.RetainCount.Should().Be(0);
        task1.RetainCount.Should().Be(0);
        TaskInfo.Current .Should().BeNull();
    }

    [Test]
    public void Begin_DefaultName()
    {
        using var scope = TaskScope.Begin();

        scope.Task            .Should().NotBeNull();
        scope.Task.Name       .Should().MatchRegex("^Task [1-9][0-9]*$");
        scope.Task.RetainCount.Should().Be(1);
        TaskInfo.Current      .Should().BeSameAs(scope.Task);
    }

    [Test]
    public void Begin_CustomName()
    {
        using var scope = TaskScope.Begin("Test");

        scope.Task            .Should().NotBeNull();
        scope.Task.Name       .Should().Be("Test");
        scope.Task.RetainCount.Should().Be(1);
        TaskInfo.Current      .Should().BeSameAs(scope.Task);
    }

    [Test]
    public void Task_Normal()
    {
        var task = new TaskInfo();

        using var scope = new TaskScope(task);

        scope.Task.Should().BeSameAs(task);
    }

    [Test]
    public void Dispose_Multiple()
    {
        var task  = new TaskInfo();
        var scope = new TaskScope(task);

        scope.Dispose();
        scope.Dispose(); // does othing

        task.RetainCount.Should().Be(0);
        TaskInfo.Current.Should().BeNull();
    }

    [Test]
    public void Dispose_OutOfOrder()
    {
        Invoking(() =>
        {
            using var scope0 = TaskScope.Begin();
            using var scope1 = TaskScope.Begin();

            scope0.Dispose(); // throws
        })
        .Should().Throw<InvalidOperationException>();
    }
}
