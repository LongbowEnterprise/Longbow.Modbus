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

    private static bool[] ReadBoolValues(ReadOnlyMemory<byte> response, ushort numberOfPoints)
    {
        var values = new bool[numberOfPoints];
        for (var i = 0; i < numberOfPoints; i++)
        {
            var byteIndex = 9 + i / 8;
            var bitIndex = i % 8;
            values[i] = (response.Span[byteIndex] & (1 << bitIndex)) != 0;
        }

        return values;
    }

    private static ushort[] ReadUShortValues(ReadOnlyMemory<byte> response, ushort numberOfPoints)
    {
        var values = new ushort[numberOfPoints];
        for (var i = 0; i < numberOfPoints; i++)
        {
            int offset = 9 + (i * 2);
            values[i] = (ushort)((response.Span[offset] << 8) | response.Span[offset + 1]);
        }

        return values;
    }

    public ValueTask<bool> WriteCoilAsync(byte slaveAddress, ushort coilAddress, bool value) => WriteBoolValuesAsync(slaveAddress, 0x05, coilAddress, [value]);

    public ValueTask<bool> WriteRegisterAsync(byte slaveAddress, ushort registerAddress, ushort value) => WriteUShortValuesAsync(slaveAddress, 0x06, registerAddress, [value]);

    public ValueTask<bool> WriteMultipleCoilsAsync(byte slaveAddress, ushort startAddress, bool[] values) => WriteBoolValuesAsync(slaveAddress, 0x0F, startAddress, values);

    public ValueTask<bool> WriteMultipleRegistersAsync(byte slaveAddress, ushort registerAddress, ushort[] values) => WriteUShortValuesAsync(slaveAddress, 0x10, registerAddress, values);

    protected abstract ValueTask<bool> WriteBoolValuesAsync(byte slaveAddress, byte functionCode, ushort address, bool[] values);

    protected abstract ValueTask<bool> WriteUShortValuesAsync(byte slaveAddress, byte functionCode, ushort address, ushort[] values);

    protected static ReadOnlyMemory<byte> WriteBoolValues(ushort address, bool[] values)
    {
        int byteCount = (values.Length + 7) / 8;
        var data = new byte[values.Length > 1 ? 5 + byteCount : 4];
        data[0] = (byte)(address >> 8);
        data[1] = (byte)address;

        if (values.Length > 1)
        {
            // 多值时，写入数量
            data[2] = (byte)(values.Length >> 8);
            data[3] = (byte)(values.Length);

            // 字节数
            data[4] = (byte)(byteCount);

            for (var i = 0; i < values.Length; i++)
            {
                if (values[i])
                {
                    int byteIndex = 5 + i / 8;
                    int bitIndex = i % 8;
                    data[byteIndex] |= (byte)(1 << bitIndex);
                }
            }
        }
        else
        {
            // 组装数据
            data[2] = values[0] ? (byte)0xFF : (byte)0x00;
            data[3] = 0x00;
        }
        return data;
    }

    protected static ReadOnlyMemory<byte> WriteUShortValues(ushort address, ushort[] values)
    {
        int byteCount = values.Length * 2;
        var data = new byte[values.Length > 1 ? 5 + byteCount : 4];
        data[0] = (byte)(address >> 8);
        data[1] = (byte)address;

        if (values.Length > 1)
        {
            // 多值时，写入数量
            data[2] = (byte)(values.Length >> 8);
            data[3] = (byte)(values.Length);

            // 字节数
            data[4] = (byte)(byteCount);

            for (var i = 0; i < values.Length; i++)
            {
                data[i * 2 + 5] = (byte)(values[i] >> 8);
                data[i * 2 + 6] = (byte)(values[i] & 0xFF);
            }
        }
        else
        {
            data[2] = (byte)(values[0] >> 8);
            data[3] = (byte)(values[0] & 0xFF);
        }
        return data;
    }

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
