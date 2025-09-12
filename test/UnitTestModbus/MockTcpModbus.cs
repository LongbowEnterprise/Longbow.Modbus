// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using System.Net;
using System.Net.Sockets;

namespace UnitTestModbus;

internal static class MockTcpModbus
{
    private static TcpListener? _listenser;

    public static TcpListener Start()
    {
        _listenser = new TcpListener(IPAddress.Loopback, 502);
        _listenser.Start();
        Task.Run(() => AcceptClientsAsync(_listenser));
        return _listenser;
    }

    public static void Stop()
    {
        _listenser?.Stop();
        _listenser?.Dispose();
        _listenser = null;
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
        using var stream = client.GetStream();
        while (true)
        {
            var buffer = new byte[1024];
            var len = await stream.ReadAsync(buffer);
            if (len == 0)
            {
                client.Close();
                break;
            }

            if (len >= 12)
            {
                var request = buffer[0..12];
                if (request[7] == 0x01)
                {
                    // ReadCoilAsync
                    await stream.WriteAsync(GenerateResponse(request, "00 00 00 05 01 01 02 05 00"), CancellationToken.None);
                }
                else if (request[7] == 0x02)
                {
                    // ReadInputsAsync
                    await stream.WriteAsync(GenerateResponse(request, "00 00 00 05 01 02 02 00 00"), CancellationToken.None);
                }
                else if (request[7] == 0x03)
                {
                    // ReadHoldingRegistersAsync
                    await stream.WriteAsync(GenerateResponse(request, "00 00 00 17 01 03 14 00 0C 00 00 00 17 00 00 00 2E 00 00 00 01 00 02 00 04 00 05"), CancellationToken.None);
                }
                else if (request[7] == 0x04)
                {
                    // ReadInputRegistersAsync
                    await stream.WriteAsync(GenerateResponse(request, "00 00 00 17 01 04 14 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00"), CancellationToken.None);
                }
                else if (request[7] == 0x05)
                {
                    // WriteCoilAsync
                    var v = request[10] == 0xFF ? "FF" : "00";
                    await stream.WriteAsync(GenerateResponse(request, $"00 00 00 06 01 05 00 00 {v} 00"), CancellationToken.None);
                }
                else if (request[7] == 0x06)
                {
                    // WriteMultipleCoilsAsync
                    await stream.WriteAsync(GenerateResponse(request, "00 00 00 06 01 06 00 00 00 0C"), CancellationToken.None);
                }
                else if (request[7] == 0x0F)
                {
                    // WriteRegisterAsync
                    await stream.WriteAsync(GenerateResponse(request, "00 00 00 06 01 0F 00 00 00 0A"), CancellationToken.None);
                }
                else if (request[7] == 0x10)
                {
                    // WriteMultipleRegistersAsync
                    await stream.WriteAsync(GenerateResponse(request, "00 00 00 06 01 10 00 00 00 0A"), CancellationToken.None);
                }
            }
        }
    }

    private static ReadOnlyMemory<byte> GenerateResponse(ReadOnlyMemory<byte> request, string data)
    {
        var buffer = HexConverter.ToBytes(data, " ");

        var response = new byte[buffer.Length + 2];
        response[0] = request.Span[0];
        response[1] = request.Span[1];
        buffer.CopyTo(response.AsSpan(2));

        return response;
    }
}

class TcpModbusFixture : IDisposable
{
    public TcpModbusFixture()
    {
        MockTcpModbus.Start();
    }

    public void Dispose()
    {
        MockTcpModbus.Stop();
        GC.SuppressFinalize(this);
    }
}

[CollectionDefinition("MockTcpModbus")]
public class TcpModbusCollection : ICollectionFixture<TcpModbusFixture>
{

}
