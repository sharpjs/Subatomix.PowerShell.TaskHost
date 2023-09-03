// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Security.AccessControl;

namespace Subatomix.PowerShell.TaskHost;

internal sealed class HostWriter : PipelineWriter, IDisposable
{
    private readonly PSHost                    _host;
    private readonly ManualResetEvent          _event;
    private readonly PSDataCollection<object?> _empty;

    private bool _isOpen;

    public HostWriter(PSHost host)
    {
        if (host is null)
            throw new ArgumentNullException(nameof(host));

        _host   = host;
        _event  = new(initialState: true);
        _empty  = CreateEmptyReadOnlyPSDataCollection();
        _isOpen = true;
    }

    public override WaitHandle WaitHandle  => _event;
    public override bool       IsOpen      => _isOpen;
    public override int        Count       => 0;
    public override int        MaxCapacity => int.MaxValue;

    public override int Write(object? obj)
    {
        var input = new PSDataCollection<object?>(capacity: 1) { obj };
        input.Complete();

        return WriteCore(obj);
    }

    public override int Write(object? obj, bool enumerateCollection)
    {
        if (!enumerateCollection)
            return Write(obj);

        if (LanguagePrimitives.GetEnumerable(obj) is not { } enumerable)
            return Write(obj);

        var input = new PSDataCollection<object?>(enumerable.Cast<object?>());

        return WriteCore(input);
    }

    private int WriteCore(PSDataCollection<object?> input)
    {
        var count    = input.Count;
        var settings = new PSInvocationSettings()
        {
            ErrorActionPreference = ActionPreference.Ignore,
            Host                  = _host,
        };

        using var shell = Sma.PowerShell.Create(RunspaceMode.CurrentRunspace);

        shell
            .AddCommand("Out-Default")
            .Invoke(input, output: _empty, settings);

        return count;
    }

    public override void Flush() { }

    public override void Close()
    {
        _isOpen = false;
    }

    public void Dispose()
    {
        Close();
        _event.Dispose();
    }

    private static PSDataCollection<object?> CreateEmptyReadOnlyPSDataCollection()
    {
        // Implicit conversion operator returns an empty, read-only PSDC
        return default(object?[]?);
    }
}
