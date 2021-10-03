using FluentAssertions;

namespace Subatomix.PowerShell.TaskHost
{
    internal static class TestExtensions
    {
        internal static void AssignTo<TParent, TMatched>(
            this AndWhichConstraint<TParent, TMatched> constraint,
            out TMatched slot)
        {
            slot = constraint.Which;
        }
    }
}
