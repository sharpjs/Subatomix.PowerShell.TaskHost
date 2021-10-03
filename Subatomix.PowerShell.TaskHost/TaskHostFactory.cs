using System;
using System.Management.Automation.Host;
using System.Threading;

namespace Subatomix.PowerShell.TaskHost
{
    /// <summary>
    ///   A factory to create <see cref="TaskHost"/> instances.
    /// </summary>
    public class TaskHostFactory
    {
        private readonly ConsoleState _console; // Overall console state
        private int                   _taskId;  // Counter for task IDs

        /// <summary>
        ///   Initializes a new <see cref="TaskHostFactory"/> instance.
        /// </summary>
        public TaskHostFactory()
        {
            _console = new();
        }

        /// <summary>
        ///   Gets the version of the <see cref="TaskHostFactory"/>
        ///   implementation.
        /// </summary>
        public static Version Version { get; }
            = typeof(TaskHost).Assembly.GetName().Version!;

        /// <summary>
        ///   Creates a new <see cref="TaskHost"/> instance wrapping the
        ///   specified PowerShell host, optionally with the specified header.
        /// </summary>
        /// <param name="host">
        ///   The 
        /// </param>
        /// <param name="header"></param>
        /// <returns></returns>
        public TaskHost Create(PSHost host, string? header = null)
        {
            if (host is null)
                throw new ArgumentNullException(nameof(host));
            if (host.UI is null)
                throw new ArgumentNullException(nameof(host) + ".UI");

            var taskId = Interlocked.Increment(ref _taskId);

            return new TaskHost(host, _console, taskId, header);
        }
    }
}
