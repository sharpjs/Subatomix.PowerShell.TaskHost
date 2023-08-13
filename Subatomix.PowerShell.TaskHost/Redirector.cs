// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.PowerShell.TaskHost;

/// <summary>
///   A utility to capture the output of a nested <see cref="PowerShell"/>
///   instance and to reissue the output from a particular cmdlet.
/// </summary>
internal abstract class Redirector
{
    private readonly PSDataCollection<PSObject?> _output;
    private readonly PSDataStreams               _streams;
    private readonly Cmdlet                      _target;

    /// <summary>
    ///   Initializes a new <see cref="Redirector"/> instance.
    /// </summary>
    /// <param name="output">
    ///   The output (success) stream of the nested <see cref="PowerShell"/>
    ///   instance from which to capture output.
    /// </param>
    /// <param name="streams">
    ///   The auxiliary streams of the <see cref="PowerShell"/> instance from
    ///   which to capture output.
    /// </param>
    /// <param name="target">
    ///   The cmdlet with which to reissue captured output.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="output"/>, <paramref name="streams"/>, and/or
    ///   <paramref name="target"/> is <see langword="null"/>.
    /// </exception>
    public Redirector(
        PSDataCollection<PSObject?> output,
        PSDataStreams               streams,
        Cmdlet                      target)
    {
        if (output is null)
            throw new ArgumentNullException(nameof(output));
        if (streams is null)
            throw new ArgumentNullException(nameof(streams));
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        _output  = output;
        _streams = streams;
        _target  = target;

        output             .DataAdded +=      Output_DataAdded;
        streams.Error      .DataAdded +=       Error_DataAdded;
        streams.Warning    .DataAdded +=     Warning_DataAdded;
        streams.Verbose    .DataAdded +=     Verbose_DataAdded;
        streams.Debug      .DataAdded +=       Debug_DataAdded;
        streams.Information.DataAdded += Information_DataAdded;
        streams.Progress   .DataAdded +=    Progress_DataAdded;
    }

    private void Output_DataAdded(object? sender, DataAddedEventArgs e)
    {
        foreach (var obj in _output.ReadAll())
            WriteObject(obj);
    }

    private void Error_DataAdded(object? sender, DataAddedEventArgs e)
    {
        foreach (var record in _streams.Error.ReadAll())
            WriteError(record);
    }

    private void Warning_DataAdded(object? sender, DataAddedEventArgs e)
    {
        foreach (var record in _streams.Warning.ReadAll())
            WriteWarning(record.Message);
    }

    private void Verbose_DataAdded(object? sender, DataAddedEventArgs e)
    {
        foreach (var record in _streams.Verbose.ReadAll())
            WriteVerbose(record.Message);
    }

    private void Debug_DataAdded(object? sender, DataAddedEventArgs e)
    {
        foreach (var record in _streams.Debug.ReadAll())
            WriteDebug(record.Message);
    }

    private void Information_DataAdded(object? sender, DataAddedEventArgs e)
    {
        foreach (var record in _streams.Information.ReadAll())
            WriteInformation(record);
    }

    private void Progress_DataAdded(object? sender, DataAddedEventArgs e)
    {
        foreach (var record in _streams.Progress.ReadAll())
            WriteProgress(record);
    }

    /// <summary>
    ///   Writes the specified object to the output (success) stream.
    /// </summary>
    /// <param name="obj">
    ///   The object to write.
    /// </param>
    /// <remarks>
    ///   Derived types can override this method to transform the output.
    /// </remarks>
    protected virtual void WriteObject(object? obj)
    {
        _target.WriteObject(obj);
    }

    /// <summary>
    ///   Writes the specified record to the error stream.
    /// </summary>
    /// <param name="record">
    ///   The record to write.
    /// </param>
    /// <remarks>
    ///   Derived types can override this method to transform the record.
    /// </remarks>
    protected virtual void WriteError(ErrorRecord record)
    {
        _target.WriteError(record);
    }

    /// <summary>
    ///   Writes the specified message to the warning stream.
    /// </summary>
    /// <param name="message">
    ///   The message to write.
    /// </param>
    /// <remarks>
    ///   Derived types can override this method to transform the message.
    /// </remarks>
    protected virtual void WriteWarning(string? message)
    {
        _target.WriteWarning(message);
    }

    /// <summary>
    ///   Writes the specified message to the verbose stream.
    /// </summary>
    /// <param name="message">
    ///   The message to write.
    /// </param>
    /// <remarks>
    ///   Derived types can override this method to transform the message.
    /// </remarks>
    protected virtual void WriteVerbose(string? message)
    {
        _target.WriteVerbose(message);
    }

    /// <summary>
    ///   Writes the specified message to the debug stream.
    /// </summary>
    /// <param name="message">
    ///   The message to write.
    /// </param>
    /// <remarks>
    ///   Derived types can override this method to transform the message.
    /// </remarks>
    protected virtual void WriteDebug(string? message)
    {
        _target.WriteDebug(message);
    }

    /// <summary>
    ///   Writes the specified record to the information stream.
    /// </summary>
    /// <param name="record">
    ///   The record to write.
    /// </param>
    /// <remarks>
    ///   Derived types can override this method to transform the record.
    /// </remarks>
    protected virtual void WriteInformation(InformationRecord record)
    {
        _target.WriteInformation(record);
    }

    /// <summary>
    ///   Writes the specified record to the progress stream.
    /// </summary>
    /// <param name="record">
    ///   The record to write.
    /// </param>
    /// <remarks>
    ///   Derived types can override this method to transform the record.
    /// </remarks>
    protected virtual void WriteProgress(ProgressRecord record)
    {
        _target.WriteProgress(record);
    }
}
