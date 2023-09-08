// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

[TestFixture]
public class OutDefaultWriterTests : TestHarnessBase
{
    private OutDefaultWriter             Writer { get; }
    private Mock<PSHost>                 Host   { get; }
    private Mock<PSHostUserInterface>    UI     { get; }
    private Mock<PSHostRawUserInterface> RawUI  { get; }

    private readonly Runspace _runspace;

    public OutDefaultWriterTests()
    {
        RawUI = Mocks.Create<PSHostRawUserInterface>();

        UI = Mocks.Create<PSHostUserInterface>();
        UI.Setup(u => u.RawUI).Returns(RawUI.Object);

        Host = Mocks.Create<PSHost>();
        Host.Setup(h => h.UI)        .Returns(UI.Object);
        Host.Setup(h => h.InstanceId).Returns(Guid.NewGuid());
        Host.Setup(h => h.Name)      .Returns(nameof(OutDefaultWriterTests));
        Host.Setup(h => h.Version)   .Returns(new Version(0, 0, 0, 0));

        _runspace = RunspaceFactory.CreateRunspace(Host.Object);
        _runspace.Open();
        Runspace.DefaultRunspace = _runspace;

        Writer = new OutDefaultWriter();
    }

    protected override void CleanUp(bool managed)
    {
        Writer.Dispose();

        Runspace.DefaultRunspace = null;
        _runspace.Dispose();

        base.CleanUp(managed);
    }

    [Test]
    public void WaitHandle_Get()
    {
        Writer.WaitHandle.Should().NotBeNull();
    }

    [Test]
    public void IsOpen_Get()
    {
        Writer.IsOpen.Should().BeTrue();
    }

    [Test]
    public void Count_Get()
    {
        Writer.Count.Should().Be(0);
    }

    [Test]
    public void MaxCapacity_Get()
    {
        Writer.MaxCapacity.Should().Be(int.MaxValue);
    }

    [Test]
    public void Flush()
    {
        Writer.Flush(); // does nothing
    }

    [Test]
    public void Close()
    {
        Writer.Close();

        Writer.IsOpen.Should().BeFalse();
    }

    [Test]
    public void Dispose_Managed()
    {
        Writer.Dispose();

        Writer.IsOpen.Should().BeFalse();
    }

    [Test]
    public void Write1()
    {
        UI.Setup(u => u.WriteLine("foo")).Verifiable();

        var count = Writer.Write("foo");

        count.Should().Be(1);
    }

    [Test]
    public void Write2_EnumerateFalse()
    {
        // Out-Default will enumerate its input regardless of enumerateCollection=false
        UI.Setup(u => u.WriteLine("foo")).Verifiable();
        UI.Setup(u => u.WriteLine("bar")).Verifiable();

        var count = Writer.Write(new[] { "foo", "bar" }, enumerateCollection: false);

        count.Should().Be(1);
    }

    [Test]
    public void Write2_EnumerateTrue()
    {
        UI.Setup(u => u.WriteLine("foo")).Verifiable();
        UI.Setup(u => u.WriteLine("bar")).Verifiable();

        var count = Writer.Write(new[] { "foo", "bar" }, enumerateCollection: true);

        count.Should().Be(2);
    }

    [Test]
    public void Write2_NonEnumerable()
    {
        UI.Setup(u => u.WriteLine("foo")).Verifiable();

        var count = Writer.Write("foo", enumerateCollection: true);

        count.Should().Be(1);
    }

    [Test]
    public void WriteN_Multiple()
    {
        UI.Setup(u => u.WriteLine("a")).Verifiable();
        UI.Setup(u => u.WriteLine("b")).Verifiable();
        UI.Setup(u => u.WriteLine("c")).Verifiable();
        UI.Setup(u => u.WriteLine("d")).Verifiable();

        var count0 = Writer.Write("a");
        var count1 = Writer.Write(new[] { "b", "c" }, enumerateCollection: true);
        var count2 = Writer.Write("d");

        count0.Should().Be(1);
        count1.Should().Be(2);
        count2.Should().Be(1);
    }
}
