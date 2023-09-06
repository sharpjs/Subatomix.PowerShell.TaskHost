// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Collections;

namespace Subatomix.PowerShell.TaskHost;

internal sealed class OutDefaultWriter : PipelineWriter, IDisposable
{
    private readonly Sma.PowerShell    _shell;
    private readonly SteppablePipeline _pipeline;
    private readonly ManualResetEvent  _event;

    private enum State { Initial, InPipeline, Closed }
    private State _state;

    public OutDefaultWriter()
    {
        _shell    = Sma.PowerShell.Create(RunspaceMode.CurrentRunspace).AddCommand("Out-Default");
        _pipeline = _shell.GetSteppablePipeline();
        _event    = new(initialState: true);
    }

    public override WaitHandle WaitHandle  => _event;
    public override bool       IsOpen      => _state < State.Closed;
    public override int        Count       => 0;
    public override int        MaxCapacity => int.MaxValue;

    public override int Write(object? obj)
    {
        Begin();
        _pipeline.Process(obj);
        return 1;
    }

    public override int Write(object? obj, bool enumerateCollection)
    {
        if (!enumerateCollection)
            return Write(obj);

        if (LanguagePrimitives.GetEnumerable(obj) is not { } enumerable)
            return Write(obj);

        return EnumerateAndWrite(enumerable);
    }

    private int EnumerateAndWrite(IEnumerable enumerable)
    {
        Begin();
        var count = 0;

        foreach (var item in enumerable)
        {
            _pipeline.Process(item);
            count++;
        }

        return count;
    }

    public override void Flush() { }

    private void Begin()
    {
        if (_state is not State.Initial)
            return;

        _pipeline.Begin(expectInput: true);
        _state = State.InPipeline;
    }

    public override void Close()
    {
        if (_state is State.InPipeline)
            _pipeline.End();

        _state = State.Closed;
    }

    public void Dispose()
    {
        Close();

        _event   .Dispose();
        _pipeline.Dispose();
        _shell   .Dispose();
    }
}
