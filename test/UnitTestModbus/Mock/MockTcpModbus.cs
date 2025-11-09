// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using System.Buffers;
using System.Net;
using System.Net.Sockets;

namespace UnitTest;

internal static class MockTcpModbus
{
    private static TcpListener? _listener;

    public static TcpListener Start()
    {
        _listener = new TcpListener(IPAddress.Loopback, TcpModbusFixture.Port);
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
        using var stream = client.GetStream();
        using var memory = MemoryPool<byte>.Shared.Rent(256);
        var buffer = memory.Memory;
        while (true)
        {
            var len = await stream.ReadAsync(buffer);
            if (len == 0)
            {
                client.Close();
                break;
            }

            if (len >= 12)
            {
                var request = buffer[0..12];
                var functionCode = request.Span[7];
                if (functionCode == 0x01)
                {
                    // ReadCoilAsync
                    await stream.WriteAsync(MockTcpResponse.ReadCoilResponse(request), CancellationToken.None);
                }
                else if (functionCode == 0x02)
                {
                    // ReadInputsAsync
                    await stream.WriteAsync(MockTcpResponse.ReadInputsResponse(request), CancellationToken.None);
                }
                else if (functionCode == 0x03)
                {
                    // ReadHoldingRegistersAsync
                    await stream.WriteAsync(MockTcpResponse.ReadHoldingRegistersResponse(request), CancellationToken.None);
                }
                else if (functionCode == 0x04)
                {
                    // ReadInputRegistersAsync
                    if (request.Span[11] == 10)
                    {
                        await stream.WriteAsync(MockTcpResponse.ReadInputRegistersResponse(request), CancellationToken.None);
                    }
                    else
                    {
                        await stream.WriteAsync(MockTcpResponse.ReadInputRegistersErrorResponse(request), CancellationToken.None);
                    }
                }
                else if (functionCode == 0x05)
                {
                    // WriteCoilAsync
                    await stream.WriteAsync(MockTcpResponse.WriteCoilResponse(request), CancellationToken.None);
                }
                else if (functionCode == 0x06)
                {
                    // WriteMultipleCoilsAsync
                    await stream.WriteAsync(MockTcpResponse.WriteMultipleCoilsResponse(request), CancellationToken.None);
                }
                else if (functionCode == 0x0F)
                {
                    // WriteRegisterAsync
                    await stream.WriteAsync(MockTcpResponse.WriteRegisterResponse(request), CancellationToken.None);
                }
                else if (functionCode == 0x10)
                {
                    // WriteMultipleRegistersAsync
                    await stream.WriteAsync(MockTcpResponse.WriteMultipleRegistersResponse(request), CancellationToken.None);
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

public class TcpModbusFixture : IDisposable
{
    public static readonly int Port = 8502;

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
