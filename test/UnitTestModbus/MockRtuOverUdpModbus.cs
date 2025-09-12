// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using System.Buffers;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace UnitTestModbus;

internal static class MockRtuOverUdpModbus
{
    private static Socket? _socket;

    public static Socket Start()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _socket.Bind(new IPEndPoint(IPAddress.Any, 503));
        Task.Run(() => AcceptClientsAsync(_socket));
        return _socket;
    }

    public static void Stop()
    {
        _socket?.Close();
        _socket?.Dispose();
        _socket = null;
    }

    private static async Task AcceptClientsAsync(Socket server)
    {
        while (true)
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            var buffer = MemoryPool<byte>.Shared.Rent(1024);
            var response = await server.ReceiveFromAsync(buffer.Memory, SocketFlags.None, remoteEP);

            if (response.ReceivedBytes >= 8)
            {
                var request = buffer.Memory[0..8];
                var data = request.Span;
                if (data[1] == 0x01)
                {
                    // ReadCoilAsync
                    await server.SendToAsync(HexConverter.ToBytes("01 01 02 FD 02 78 AD", " "), response.RemoteEndPoint, CancellationToken.None);
                }
                else if (data[1] == 0x02)
                {
                    // ReadInputsAsync
                    await server.SendToAsync(HexConverter.ToBytes("01 02 02 00 00 B9 B8", " "), response.RemoteEndPoint, CancellationToken.None);
                }
                else if (data[1] == 0x03)
                {
                    // ReadHoldingRegistersAsync
                    await server.SendToAsync(HexConverter.ToBytes("01 03 14 00 0C 00 00 00 17 00 00 00 2E 00 00 00 01 00 02 00 04 00 05 90 D2", " "), response.RemoteEndPoint, CancellationToken.None);
                }
                else if (data[1] == 0x04)
                {
                    // ReadInputRegistersAsync
                    await server.SendToAsync(HexConverter.ToBytes("01 04 14 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 95 81", " "), response.RemoteEndPoint, CancellationToken.None);
                }
                else if (data[1] == 0x05)
                {
                    // WriteCoilAsync
                    var v = data[4] == 0xFF ? "01 05 00 00 FF 00 8C 3A" : "01 05 00 01 00 00 9C 0A";
                    await server.SendToAsync(HexConverter.ToBytes(v, " "), response.RemoteEndPoint, CancellationToken.None);
                }
                else if (data[1] == 0x06)
                {
                    // WriteMultipleCoilsAsync
                    await server.SendToAsync(HexConverter.ToBytes("01 06 00 00 00 0C 89 CF", " "), response.RemoteEndPoint, CancellationToken.None);
                }
                else if (data[1] == 0x0F)
                {
                    // WriteRegisterAsync
                    await server.SendToAsync(HexConverter.ToBytes("01 0F 00 00 00 0A D5 CC", " "), response.RemoteEndPoint, CancellationToken.None);
                }
                else if (data[1] == 0x10)
                {
                    // WriteMultipleRegistersAsync
                    await server.SendToAsync(HexConverter.ToBytes("01 10 00 00 00 0A 40 0E", " "), response.RemoteEndPoint, CancellationToken.None);
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

class RtuOverUdpModbusFixture : IDisposable
{
    public RtuOverUdpModbusFixture()
    {
        MockRtuOverUdpModbus.Start();
    }

    public void Dispose()
    {
        MockRtuOverUdpModbus.Stop();
        GC.SuppressFinalize(this);
    }
}

[CollectionDefinition("MockRtuOverUdpModbus")]
public class RtuOverUdpModbusCollection : ICollectionFixture<RtuOverUdpModbusFixture>
{

}
