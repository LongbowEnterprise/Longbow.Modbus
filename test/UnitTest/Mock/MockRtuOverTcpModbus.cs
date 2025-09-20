// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using System.Net;
using System.Net.Sockets;

namespace UnitTest;

internal static class MockRtuOverTcpModbus
{
    private static TcpListener? _listener;

    public static TcpListener Start()
    {
        _listener = new TcpListener(IPAddress.Loopback, RtuOverTcpModbusFixture.Port);
        _listener.Start();
        Task.Run(() => AcceptClientsAsync(_listener));
        return _listener;
    }

    public static void Stop()
    {
        _listener?.Stop();
        _listener?.Dispose();
        _listener = null;
    }

    private static async Task AcceptClientsAsync(TcpListener server)
    {
        while (true)
        {
            var client = await server.AcceptTcpClientAsync();
            _ = Task.Run(() => MockAsync(client));
        }
    }

    private static async Task MockAsync(TcpClient client)
    {
        await using var stream = client.GetStream();
        while (true)
        {
            var buffer = new byte[1024];
            var len = await stream.ReadAsync(buffer);
            if (len == 0)
            {
                client.Close();
                break;
            }

            if (len >= 8)
            {
                var request = buffer[0..8];
                if (request[1] == 0x01)
                {
                    // ReadCoilAsync
                    await stream.WriteAsync(MockRtuResponse.ReadCoilResponse(), CancellationToken.None);
                }
                else if (request[1] == 0x02)
                {
                    // ReadInputsAsync
                    await stream.WriteAsync(MockRtuResponse.ReadInputsResponse(), CancellationToken.None);
                }
                else if (request[1] == 0x03)
                {
                    // ReadHoldingRegistersAsync
                    await stream.WriteAsync(MockRtuResponse.ReadHoldingRegistersResponse(), CancellationToken.None);
                }
                else if (request[1] == 0x04)
                {
                    // ReadInputRegistersAsync
                    await stream.WriteAsync(MockRtuResponse.ReadInputRegistersResponse(), CancellationToken.None);
                }
                else if (request[1] == 0x05)
                {
                    // WriteCoilAsync
                    await stream.WriteAsync(MockRtuResponse.WriteCoilResponse(request), CancellationToken.None);
                }
                else if (request[1] == 0x06)
                {
                    // WriteMultipleCoilsAsync
                    await stream.WriteAsync(MockRtuResponse.WriteMultipleCoilsResponse(), CancellationToken.None);
                }
                else if (request[1] == 0x0F)
                {
                    // WriteRegisterAsync
                    await stream.WriteAsync(MockRtuResponse.WriteRegisterResponse(), CancellationToken.None);
                }
                else if (request[1] == 0x10)
                {
                    // WriteMultipleRegistersAsync
                    await stream.WriteAsync(MockRtuResponse.WriteMultipleRegistersResponse(), CancellationToken.None);
                }
            }
        }
    }
}

class RtuOverTcpModbusFixture : IDisposable
{
    public static readonly int Port = 8501;

    public RtuOverTcpModbusFixture()
    {
        MockRtuOverTcpModbus.Start();
    }

    public void Dispose()
    {
        MockRtuOverTcpModbus.Stop();
        GC.SuppressFinalize(this);
    }
}

[CollectionDefinition("MockRtuOverTcpModbus")]
public class RtuOverTcpModbusCollection : ICollectionFixture<RtuOverTcpModbusFixture>
{

}
