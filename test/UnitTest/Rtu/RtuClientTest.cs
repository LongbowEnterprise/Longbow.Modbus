// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using Microsoft.Extensions.DependencyInjection;

namespace UnitTest.Rtu;

public class RtuClientTest
{
    [Fact]
    public async Task Connect_Exception()
    {
        var sc = new ServiceCollection();
        sc.AddMockRtuClientService();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();
        await using var client = factory.GetOrCreateRtuMaster("test", op =>
        {
            op.PortName = "COM3";
        });

        // 连接 Master
        await Assert.ThrowsAnyAsync<Exception>(async () => await client.ConnectAsync());
    }

    [Fact]
    public async Task ReadCoilsAsync_Ok()
    {
        var sc = new ServiceCollection();
        sc.AddMockRtuClientService();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();
        await using var client = factory.GetOrCreateRtuMaster(op =>
        {
            op.PortName = "COM1";
            op.DiscardNull = false;
            op.BaudRate = 9600;
            op.DataBits = 8;
            op.Parity = System.IO.Ports.Parity.None;
            op.StopBits = System.IO.Ports.StopBits.One;
            op.RtsEnable = false;
            op.DtrEnable = false;
            op.Handshake = System.IO.Ports.Handshake.None;
            op.ReadBufferSize = 4096;
            op.WriteBufferSize = 2048;
            op.RtsEnable = false;
            op.DtrEnable = false;
        });

        // 连接 Master
        var connected = await client.ConnectAsync();
        Assert.True(connected);

        var response = await client.ReadCoilsAsync(0x01, 0, 10);
        Assert.NotNull(response);
        Assert.Equal(10, response.ReadBoolValues(10).Length);

        response = await client.ReadCoilsAsync(0x01, 0, 5);
        Assert.NotNull(response);
        Assert.Equal(5, response.ReadBoolValues(5).Length);

        var client2 = factory.GetOrCreateRtuMaster("test");
        Assert.NotEqual(client, client2);

        var client3 = factory.GetOrCreateRtuMaster("test");
        Assert.Equal(client2, client3);

        await using var client4 = factory.RemoveRtuMaster("test");
    }

    [Fact]
    public async Task ReadInputsAsync_Ok()
    {
        var sc = new ServiceCollection();
        sc.AddMockRtuClientService();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();
        await using var client = factory.GetOrCreateRtuMaster("");

        // 连接 Master
        await client.ConnectAsync();
        var response = await client.ReadInputsAsync(0x01, 0, 10);
        Assert.NotNull(response);
        Assert.Equal(10, response.ReadBoolValues(10).Length);
    }

    [Fact]
    public async Task ReadHoldingRegistersAsync_Ok()
    {
        var sc = new ServiceCollection();
        sc.AddMockRtuClientService();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();
        await using var client = factory.GetOrCreateRtuMaster("test");

        // 连接 Master
        await client.ConnectAsync();
        var response = await client.ReadHoldingRegistersAsync(0x01, 0, 10);
        Assert.NotNull(response);
        Assert.Equal(10, response.ReadUShortValues(10).Length);
    }

    [Fact]
    public async Task ReadInputRegistersAsync_Ok()
    {
        var sc = new ServiceCollection();
        sc.AddMockRtuClientService();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();
        await using var client = factory.GetOrCreateRtuMaster("test");

        // 连接 Master
        await client.ConnectAsync();
        var response = await client.ReadInputRegistersAsync(0x01, 0, 10);
        Assert.NotNull(response);
        Assert.Equal(10, response.ReadUShortValues(10).Length);
    }

    [Fact]
    public async Task WriteCoilAsync_Ok()
    {
        var sc = new ServiceCollection();
        sc.AddMockRtuClientService();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();
        await using var client = factory.GetOrCreateRtuMaster("test");

        // 连接 Master
        await client.ConnectAsync();
        var response = await client.WriteCoilAsync(0x01, 0, true);
        Assert.True(response.IsSuccess);

        response = await client.WriteCoilAsync(0x01, 1, false);
        Assert.True(response.IsSuccess);
    }

    [Fact]
    public async Task WriteMultipleCoilsAsync_Ok()
    {
        var sc = new ServiceCollection();
        sc.AddMockRtuClientService();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();
        await using var client = factory.GetOrCreateRtuMaster("test");

        // 连接 Master
        await client.ConnectAsync();
        var response = await client.WriteMultipleCoilsAsync(0x01, 0, [true, true, true, true, true, true, true, true, false, true]);
        Assert.True(response.IsSuccess);
    }

    [Fact]
    public async Task WriteRegisterAsync()
    {
        var sc = new ServiceCollection();
        sc.AddMockRtuClientService();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();
        await using var client = factory.GetOrCreateRtuMaster("test");

        // 连接 Master
        await client.ConnectAsync();
        var response = await client.WriteRegisterAsync(0x01, 0, 12);
        Assert.True(response.IsSuccess);
    }

    [Fact]
    public async Task WriteMultipleRegistersAsync_Ok()
    {
        var sc = new ServiceCollection();
        sc.AddMockRtuClientService();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();
        await using var client = factory.GetOrCreateRtuMaster("test");

        // 连接 Master
        await client.ConnectAsync();
        var response = await client.WriteMultipleRegistersAsync(0x01, 0, [12, 0, 23, 0, 46, 0, 01, 02, 04, 05]);
        Assert.True(response.IsSuccess);
    }
}
