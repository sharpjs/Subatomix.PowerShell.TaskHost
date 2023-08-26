// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

[TestFixture]
public class RedirectorTests : RedirectorTestsBase
{
    public RedirectorTests()
    {
        new Redirector(Output, Streams, Cmdlet);
    }

    [Test]
    public void Construct_NullOutput()
    {
        Invoking(() => new Redirector(null!, Streams, Cmdlet))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Construct_NullStreams()
    {
        Invoking(() => new Redirector(Output, null!, Cmdlet))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Construct_NullCmdlet()
    {
        Invoking(() => new Redirector(Output, Streams, null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void WriteProgress()
    {
        var record = new ProgressRecord(0, "test", "test");

        Runtime
            .Setup(r => r.WriteProgress(record))
            .Verifiable();

        Streams.Progress.Add(record);
    }
}
