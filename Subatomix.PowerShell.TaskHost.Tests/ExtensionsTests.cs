// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

[TestFixture]
public class ExtensionsTests
{
    [Test]
    public void Sanitize_Null()
    {
        (null as string?[]).Sanitize().Should().BeEmpty();
    }

    [Test]
    public void Sanitize_Empty()
    {
        var array = new string?[0]; // Do not refactor to use Array.Empty

        array.Sanitize().Should().BeSameAs(array);
    }

    [Test]
    public void Sanitize_Normal()
    {
        var array = new string?[] { "a", "b" };

        array.Sanitize().Should().BeSameAs(array);
    }

    [Test]
    public void Sanitize_NullElement()
    {
        var array = new string?[] { "a", null, "b" };

        array.Sanitize().Should().Equal("a", "b");
    }
}
