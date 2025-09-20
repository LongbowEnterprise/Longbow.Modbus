// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using System.Net;
using System.Net.Sockets;

namespace Longbow.Modbus;

class DefaultUdpClient(ModbusUdpClientOptions options, IModbusMessageBuilder builder) : ModbusClientBase(builder), IModbusTcpClient
{
    private UdpClient _client = default!;

    public async ValueTask<bool> ConnectAsync(IPEndPoint endPoint, CancellationToken token = default)
    {
        await CloseAsync();
        _client = new UdpClient(options.LocalEndPoint);
        _client.Connect(endPoint);
        return true;
    }

    protected override async Task<ReadOnlyMemory<byte>> SendAsync(ReadOnlyMemory<byte> request, CancellationToken token = default)
    {
        _client.ThrowIfNotConnected();

        var ret = ReadOnlyMemory<byte>.Empty;
        try
        {
            var sendToken = new CancellationTokenSource(options.WriteTimeout);
            await _client.SendAsync(request, sendToken.Token);

            var receiveToken = new CancellationTokenSource(options.ReadTimeout);
            var result = await _client.ReceiveAsync(receiveToken.Token);
            ret = result.Buffer;
        }
        catch (Exception ex)
        {
            Exception = ex;
        }

        return ret;
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
