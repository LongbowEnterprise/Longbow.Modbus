// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using System.Net;

namespace Longbow.Modbus;

class DefaultModbusRtuOverTcpClient(ITcpSocketClient client, IModbusRtuMessageBuilder builder) : ModbusClientBase, IModbusTcpClient
{
    private CancellationTokenSource? _receiveCancellationTokenSource;

    public ValueTask<bool> ConnectAsync(IPEndPoint endPoint, CancellationToken token = default) => client.ConnectAsync(endPoint, token);

    protected override async ValueTask<ReadOnlyMemory<byte>> ReadAsync(byte slaveAddress, byte functionCode, ushort startAddress, ushort numberOfPoints)
    {
        client.ThrowIfNotConnected();

        var request = builder.BuildReadRequest(slaveAddress, functionCode, startAddress, numberOfPoints);
        var result = await client.SendAsync(request);
        if (!result)
        {
            return default;
        }

        _receiveCancellationTokenSource ??= new();
        var received = await client.ReceiveAsync(_receiveCancellationTokenSource.Token);

        if (!builder.TryValidateReadResponse(received, slaveAddress, functionCode, out var exception))
        {
            Exception = exception;
            return default;
        }

        return received;
    }

    protected override bool[] ReadBoolValues(ReadOnlyMemory<byte> response, ushort numberOfPoints)
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

    protected override ushort[] ReadUShortValues(ReadOnlyMemory<byte> response, ushort numberOfPoints)
    {
        var values = new ushort[numberOfPoints];
        for (var i = 0; i < numberOfPoints; i++)
        {
            int offset = 9 + (i * 2);
            values[i] = (ushort)((response.Span[offset] << 8) | response.Span[offset + 1]);
        }

        return values;
    }

    protected override async ValueTask<bool> WriteBoolValuesAsync(byte slaveAddress, byte functionCode, ushort address, bool[] values)
    {
        client.ThrowIfNotConnected();

        var data = WriteBoolValues(address, values);
        var request = builder.BuildWriteRequest(slaveAddress, functionCode, data);
        var result = await client.SendAsync(request);
        if (result)
        {
            var response = await client.ReceiveAsync();
            if (!builder.TryValidateWriteResponse(response, slaveAddress, functionCode, data, out var exception))
            {
                Exception = exception;
                result = false;
            }
        }
        return result;
    }

    protected override async ValueTask<bool> WriteUShortValuesAsync(byte slaveAddress, byte functionCode, ushort address, ushort[] values)
    {
        client.ThrowIfNotConnected();

        var data = WriteUShortValues(address, values);
        var request = builder.BuildWriteRequest(slaveAddress, functionCode, data);
        var result = await client.SendAsync(request);
        if (result)
        {
            var response = await client.ReceiveAsync();
            if (!builder.TryValidateWriteResponse(response, slaveAddress, functionCode, data, out var exception))
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

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async ValueTask CloseAsync()
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
    /// <inheritdoc/>
    /// </summary>
    /// <param name="disposing"></param>
    /// <returns></returns>
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            await CloseAsync();
        }
    }
}
