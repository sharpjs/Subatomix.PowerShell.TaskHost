/*
    Copyright 2021 Jeffrey Sharp

    Permission to use, copy, modify, and distribute this software for any
    purpose with or without fee is hereby granted, provided that the above
    copyright notice and this permission notice appear in all copies.

    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
    WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
    MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
    ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
    WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
    ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
    OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
*/

using System;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Host;

namespace Subatomix.PowerShell.TaskHost
{
    using static FormattableString;

    public class TaskHost : PSHost
    {
        private readonly PSHost     _host;  // Underlying host implementation
        private readonly TaskHostUI _ui;    // Wrapped UI implementation
        private readonly Guid       _id;    // Host identifier (random)
        private readonly string     _name;  // Host name

        internal TaskHost(PSHost host, ConsoleState state, int taskId, string? header)
        {
            _host = host;
            _ui   = new TaskHostUI(host.UI, state, taskId, header);
            _id   = Guid.NewGuid();
            _name = Invariant($"TaskHost<{host.Name}>#{taskId}");
        }

        public override Guid InstanceId
            => _id;

        public override string Name
            => _name;

        public override Version Version
            => TaskHostFactory.Version;

        public override PSHostUserInterface UI
            => _ui;

        public override CultureInfo CurrentCulture
            => _host.CurrentCulture;

        public override CultureInfo CurrentUICulture
            => _host.CurrentUICulture;

        public override PSObject PrivateData
            => _host.PrivateData;

        public override bool DebuggerEnabled
        {
            get => _host.DebuggerEnabled;
            set => _host.DebuggerEnabled = value;
        }

        public override void EnterNestedPrompt()
            => _host.EnterNestedPrompt();

        public override void ExitNestedPrompt()
            => _host.ExitNestedPrompt();

        public override void NotifyBeginApplication()
            => _host.NotifyBeginApplication();

        public override void NotifyEndApplication()
            => _host.NotifyEndApplication();

        public override void SetShouldExit(int exitCode)
            => _host.SetShouldExit(exitCode);
    }
}
