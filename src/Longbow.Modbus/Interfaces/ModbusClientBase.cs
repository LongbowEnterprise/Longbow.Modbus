// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

namespace Longbow.Modbus;

abstract class ModbusClientBase : IModbusClient
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
    /// 资源销毁方法
    /// </summary>
    /// <param name="disposing"></param>
    /// <returns></returns>
    protected abstract ValueTask DisposeAsync(bool disposing);

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(true);
        GC.SuppressFinalize(this);
    }
}
