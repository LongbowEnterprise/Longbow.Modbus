// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using System.Net;
using System.Net.Sockets;

namespace Longbow.Modbus;

class DefaultUdpClient(ModbusUdpClientOptions options, IModbusTcpMessageBuilder builder) : ModbusClientBase(builder), IModbusTcpClient
{
    private UdpClient _client = default!;

    public async ValueTask<bool> ConnectAsync(IPEndPoint endPoint, CancellationToken token = default)
    {
        var ret = false;

        await CloseAsync();
        _client = new UdpClient(options.LocalEndPoint);

        try
        {
            var connectToken = token;
            // 增加连接超时处理
            if (options.ConnectTimeout > 0)
            {
                using var connectTimeoutCancellationTokenSource = new CancellationTokenSource(options.ConnectTimeout);
                var linkToken = CancellationTokenSource.CreateLinkedTokenSource(token, connectTimeoutCancellationTokenSource.Token);
                connectToken = linkToken.Token;
            }
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
            }, connectToken);
            ret = true;
        }
        catch (Exception ex)
        {
            Exception = ex;
        }

        return ret;
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
