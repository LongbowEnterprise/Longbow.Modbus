// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using System.Net;
using System.Net.Sockets;

namespace UnitTestModbus;

internal static class MockRtuOverTcpModbus
{
    private static TcpListener? _listenser;

    public static TcpListener Start()
    {
        _listenser = new TcpListener(IPAddress.Loopback, 501);
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

            if (len >= 8)
            {
                var request = buffer[0..8];
                if (request[1] == 0x01)
                {
                    // ReadCoilAsync
                    await stream.WriteAsync(HexConverter.ToBytes("01 01 02 FD 02 78 AD", " "), CancellationToken.None);
                }
                else if (request[1] == 0x02)
                {
                    // ReadInputsAsync
                    await stream.WriteAsync(HexConverter.ToBytes("01 02 02 00 00 B9 B8", " "), CancellationToken.None);
                }
                else if (request[1] == 0x03)
                {
                    // ReadHoldingRegistersAsync
                    await stream.WriteAsync(HexConverter.ToBytes("01 03 14 00 0C 00 00 00 17 00 00 00 2E 00 00 00 01 00 02 00 04 00 05 90 D2", " "), CancellationToken.None);
                }
                else if (request[1] == 0x04)
                {
                    // ReadInputRegistersAsync
                    await stream.WriteAsync(HexConverter.ToBytes("01 04 14 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 95 81", " "), CancellationToken.None);
                }
                else if (request[1] == 0x05)
                {
                    // WriteCoilAsync
                    var v = request[4] == 0xFF ? "01 05 00 00 FF 00 8C 3A" : "01 05 00 01 00 00 9C 0A";
                    await stream.WriteAsync(HexConverter.ToBytes(v, " "), CancellationToken.None);
                }
                else if (request[1] == 0x06)
                {
                    // WriteMultipleCoilsAsync
                    await stream.WriteAsync(HexConverter.ToBytes("01 06 00 00 00 0C 89 CF", " "), CancellationToken.None);
                }
                else if (request[1] == 0x0F)
                {
                    // WriteRegisterAsync
                    await stream.WriteAsync(HexConverter.ToBytes("01 0F 00 00 00 0A D5 CC", " "), CancellationToken.None);
                }
                else if (request[1] == 0x10)
                {
                    // WriteMultipleRegistersAsync
                    await stream.WriteAsync(HexConverter.ToBytes("01 10 00 00 00 0A 40 0E", " "), CancellationToken.None);
                }
            }
        }
    }
}

class RtuOverTcpModbusFixture : IDisposable
{
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
public class RtuModbusCollection : ICollectionFixture<RtuOverTcpModbusFixture>
{

}
