// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

internal static class TestExtensions
{
    internal static void AssignTo<TParent, TMatched>(
        this AndWhichConstraint<TParent, TMatched> constraint,
        out TMatched slot)
    {
        slot = constraint.Which;
    }

    internal static void ReleaseAll(this TaskInfo task)
    {
        while (task.RetainCount > 0)
            task.Release();
    }
}
