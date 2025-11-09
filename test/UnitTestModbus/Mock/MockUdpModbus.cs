// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using System.Buffers;
using System.Net;
using System.Net.Sockets;

namespace UnitTest;

internal static class MockUdpModbus
{
    private static Socket? _socket;

    public static Socket Start()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _socket.Bind(new IPEndPoint(IPAddress.Any, UdpModbusFixture.Port));
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
                    await server.SendToAsync(MockTcpResponse.ReadCoilResponse(request), response.RemoteEndPoint, CancellationToken.None);
                }
                else if (data[7] == 0x02)
                {
                    // ReadInputsAsync
                    await server.SendToAsync(MockTcpResponse.ReadInputsResponse(request), response.RemoteEndPoint, CancellationToken.None);
                }
                else if (data[7] == 0x03)
                {
                    // ReadHoldingRegistersAsync
                    await server.SendToAsync(MockTcpResponse.ReadHoldingRegistersResponse(request), response.RemoteEndPoint, CancellationToken.None);
                }
                else if (data[7] == 0x04)
                {
                    // ReadInputRegistersAsync
                    await server.SendToAsync(MockTcpResponse.ReadInputRegistersResponse(request), response.RemoteEndPoint, CancellationToken.None);
                }
                else if (data[7] == 0x05)
                {
                    // WriteCoilAsync
                    await server.SendToAsync(MockTcpResponse.WriteCoilResponse(request), response.RemoteEndPoint, CancellationToken.None);
                }
                else if (data[7] == 0x06)
                {
                    // WriteMultipleCoilsAsync
                    await server.SendToAsync(MockTcpResponse.WriteMultipleCoilsResponse(request), response.RemoteEndPoint, CancellationToken.None);
                }
                else if (data[7] == 0x0F)
                {
                    // WriteRegisterAsync
                    await server.SendToAsync(MockTcpResponse.WriteRegisterResponse(request), response.RemoteEndPoint, CancellationToken.None);
                }
                else if (data[7] == 0x10)
                {
                    // WriteMultipleRegistersAsync
                    await server.SendToAsync(MockTcpResponse.WriteMultipleRegistersResponse(request), response.RemoteEndPoint, CancellationToken.None);
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
    public static readonly int Port = 8504;

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
