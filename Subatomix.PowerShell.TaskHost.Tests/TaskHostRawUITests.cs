// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class TaskHostRawUITests : TestHarnessBase
{
    private readonly Mock<PSHostRawUserInterface> _inner;
    private readonly TaskHostRawUI                _rawUI;

    public TaskHostRawUITests()
    {
        _inner = Mocks.Create<PSHostRawUserInterface>();
        _rawUI = new TaskHostRawUI(_inner.Object, new());
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
        Invoking(() => new TaskHostRawUI(_inner.Object, null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void ForegroundColor_Get()
    {
        var value = ConsoleColor.DarkBlue;

        _inner.SetupGet(u => u.ForegroundColor).Returns(value).Verifiable();

        _rawUI.ForegroundColor.Should().Be(value);
    }

    [Test]
    public void ForegroundColor_Set()
    {
        var value = ConsoleColor.DarkBlue;

        _inner.SetupSet(u => u.ForegroundColor = value).Verifiable();

        _rawUI.ForegroundColor = value;
    }

    [Test]
    public void BackgroundColor_Get()
    {
        var value = ConsoleColor.DarkBlue;

        _inner.SetupGet(u => u.BackgroundColor).Returns(value).Verifiable();

        _rawUI.BackgroundColor.Should().Be(value);
    }

    [Test]
    public void BackgroundColor_Set()
    {
        var value = ConsoleColor.DarkBlue;

        _inner.SetupSet(u => u.BackgroundColor = value).Verifiable();

        _rawUI.BackgroundColor = value;
    }

    [Test]
    public void CursorPosition_Get()
    {
        var value = new Coordinates(1, 1);

        _inner.SetupGet(u => u.CursorPosition).Returns(value).Verifiable();

        _rawUI.CursorPosition.Should().Be(value);
    }

    [Test]
    public void CursorPosition_Set()
    {
        var value = new Coordinates(1, 1);

        _inner.SetupSet(u => u.CursorPosition = value).Verifiable();

        _rawUI.CursorPosition = value;
    }

    [Test]
    public void CursorSize_Get()
    {
        var value = 3;

        _inner.SetupGet(u => u.CursorSize).Returns(value).Verifiable();

        _rawUI.CursorSize.Should().Be(value);
    }

    [Test]
    public void CursorSize_Set()
    {
        var value = 3;

        _inner.SetupSet(u => u.CursorSize = value).Verifiable();

        _rawUI.CursorSize = value;
    }

    [Test]
    public void WindowPosition_Get()
    {
        var value = new Coordinates(1, 1);

        _inner.SetupGet(u => u.WindowPosition).Returns(value).Verifiable();

        _rawUI.WindowPosition.Should().Be(value);
    }

    [Test]
    public void WindowPosition_Set()
    {
        var value = new Coordinates(1, 1);

        _inner.SetupSet(u => u.WindowPosition = value).Verifiable();

        _rawUI.WindowPosition = value;
    }

    [Test]
    public void WindowSize_Get()
    {
        var value = new Size(80, 25);

        _inner.SetupGet(u => u.WindowSize).Returns(value).Verifiable();

        _rawUI.WindowSize.Should().Be(value);
    }

    [Test]
    public void WindowSize_Set()
    {
        var value = new Size(42, 42);

        _inner.SetupSet(u => u.WindowSize = value).Verifiable();

        _rawUI.WindowSize = value;
    }

    [Test]
    public void MaxWindowSize_Get()
    {
        var value = new Size(80, 25);

        _inner.SetupGet(u => u.MaxWindowSize).Returns(value).Verifiable();

        _rawUI.MaxWindowSize.Should().Be(value);
    }

    [Test]
    public void MaxPhysicalWindowSize_Get()
    {
        var value = new Size(80, 25);

        _inner.SetupGet(u => u.MaxPhysicalWindowSize).Returns(value).Verifiable();

        _rawUI.MaxPhysicalWindowSize.Should().Be(value);
    }

    [Test]
    public void WindowTitle_Get()
    {
        var value = "a";

        _inner.SetupGet(u => u.WindowTitle).Returns(value).Verifiable();

        _rawUI.WindowTitle.Should().Be(value);
    }

    [Test]
    public void WindowTitle_Set()
    {
        var value = "a";

        _inner.SetupSet(u => u.WindowTitle = value).Verifiable();

        _rawUI.WindowTitle = value;
    }

    [Test]
    public void BufferSize_Get()
    {
        var value = new Size(42, 42);

        _inner.SetupGet(u => u.BufferSize).Returns(value).Verifiable();

        _rawUI.BufferSize.Should().Be(value);
    }

    [Test]
    public void BufferSize_Set()
    {
        var value = new Size(42, 42);

        _inner.SetupSet(u => u.BufferSize = value).Verifiable();

        _rawUI.BufferSize = value;
    }

    [Test]
    public void KeyAvailable_Get()
    {
        var value = true;

        _inner.SetupGet(u => u.KeyAvailable).Returns(value).Verifiable();

        _rawUI.KeyAvailable.Should().Be(value);
    }

    [Test]
    public void FlushInputBuffer()
    {
        _inner.Setup(u => u.FlushInputBuffer()).Verifiable();

        _rawUI.FlushInputBuffer();
    }

    [Test]
    public void GetBufferContents()
    {
        var rectangle = new Rectangle(1, 2, 3, 4);
        var contents  = new BufferCell[2, 2];

        _inner.Setup(u => u.GetBufferContents(rectangle)).Returns(contents).Verifiable();

        _rawUI.GetBufferContents(rectangle).Should().BeSameAs(contents);
    }

    [Test]
    public void ReadKey()
    {
        var options = ReadKeyOptions.AllowCtrlC;
        var keyInfo = new KeyInfo(42, 'a', ControlKeyStates.ShiftPressed, keyDown: true);

        _inner.Setup(u => u.ReadKey(options)).Returns(keyInfo).Verifiable();

        _rawUI.ReadKey(options).Should().Be(keyInfo);
    }

    [Test]
    public void SetBufferContents_RectangeAndFill()
    {
        var rectangle = new Rectangle(1, 2, 3, 4);
        var fill      = new BufferCell();

        _inner.Setup(u => u.SetBufferContents(rectangle, fill)).Verifiable();

        _rawUI.SetBufferContents(rectangle, fill);
    }

    [Test]
    public void SetBufferContents_OriginAndContents()
    {
        var origin   = new Coordinates(1, 2);
        var contents = new BufferCell[2, 2];

        _inner.Setup(u => u.SetBufferContents(origin, contents)).Verifiable();

        _rawUI.SetBufferContents(origin, contents);
    }

    [Test]
    public void ScrollBufferContents()
    {
        var source      = new Rectangle(1, 2, 3, 4);
        var destination = new Coordinates(1, 2);
        var clip        = new Rectangle(1, 1, 1, 1);
        var fill        = new BufferCell();

        _inner.Setup(u => u.ScrollBufferContents(source, destination, clip, fill)).Verifiable();

        _rawUI.ScrollBufferContents(source, destination, clip, fill);
    }

    [Test]
    public void LengthInBufferCells_Char()
    {
        var source = 'a';
        var result = 1;

        _inner.Setup(u => u.LengthInBufferCells(source)).Returns(result).Verifiable();

        _rawUI.LengthInBufferCells(source).Should().Be(result);
    }

    [Test]
    public void LengthInBufferCells_String()
    {
        var source = "a";
        var result = 1;

        _inner.Setup(u => u.LengthInBufferCells(source)).Returns(result).Verifiable();

        _rawUI.LengthInBufferCells(source).Should().Be(result);
    }

    [Test]
    public void LengthInBufferCells_StringAndOffset()
    {
        var source = "a";
        var offset = 0;
        var result = 1;

        _inner.Setup(u => u.LengthInBufferCells(source, offset)).Returns(result).Verifiable();

        _rawUI.LengthInBufferCells(source, offset).Should().Be(result);
    }
}
