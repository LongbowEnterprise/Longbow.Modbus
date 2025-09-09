// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using System.Net;

namespace Longbow.Modbus;

class DefaultRtuOverTcpClient(ITcpSocketClient client, IModbusRtuMessageBuilder builder) : ModbusClientBase(builder), IModbusTcpClient
{
    private CancellationTokenSource? _receiveCancellationTokenSource;

    public ValueTask<bool> ConnectAsync(IPEndPoint endPoint, CancellationToken token = default) => client.ConnectAsync(endPoint, token);

    protected override async Task<ReadOnlyMemory<byte>> SendAsync(ReadOnlyMemory<byte> request, CancellationToken token = default)
    {
        client.ThrowIfNotConnected();

        await client.SendAsync(request);
        var received = await client.ReceiveAsync();
        return received;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override async ValueTask CloseAsync()
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
}
