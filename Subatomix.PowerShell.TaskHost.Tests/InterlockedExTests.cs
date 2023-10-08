// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

[TestFixture]
[NonParallelizable]
public class InterlockedExTests
{
    [Test]
    [TestCase( 1, 1)]
    [TestCase( 0, 0)]
    [TestCase(-1, 0)]
    public void DecrementZeroSaturating(long value, long expected)
    {
        const long Iterations = 256;

        value += Iterations;

        Parallel.For(0, Iterations, _ => InterlockedEx.DecrementZeroSaturating(ref value));

        value.Should().Be(expected);
    }

}
