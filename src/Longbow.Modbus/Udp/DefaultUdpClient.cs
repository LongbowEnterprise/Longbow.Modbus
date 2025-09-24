// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using System.Net;
using System.Net.Sockets;

namespace Longbow.Modbus;

class DefaultUdpClient(ModbusUdpClientOptions options, IModbusMessageBuilder builder) : ModbusClientBase(builder), IModbusTcpClient
{
    private UdpClient _client = default!;

    public ValueTask<bool> ConnectAsync(IPEndPoint endPoint, CancellationToken token = default)
    {
        _client = new UdpClient(options.LocalEndPoint);
        _client.Connect(endPoint);

        return ValueTask.FromResult(true);
    }

    protected override async Task<ReadOnlyMemory<byte>> SendAsync(ReadOnlyMemory<byte> request, CancellationToken token = default)
    {
        await _client.SendAsync(request, token);

        var result = await _client.ReceiveAsync(token);
        return result.Buffer;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override ValueTask CloseAsync()
    {
        if (_client != null)
        {
            _client.Close();
            _client.Dispose();
            _client = default!;
        }

        return ValueTask.CompletedTask;
    }
}
