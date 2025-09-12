// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using System.Buffers;
using System.Net;
using System.Net.Sockets;

namespace UnitTestModbus;

internal static class MockUdpModbus
{
    private static Socket? _socket;

    public static Socket Start()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _socket.Bind(new IPEndPoint(IPAddress.Any, 504));
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

            if (response.ReceivedBytes >= 12)
            {
                var request = buffer.Memory[0..12];
                var data = request.Span;
                if (data[7] == 0x01)
                {
                    // ReadCoilAsync
                    await server.SendToAsync(GenerateResponse(request, "00 00 00 05 01 01 02 05 00"), response.RemoteEndPoint, CancellationToken.None);
                }
                else if (data[7] == 0x02)
                {
                    // ReadInputsAsync
                    await server.SendToAsync(GenerateResponse(request, "00 00 00 05 01 02 02 00 00"), response.RemoteEndPoint, CancellationToken.None);
                }
                else if (data[7] == 0x03)
                {
                    // ReadHoldingRegistersAsync
                    await server.SendToAsync(GenerateResponse(request, "00 00 00 17 01 03 14 00 0C 00 00 00 17 00 00 00 2E 00 00 00 01 00 02 00 04 00 05"), response.RemoteEndPoint, CancellationToken.None);
                }
                else if (data[7] == 0x04)
                {
                    // ReadInputRegistersAsync
                    await server.SendToAsync(GenerateResponse(request, "00 00 00 17 01 04 14 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00"), response.RemoteEndPoint, CancellationToken.None);
                }
                else if (data[7] == 0x05)
                {
                    // WriteCoilAsync
                    var v = data[10] == 0xFF ? "FF" : "00";
                    await server.SendToAsync(GenerateResponse(request, $"00 00 00 06 01 05 00 00 {v} 00"), response.RemoteEndPoint, CancellationToken.None);
                }
                else if (data[7] == 0x06)
                {
                    // WriteMultipleCoilsAsync
                    await server.SendToAsync(GenerateResponse(request, "00 00 00 06 01 06 00 00 00 0C"), response.RemoteEndPoint, CancellationToken.None);
                }
                else if (data[7] == 0x0F)
                {
                    // WriteRegisterAsync
                    await server.SendToAsync(GenerateResponse(request, "00 00 00 06 01 0F 00 00 00 0A"), response.RemoteEndPoint, CancellationToken.None);
                }
                else if (data[7] == 0x10)
                {
                    // WriteMultipleRegistersAsync
                    await server.SendToAsync(GenerateResponse(request, "00 00 00 06 01 10 00 00 00 0A"), response.RemoteEndPoint, CancellationToken.None);
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

class UdpModbusFixture : IDisposable
{
    public UdpModbusFixture()
    {
        MockUdpModbus.Start();
    }

    public void Dispose()
    {
        MockUdpModbus.Stop();
        GC.SuppressFinalize(this);
    }
}

[CollectionDefinition("MockUdpModbus")]
public class UdpModbusCollection : ICollectionFixture<UdpModbusFixture>
{

}
