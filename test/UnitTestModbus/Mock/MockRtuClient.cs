// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using Longbow.SerialPorts;
using Microsoft.Extensions.Options;
using System.Net.Sockets;
using UnitTest;

namespace Microsoft.Extensions.DependencyInjection;

internal static class MockRtuClient
{
    public static IServiceCollection AddMockRtuClientService(this IServiceCollection services)
    {
        services.AddModbusFactory();
        services.AddTransient<ISerialPortClient, MockSerialPortClient>();
        return services;
    }
}

class MockSerialPortClient(IOptions<SerialPortOptions> options) : ISerialPortClient
{
    public bool IsOpen => Options.PortName != "COM3";

    private readonly SerialPortOptions _options = CopyTo(options.Value);

    public SerialPortOptions Options => _options;

    public ValueTask<bool> OpenAsync(CancellationToken token = default) => Options.PortName == "COM3" ? throw new SocketException((int)SocketError.ConnectionReset) : ValueTask.FromResult(true);

    private Memory<byte> _buffer = Memory<byte>.Empty;
    public ValueTask<bool> SendAsync(ReadOnlyMemory<byte> data, CancellationToken token = default)
    {
        _buffer = new Memory<byte>(new byte[data.Length]);
        data.CopyTo(_buffer);
        return ValueTask.FromResult(true);
    }

    public ValueTask<ReadOnlyMemory<byte>> ReceiveAsync(CancellationToken token = default)
    {
        if (_buffer.Length < 8)
        {
            throw new IOException("Invalid Modbus RTU request.");
        }

        var ret = ReadOnlyMemory<byte>.Empty;
        var functionCode = _buffer.Span[1];
        if (functionCode == 0x01)
        {
            // ReadCoilAsync
            ret = MockRtuResponse.ReadCoilResponse();
        }
        else if (functionCode == 0x02)
        {
            // ReadInputsAsync
            ret = MockRtuResponse.ReadInputsResponse();
        }
        else if (functionCode == 0x03)
        {
            // ReadHoldingRegistersAsync
            ret = MockRtuResponse.ReadHoldingRegistersResponse();
        }
        else if (functionCode == 0x04)
        {
            // ReadInputRegistersAsync
            ret = MockRtuResponse.ReadInputRegistersResponse();
        }
        else if (functionCode == 0x05)
        {
            // WriteCoilAsync
            ret = MockRtuResponse.WriteCoilResponse(_buffer);
        }
        else if (functionCode == 0x06)
        {
            // WriteMultipleCoilsAsync
            ret = MockRtuResponse.WriteMultipleCoilsResponse();
        }
        else if (functionCode == 0x0F)
        {
            // WriteRegisterAsync
            ret = MockRtuResponse.WriteRegisterResponse();
        }
        else if (functionCode == 0x10)
        {
            // WriteMultipleRegistersAsync
            ret = MockRtuResponse.WriteMultipleRegistersResponse();
        }

        return ValueTask.FromResult(ret);
    }

    public ValueTask CloseAsync() => ValueTask.CompletedTask;

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public static SerialPortOptions CopyTo(SerialPortOptions source) => new()
    {
        BaudRate = source.BaudRate,
        DataBits = source.DataBits,
        Parity = source.Parity,
        PortName = source.PortName,
        StopBits = source.StopBits,
        DiscardNull = source.DiscardNull,
        Handshake = source.Handshake,
        ReadBufferSize = source.ReadBufferSize,
        WriteBufferSize = source.WriteBufferSize,
        ReadTimeout = source.ReadTimeout,
        WriteTimeout = source.WriteTimeout,
        RtsEnable = source.RtsEnable,
        DtrEnable = source.DtrEnable
    };
}
