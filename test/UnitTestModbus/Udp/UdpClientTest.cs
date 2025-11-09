// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace UnitTest.Udp;

[Collection("MockUdpModbus")]
public class UdpClientTest
{
    [Fact]
    public async Task ReadCoilsAsync_Ok()
    {
        var sc = new ServiceCollection();
        sc.AddModbusFactory();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();
        await using var client = factory.GetOrCreateUdpMaster(op =>
        {
            op.LocalEndPoint = new(IPAddress.Any, 0);
        });

        // 连接 Master
        await client.ConnectAsync("127.0.0.1", UdpModbusFixture.Port);
        var response = await client.ReadCoilsAsync(0x01, 0, 10);
        Assert.NotNull(response);
        Assert.Equal(10, response.ReadBoolValues(10).Length);

        await using var client2 = factory.GetOrCreateUdpMaster("test");
        Assert.NotEqual(client, client2);

        var client3 = factory.GetOrCreateUdpMaster("test");
        Assert.Equal(client2, client3);

        factory.RemoveUdpMaster("test");
    }

    [Fact]
    public async Task ReadInputsAsync_Ok()
    {
        var sc = new ServiceCollection();
        sc.AddModbusFactory();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();
        var ipAddress = TcpSocketUtility.ConvertToIPAddress("127.0.0.1");
        await using var client = factory.GetOrCreateUdpMaster("test", op => op.LocalEndPoint = new IPEndPoint(ipAddress, 0));

        // 连接 Master
        await client.ConnectAsync("127.0.0.1", UdpModbusFixture.Port);
        var response = await client.ReadInputsAsync(0x01, 0, 10);
        Assert.NotNull(response);
        Assert.Equal(10, response.ReadBoolValues(10).Length);
    }

    [Fact]
    public async Task ReadHoldingRegistersAsync_Ok()
    {
        var sc = new ServiceCollection();
        sc.AddModbusFactory();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();
        await using var client = factory.GetOrCreateUdpMaster("");

        // 连接 Master
        await client.ConnectAsync("127.0.0.1", UdpModbusFixture.Port);
        var response = await client.ReadHoldingRegistersAsync(0x01, 0, 10);
        Assert.NotNull(response);
        Assert.Equal(10, response.ReadUShortValues(10).Length);
    }

    [Fact]
    public async Task ReadInputRegistersAsync_Ok()
    {
        var sc = new ServiceCollection();
        sc.AddModbusFactory();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();
        await using var client = factory.GetOrCreateUdpMaster("test");

        // 连接 Master
        await client.ConnectAsync("127.0.0.1", UdpModbusFixture.Port);
        var response = await client.ReadInputRegistersAsync(0x01, 0, 10);
        Assert.NotNull(response);
        Assert.Equal(10, response.ReadUShortValues(10).Length);
    }

    [Fact]
    public async Task WriteCoilAsync_Ok()
    {
        var sc = new ServiceCollection();
        sc.AddModbusFactory();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();
        await using var client = factory.GetOrCreateUdpMaster("test");

        // 连接 Master
        await client.ConnectAsync("127.0.0.1", UdpModbusFixture.Port);
        var response = await client.WriteCoilAsync(0x01, 0, true);
        Assert.True(response.IsSuccess);

        response = await client.WriteCoilAsync(0x01, 0, false);
        Assert.True(response.IsSuccess);
    }

    [Fact]
    public async Task WriteMultipleCoilsAsync_Ok()
    {
        var sc = new ServiceCollection();
        sc.AddModbusFactory();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();
        await using var client = factory.GetOrCreateUdpMaster("test");

        // 连接 Master
        await client.ConnectAsync("127.0.0.1", UdpModbusFixture.Port);
        var response = await client.WriteMultipleCoilsAsync(0x01, 0, [true, true, true, true, true, true, true, true, false, true]);
        Assert.True(response.IsSuccess);
    }

    [Fact]
    public async Task WriteRegisterAsync()
    {
        var sc = new ServiceCollection();
        sc.AddModbusFactory();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();
        await using var client = factory.GetOrCreateUdpMaster("test");

        // 连接 Master
        await client.ConnectAsync("127.0.0.1", UdpModbusFixture.Port);
        var response = await client.WriteRegisterAsync(0x01, 0, 12);
        Assert.True(response.IsSuccess);
    }

    [Fact]
    public async Task WriteMultipleRegistersAsync_Ok()
    {
        var sc = new ServiceCollection();
        sc.AddModbusFactory();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();
        await using var client = factory.GetOrCreateUdpMaster("test");

        // 连接 Master
        await client.ConnectAsync("127.0.0.1", UdpModbusFixture.Port);
        var response = await client.WriteMultipleRegistersAsync(0x01, 0, [14, 0, 23, 0, 46, 0, 01, 02, 04, 05]);
        Assert.True(response.IsSuccess);
    }
}
