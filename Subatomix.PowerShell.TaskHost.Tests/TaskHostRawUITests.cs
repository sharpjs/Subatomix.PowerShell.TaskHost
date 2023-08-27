// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class TaskHostRawUITests : TestHarnessBase
{
    private TaskHostRawUI                RawUI      { get; }
    private Mock<PSHostRawUserInterface> InnerRawUI { get; }

    public TaskHostRawUITests()
    {
        InnerRawUI = Mocks.Create<PSHostRawUserInterface>();
        RawUI      = new TaskHostRawUI(InnerRawUI.Object, new());
    }

    [Test]
    public void Construct_NullUI()
    {
        Invoking(() => new TaskHostRawUI(null!, new()))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Construct_NullLockable()
    {
        Invoking(() => new TaskHostRawUI(InnerRawUI.Object, null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void ForegroundColor_Get()
    {
        var value = ConsoleColor.DarkBlue;

        InnerRawUI.SetupGet(u => u.ForegroundColor).Returns(value).Verifiable();

        RawUI.ForegroundColor.Should().Be(value);
    }

    [Test]
    public void ForegroundColor_Set()
    {
        var value = ConsoleColor.DarkBlue;

        InnerRawUI.SetupSet(u => u.ForegroundColor = value).Verifiable();

        RawUI.ForegroundColor = value;
    }

    [Test]
    public void BackgroundColor_Get()
    {
        var value = ConsoleColor.DarkBlue;

        InnerRawUI.SetupGet(u => u.BackgroundColor).Returns(value).Verifiable();

        RawUI.BackgroundColor.Should().Be(value);
    }

    [Test]
    public void BackgroundColor_Set()
    {
        var value = ConsoleColor.DarkBlue;

        InnerRawUI.SetupSet(u => u.BackgroundColor = value).Verifiable();

        RawUI.BackgroundColor = value;
    }

    [Test]
    public void CursorPosition_Get()
    {
        var value = new Coordinates(1, 1);

        InnerRawUI.SetupGet(u => u.CursorPosition).Returns(value).Verifiable();

        RawUI.CursorPosition.Should().Be(value);
    }

    [Test]
    public void CursorPosition_Set()
    {
        var value = new Coordinates(1, 1);

        InnerRawUI.SetupSet(u => u.CursorPosition = value).Verifiable();

        RawUI.CursorPosition = value;
    }

    [Test]
    public void CursorSize_Get()
    {
        var value = 3;

        InnerRawUI.SetupGet(u => u.CursorSize).Returns(value).Verifiable();

        RawUI.CursorSize.Should().Be(value);
    }

    [Test]
    public void CursorSize_Set()
    {
        var value = 3;

        InnerRawUI.SetupSet(u => u.CursorSize = value).Verifiable();

        RawUI.CursorSize = value;
    }

    [Test]
    public void WindowPosition_Get()
    {
        var value = new Coordinates(1, 1);

        InnerRawUI.SetupGet(u => u.WindowPosition).Returns(value).Verifiable();

        RawUI.WindowPosition.Should().Be(value);
    }

    [Test]
    public void WindowPosition_Set()
    {
        var value = new Coordinates(1, 1);

        InnerRawUI.SetupSet(u => u.WindowPosition = value).Verifiable();

        RawUI.WindowPosition = value;
    }

    [Test]
    public void WindowSize_Get()
    {
        var value = new Size(80, 25);

        InnerRawUI.SetupGet(u => u.WindowSize).Returns(value).Verifiable();

        RawUI.WindowSize.Should().Be(value);
    }

    [Test]
    public void WindowSize_Set()
    {
        var value = new Size(42, 42);

        InnerRawUI.SetupSet(u => u.WindowSize = value).Verifiable();

        RawUI.WindowSize = value;
    }

    [Test]
    public void MaxWindowSize_Get()
    {
        var value = new Size(80, 25);

        InnerRawUI.SetupGet(u => u.MaxWindowSize).Returns(value).Verifiable();

        RawUI.MaxWindowSize.Should().Be(value);
    }

    [Test]
    public void MaxPhysicalWindowSize_Get()
    {
        var value = new Size(80, 25);

        InnerRawUI.SetupGet(u => u.MaxPhysicalWindowSize).Returns(value).Verifiable();

        RawUI.MaxPhysicalWindowSize.Should().Be(value);
    }

    [Test]
    public void WindowTitle_Get()
    {
        var value = "a";

        InnerRawUI.SetupGet(u => u.WindowTitle).Returns(value).Verifiable();

        RawUI.WindowTitle.Should().Be(value);
    }

    [Test]
    public void WindowTitle_Set()
    {
        var value = "a";

        InnerRawUI.SetupSet(u => u.WindowTitle = value).Verifiable();

        RawUI.WindowTitle = value;
    }

    [Test]
    public void BufferSize_Get()
    {
        var value = new Size(42, 42);

        InnerRawUI.SetupGet(u => u.BufferSize).Returns(value).Verifiable();

        RawUI.BufferSize.Should().Be(value);
    }

    [Test]
    public void BufferSize_Set()
    {
        var value = new Size(42, 42);

        InnerRawUI.SetupSet(u => u.BufferSize = value).Verifiable();

        RawUI.BufferSize = value;
    }

    [Test]
    public void KeyAvailable_Get()
    {
        var value = true;

        InnerRawUI.SetupGet(u => u.KeyAvailable).Returns(value).Verifiable();

        RawUI.KeyAvailable.Should().Be(value);
    }

    [Test]
    public void FlushInputBuffer()
    {
        InnerRawUI.Setup(u => u.FlushInputBuffer()).Verifiable();

        RawUI.FlushInputBuffer();
    }

    [Test]
    public void GetBufferContents()
    {
        var rectangle = new Rectangle(1, 2, 3, 4);
        var contents  = new BufferCell[2, 2];

        InnerRawUI.Setup(u => u.GetBufferContents(rectangle)).Returns(contents).Verifiable();

        RawUI.GetBufferContents(rectangle).Should().BeSameAs(contents);
    }

    [Test]
    public void ReadKey()
    {
        var options = ReadKeyOptions.AllowCtrlC;
        var keyInfo = new KeyInfo(42, 'a', ControlKeyStates.ShiftPressed, keyDown: true);

        InnerRawUI.Setup(u => u.ReadKey(options)).Returns(keyInfo).Verifiable();

        RawUI.ReadKey(options).Should().Be(keyInfo);
    }

    [Test]
    public void SetBufferContents_RectangeAndFill()
    {
        var rectangle = new Rectangle(1, 2, 3, 4);
        var fill      = new BufferCell();

        InnerRawUI.Setup(u => u.SetBufferContents(rectangle, fill)).Verifiable();

        RawUI.SetBufferContents(rectangle, fill);
    }

    [Test]
    public void SetBufferContents_OriginAndContents()
    {
        var origin   = new Coordinates(1, 2);
        var contents = new BufferCell[2, 2];

        InnerRawUI.Setup(u => u.SetBufferContents(origin, contents)).Verifiable();

        RawUI.SetBufferContents(origin, contents);
    }

    [Test]
    public void ScrollBufferContents()
    {
        var source      = new Rectangle(1, 2, 3, 4);
        var destination = new Coordinates(1, 2);
        var clip        = new Rectangle(1, 1, 1, 1);
        var fill        = new BufferCell();

        InnerRawUI.Setup(u => u.ScrollBufferContents(source, destination, clip, fill)).Verifiable();

        RawUI.ScrollBufferContents(source, destination, clip, fill);
    }

    [Test]
    public void LengthInBufferCells_Char()
    {
        var source = 'a';
        var result = 1;

        InnerRawUI.Setup(u => u.LengthInBufferCells(source)).Returns(result).Verifiable();

        RawUI.LengthInBufferCells(source).Should().Be(result);
    }

    [Test]
    public void LengthInBufferCells_String()
    {
        var source = "a";
        var result = 1;

        InnerRawUI.Setup(u => u.LengthInBufferCells(source)).Returns(result).Verifiable();

        RawUI.LengthInBufferCells(source).Should().Be(result);
    }

    [Test]
    public void LengthInBufferCells_StringAndOffset()
    {
        var source = "a";
        var offset = 0;
        var result = 1;

        InnerRawUI.Setup(u => u.LengthInBufferCells(source, offset)).Returns(result).Verifiable();

        RawUI.LengthInBufferCells(source, offset).Should().Be(result);
    }
}
