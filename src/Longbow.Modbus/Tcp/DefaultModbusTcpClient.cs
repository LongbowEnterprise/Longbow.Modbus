// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using System.Net;

namespace Longbow.Modbus;

class DefaultModbusTcpClient(ITcpSocketClient client, IModbusTcpMessageBuilder builder) : ModbusClientBase, IModbusTcpClient
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

    protected override bool[] ReadBoolValues(ReadOnlyMemory<byte> response, ushort numberOfPoints) => builder.ReadBoolValues(response, numberOfPoints);

    protected override ushort[] ReadUShortValues(ReadOnlyMemory<byte> response, ushort numberOfPoints) => builder.ReadUShortValues(response, numberOfPoints);

    protected override async ValueTask<bool> WriteBoolValuesAsync(byte slaveAddress, byte functionCode, ushort address, bool[] values)
    {
        client.ThrowIfNotConnected();

        var data = builder.WriteBoolValues(address, values);
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

        var data = builder.WriteUShortValues(address, values);
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
