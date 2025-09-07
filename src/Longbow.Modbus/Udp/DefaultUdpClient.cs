// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using System.Net;
using System.Net.Sockets;

namespace Longbow.Modbus;

class DefaultUdpClient(ModbusUdpClientOptions options, IModbusTcpMessageBuilder builder) : ModbusClientBase, IModbusUdpClient
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

    private async Task<ReadOnlyMemory<byte>> SendAsync(ReadOnlyMemory<byte> request)
    {
        _client.ThrowIfNotConnected();

        var ret = ReadOnlyMemory<byte>.Empty;
        try
        {
            var sendToken = new CancellationTokenSource(options.WriteTimeout);
            await _client.SendAsync(request, sendToken.Token);

            var receivetoken = new CancellationTokenSource(options.ReadTimeout);
            var result = await _client.ReceiveAsync(receivetoken.Token);
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
        // 构建请求报文
        var request = builder.BuildReadRequest(slaveAddress, functionCode, startAddress, numberOfPoints);

        // 发送请求
        var received = await SendAsync(request);

        // 验证响应报文
        var valid = builder.TryValidateReadResponse(received, slaveAddress, functionCode, out var exception);

        Exception = valid ? null : exception;
        return valid ? received : default;
    }

    protected override bool[] ReadBoolValues(ReadOnlyMemory<byte> response, ushort numberOfPoints) => builder.ReadBoolValues(response, numberOfPoints);

    protected override ushort[] ReadUShortValues(ReadOnlyMemory<byte> response, ushort numberOfPoints) => builder.ReadUShortValues(response, numberOfPoints);

    protected override async ValueTask<bool> WriteBoolValuesAsync(byte slaveAddress, byte functionCode, ushort address, bool[] values)
    {
        // 构建请求报文
        var data = builder.WriteBoolValues(address, values);
        var request = builder.BuildWriteRequest(slaveAddress, functionCode, data);

        // 发送请求
        var received = await SendAsync(request);

        // 验证响应报文
        var valid = builder.TryValidateWriteResponse(received, slaveAddress, functionCode, data, out var exception);

        Exception = valid ? null : exception;
        return valid;
    }

    protected override async ValueTask<bool> WriteUShortValuesAsync(byte slaveAddress, byte functionCode, ushort address, ushort[] values)
    {
        // 构建请求报文
        var data = builder.WriteUShortValues(address, values);
        var request = builder.BuildWriteRequest(slaveAddress, functionCode, data);

        // 发送请求
        var received = await SendAsync(request);

        // 验证响应报文
        var valid = builder.TryValidateWriteResponse(received, slaveAddress, functionCode, data, out var exception);

        Exception = valid ? null : exception;
        return valid;
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
