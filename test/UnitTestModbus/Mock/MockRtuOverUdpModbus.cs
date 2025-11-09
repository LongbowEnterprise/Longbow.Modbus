// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using System.Buffers;
using System.Net;
using System.Net.Sockets;

namespace UnitTest;

internal static class MockRtuOverUdpModbus
{
    private static Socket? _socket;

    public static Socket Start()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _socket.Bind(new IPEndPoint(IPAddress.Any, RtuOverUdpModbusFixture.Port));
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
                    await server.SendToAsync(MockRtuResponse.ReadCoilResponse(), response.RemoteEndPoint, CancellationToken.None);
                }
                else if (data[1] == 0x02)
                {
                    // ReadInputsAsync
                    await server.SendToAsync(MockRtuResponse.ReadInputsResponse(), response.RemoteEndPoint, CancellationToken.None);
                }
                else if (data[1] == 0x03)
                {
                    // ReadHoldingRegistersAsync
                    await server.SendToAsync(MockRtuResponse.ReadHoldingRegistersResponse(), response.RemoteEndPoint, CancellationToken.None);
                }
                else if (data[1] == 0x04)
                {
                    // ReadInputRegistersAsync
                    await server.SendToAsync(MockRtuResponse.ReadInputRegistersResponse(), response.RemoteEndPoint, CancellationToken.None);
                }
                else if (data[1] == 0x05)
                {
                    // WriteCoilAsync
                    await server.SendToAsync(MockRtuResponse.WriteCoilResponse(request), response.RemoteEndPoint, CancellationToken.None);
                }
                else if (data[1] == 0x06)
                {
                    // WriteMultipleCoilsAsync
                    await server.SendToAsync(MockRtuResponse.WriteMultipleCoilsResponse(), response.RemoteEndPoint, CancellationToken.None);
                }
                else if (data[1] == 0x0F)
                {
                    // WriteRegisterAsync
                    await server.SendToAsync(MockRtuResponse.WriteRegisterResponse(), response.RemoteEndPoint, CancellationToken.None);
                }
                else if (data[1] == 0x10)
                {
                    // WriteMultipleRegistersAsync
                    await server.SendToAsync(MockRtuResponse.WriteMultipleRegistersResponse(), response.RemoteEndPoint, CancellationToken.None);
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
    public static readonly int Port = 8503;

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
