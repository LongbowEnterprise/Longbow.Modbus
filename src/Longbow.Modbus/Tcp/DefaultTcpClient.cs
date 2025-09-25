// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using System.Net;

namespace Longbow.Modbus;

class DefaultTcpClient(ITcpSocketClient client, IModbusMessageBuilder builder) : ModbusClientBase(builder), IModbusTcpClient
{
    public ValueTask<bool> ConnectAsync(IPEndPoint endPoint, CancellationToken token = default) => client.ConnectAsync(endPoint, token);

    protected override async Task<ReadOnlyMemory<byte>> SendAsync(ReadOnlyMemory<byte> request, CancellationToken token = default)
    {
        if (client.IsConnected == false)
        {
            throw new InvalidOperationException("The TCP client is not connected. Please establish a connection before sending data.");
        }

        var ret = await client.SendAsync(request, token);

        return ret ? await client.ReceiveAsync(token) : ReadOnlyMemory<byte>.Empty;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override async ValueTask CloseAsync()
    {
        if (client.IsConnected)
        {
            await client.CloseAsync();
        }
    }
}
