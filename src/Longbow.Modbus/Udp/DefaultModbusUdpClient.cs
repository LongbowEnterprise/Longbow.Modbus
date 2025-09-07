// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Sockets;

namespace Longbow.Modbus;

class DefaultModbusUdpClient(ModbusUdpClientOptions options, IModbusTcpMessageBuilder builder) : ModbusClientBase, IModbusUdpClient
{
    private UdpClient _client = default!;

    public async ValueTask<bool> ConnectAsync(IPEndPoint endPoint, CancellationToken token = default)
    {
        var ret = false;
        _client = new UdpClient(options.LocalEndPoint);

        try
        {
            await Task.Run(() =>
            {
                try
                {
                    _client.Connect(endPoint);
                }
                catch (Exception ex)
                {
                    _client.Dispose();
                    _client = default!;

                    Exception = ex;
                }
            }, token);
            ret = true;
        }
        catch (Exception ex)
        {
            Exception = ex;
        }
        return ret;
    }

    private async Task SendAsync(ReadOnlyMemory<byte> request)
    {
        Exception = null;

        try
        {
            var token = new CancellationTokenSource(options.WriteTimeout);
            await _client.SendAsync(request, token.Token);
        }
        catch (Exception ex)
        {
            Exception = ex;
        }
    }

    private async Task<ReadOnlyMemory<byte>> ReceiveAsync()
    {
        if (Exception != null)
        {
            return default;
        }

        var ret = ReadOnlyMemory<byte>.Empty;
        try
        {
            var token = new CancellationTokenSource(options.ReadTimeout);
            var result = await _client.ReceiveAsync(token.Token);
            ret = result.Buffer;
        }
        catch (Exception ex)
        {
            Exception = ex;
        }
        return ret;
    }

    protected override async ValueTask<ReadOnlyMemory<byte>> ReadAsync(byte slaveAddress, byte functionCode, ushort startAddress, ushort numberOfPoints)
    {
        _client.ThrowIfNotConnected();

        var request = builder.BuildReadRequest(slaveAddress, functionCode, startAddress, numberOfPoints);
        await SendAsync(request);
        var received = await ReceiveAsync();

        if (Exception != null)
        {
            return default;
        }

        var valid = builder.TryValidateReadResponse(received, slaveAddress, functionCode, out var exception);
        Exception = valid ? null : exception;
        return valid ? received : default;
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
        _client.ThrowIfNotConnected();

        var data = WriteBoolValues(address, values);
        var request = builder.BuildWriteRequest(slaveAddress, functionCode, data);
        await SendAsync(request);
        var received = await ReceiveAsync();

        if (Exception != null)
        {
            return false;
        }

        var valid = builder.TryValidateWriteResponse(received, slaveAddress, functionCode, data, out var exception);
        Exception = valid ? null : exception;
        return valid;
    }

    protected override async ValueTask<bool> WriteUShortValuesAsync(byte slaveAddress, byte functionCode, ushort address, ushort[] values)
    {
        _client.ThrowIfNotConnected();

        var data = WriteUShortValues(address, values);
        var request = builder.BuildWriteRequest(slaveAddress, functionCode, data);
        await SendAsync(request);
        var response = await ReceiveAsync();

        if (Exception != null)
        {
            return false;
        }

        var valid = builder.TryValidateWriteResponse(response, slaveAddress, functionCode, data, out var exception);
        Exception = valid ? null : exception;
        return valid;
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
    public ValueTask CloseAsync()
    {
        if (_client != null)
        {
            _client.Close();
            _client.Dispose();
            _client = default!;
        }

        return ValueTask.CompletedTask;
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
