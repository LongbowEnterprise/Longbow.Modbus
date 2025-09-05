// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using Longbow.TcpSocket;
using System.Net;

namespace Longbow.Modbus;

class DefaultModbusTcpClient(ITcpSocketClient client) : IModbusTcpClient
{
    private CancellationTokenSource? _receiveCancellationTokenSource;

    private readonly ModbusTcpMessageBuilder _builder = new();

    public Exception? Exception { get; private set; }

    public ValueTask<bool> ConnectAsync(IPEndPoint endPoint, CancellationToken token = default) => client.ConnectAsync(endPoint, token);

    public ValueTask<bool[]?> ReadCoilsAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints) => ReadAsync(slaveAddress, 0x01, startAddress, numberOfPoints, ReadBoolValues);

    public ValueTask<bool[]?> ReadInputsAsync(byte slaveAddress, ushort startAddress, ushort numberOfInputs) => ReadAsync(slaveAddress, 0x02, startAddress, numberOfInputs, ReadBoolValues);

    public ValueTask<ushort[]?> ReadHoldingRegistersAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints) => ReadAsync(slaveAddress, 0x03, startAddress, numberOfPoints, ReadUShortValues);

    public ValueTask<ushort[]?> ReadInputRegistersAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints) => ReadAsync(slaveAddress, 0x04, startAddress, numberOfPoints, ReadUShortValues);

    public ValueTask<bool> WriteCoilAsync(byte slaveAddress, ushort coilAddress, bool value) => WriteBoolValuesAsync(slaveAddress, 0x05, coilAddress, [value]);

    public ValueTask<bool> WriteMultipleCoilsAsync(byte slaveAddress, ushort startAddress, bool[] values) => WriteBoolValuesAsync(slaveAddress, 0x0F, startAddress, values);

    public ValueTask<bool> WriteRegisterAsync(byte slaveAddress, ushort registerAddress, ushort value) => WriteUShortValuesAsync(slaveAddress, 0x06, registerAddress, [value]);

    public ValueTask<bool> WriteMultipleRegistersAsync(byte slaveAddress, ushort registerAddress, ushort[] values) => WriteUShortValuesAsync(slaveAddress, 0x10, registerAddress, values);

    private async ValueTask<TResult?> ReadAsync<TResult>(byte slaveAddress, byte functionCode, ushort startAddress, ushort numberOfPoints, Func<ReadOnlyMemory<byte>, ushort, TResult> parser)
    {
        if (!client.IsConnected)
        {
            throw new InvalidOperationException("站点未连接请先使用 ConnectAsync 连接设备");
        }

        var request = _builder.BuildReadRequest(slaveAddress, functionCode, startAddress, numberOfPoints);
        var result = await client.SendAsync(request);
        if (!result)
        {
            return default;
        }

        _receiveCancellationTokenSource ??= new();
        var received = await client.ReceiveAsync(_receiveCancellationTokenSource.Token);

        if (!_builder.TryValidateReadResponse(received, functionCode, out var exception))
        {
            Exception = exception;
            return default;
        }

        return parser(received, numberOfPoints);
    }

    private async ValueTask<bool> WriteBoolValuesAsync(byte slaveAddress, byte functionCode, ushort address, bool[] values)
    {
        if (!client.IsConnected)
        {
            throw new InvalidOperationException("站点未连接请先使用 ConnectAsync 连接设备");
        }

        var data = WriteBoolValues(address, values);
        var request = _builder.BuildWriteRequest(slaveAddress, functionCode, data);
        var result = await client.SendAsync(request);
        if (result)
        {
            var response = await client.ReceiveAsync();
            if (!_builder.TryValidateWriteResponse(response, functionCode, data, out var exception))
            {
                Exception = exception;
                result = false;
            }
        }
        return result;
    }

    private async ValueTask<bool> WriteUShortValuesAsync(byte slaveAddress, byte functionCode, ushort address, ushort[] values)
    {
        if (!client.IsConnected)
        {
            throw new InvalidOperationException("站点未连接请先使用 ConnectAsync 连接设备");
        }

        var data = WriteUShortValues(address, values);
        var request = _builder.BuildWriteRequest(slaveAddress, functionCode, data);
        var result = await client.SendAsync(request);
        if (result)
        {
            var response = await client.ReceiveAsync();
            if (!_builder.TryValidateWriteResponse(response, functionCode, data, out var exception))
            {
                Exception = exception;
                result = false;
            }
        }
        return result;
    }

    private static ReadOnlyMemory<byte> WriteBoolValues(ushort address, bool[] values)
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

    private static ReadOnlyMemory<byte> WriteUShortValues(ushort address, ushort[] values)
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

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async ValueTask CloseAsync()
    {
        await CloseCoreAsync();
    }

    private async ValueTask CloseCoreAsync()
    {
        // 取消接收数据的任务
        if (_receiveCancellationTokenSource != null)
        {
            _receiveCancellationTokenSource.Cancel();
            _receiveCancellationTokenSource.Dispose();
            _receiveCancellationTokenSource = null;
        }

        if (client.IsConnected)
        {
            await client.CloseAsync();
        }
    }

    /// <summary>
    /// Releases the resources used by the current instance of the class.
    /// </summary>
    /// <remarks>This method is called to free both managed and unmanaged resources. If the <paramref
    /// name="disposing"/> parameter is <see langword="true"/>, the method releases managed resources in addition to
    /// unmanaged resources. Override this method in a derived class to provide custom cleanup logic.</remarks>
    /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only
    /// unmanaged resources.</param>
    private async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            await CloseAsync();
        }
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
