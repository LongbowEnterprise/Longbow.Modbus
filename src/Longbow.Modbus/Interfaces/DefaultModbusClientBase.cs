// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

namespace Longbow.Modbus;

abstract class DefaultModbusClientBase : IModbusClient
{
    public Exception? Exception { get; protected set; }

    public async ValueTask<bool[]> ReadCoilsAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints)
    {
        var response = await ReadAsync(slaveAddress, 0x01, startAddress, numberOfPoints);
        return ReadBoolValues(response, numberOfPoints);
    }

    public async ValueTask<bool[]> ReadInputsAsync(byte slaveAddress, ushort startAddress, ushort numberOfInputs)
    {
        var response = await ReadAsync(slaveAddress, 0x02, startAddress, numberOfInputs);
        return ReadBoolValues(response, numberOfInputs);
    }

    public async ValueTask<ushort[]> ReadHoldingRegistersAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints)
    {
        var response = await ReadAsync(slaveAddress, 0x03, startAddress, numberOfPoints);
        return ReadUShortValues(response, numberOfPoints);
    }

    public async ValueTask<ushort[]> ReadInputRegistersAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints)
    {
        var response = await ReadAsync(slaveAddress, 0x04, startAddress, numberOfPoints);
        return ReadUShortValues(response, numberOfPoints);
    }

    protected abstract ValueTask<ReadOnlyMemory<byte>> ReadAsync(byte slaveAddress, byte functionCode, ushort startAddress, ushort numberOfPoints);

    protected abstract bool[] ReadBoolValues(ReadOnlyMemory<byte> response, ushort numberOfPoints);

    protected abstract ushort[] ReadUShortValues(ReadOnlyMemory<byte> response, ushort numberOfPoints);

    public ValueTask<bool> WriteCoilAsync(byte slaveAddress, ushort coilAddress, bool value) => WriteBoolValuesAsync(slaveAddress, 0x05, coilAddress, [value]);

    public ValueTask<bool> WriteRegisterAsync(byte slaveAddress, ushort registerAddress, ushort value) => WriteUShortValuesAsync(slaveAddress, 0x06, registerAddress, [value]);

    public ValueTask<bool> WriteMultipleCoilsAsync(byte slaveAddress, ushort startAddress, bool[] values) => WriteBoolValuesAsync(slaveAddress, 0x0F, startAddress, values);

    public ValueTask<bool> WriteMultipleRegistersAsync(byte slaveAddress, ushort registerAddress, ushort[] values) => WriteUShortValuesAsync(slaveAddress, 0x10, registerAddress, values);

    protected abstract ValueTask<bool> WriteBoolValuesAsync(byte slaveAddress, byte functionCode, ushort address, bool[] values);

    protected abstract ValueTask<bool> WriteUShortValuesAsync(byte slaveAddress, byte functionCode, ushort address, ushort[] values);

    /// <summary>
    /// Releases the resources used by the current instance of the class.
    /// </summary>
    /// <remarks>This method is called to free both managed and unmanaged resources. If the <paramref
    /// name="disposing"/> parameter is <see langword="true"/>, the method releases managed resources in addition to
    /// unmanaged resources. Override this method in a derived class to provide custom cleanup logic.</remarks>
    /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only
    /// unmanaged resources.</param>
    protected virtual ValueTask DisposeAsync(bool disposing)
    {
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(true);
        GC.SuppressFinalize(this);
    }
}
