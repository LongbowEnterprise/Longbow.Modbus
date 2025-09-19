// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace UnitTest;

[Collection("MockUdpModbus")]
public class UdpClientTest
{
    [Fact]
    public async Task Connect_Exception()
    {
        var sc = new ServiceCollection();
        sc.AddModbusFactory();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();
        await using var client = factory.GetOrCreateUdpMaster();

        // 未连接 Master 直接读取
        var ex = await Assert.ThrowsAnyAsync<InvalidOperationException>(async () =>
        {
            await client.ReadCoilsAsync(0x01, 0, 10);
        });
        Assert.NotNull(ex);
    }

    [Fact]
    public async Task ReadCoilsAsync_Ok()
    {
        var sc = new ServiceCollection();
        sc.AddModbusFactory();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();
        await using var client = factory.GetOrCreateUdpMaster(op =>
        {
            op.ConnectTimeout = 1000;
            op.LocalEndPoint = new(IPAddress.Any, 0);
        });

        // 连接 Master
        await client.ConnectAsync("127.0.0.1", 504);
        var response = await client.ReadCoilsAsync(0x01, 0, 10);
        Assert.NotNull(response);
        Assert.Equal(10, response.Length);

        await using var client2 = factory.GetOrCreateUdpMaster();
        Assert.NotEqual(client, client2);

        factory.RemoveTcpMaster("test");
    }

    [Fact]
    public async Task ReadInputsAsync_Ok()
    {
        var sc = new ServiceCollection();
        sc.AddModbusFactory();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();
        await using var client = factory.GetOrCreateUdpMaster();

        // 连接 Master
        await client.ConnectAsync("127.0.0.1", 504);
        var response = await client.ReadInputsAsync(0x01, 0, 10);
        Assert.NotNull(response);
        Assert.Equal(10, response.Length);
    }

    [Fact]
    public async Task ReadHoldingRegistersAsync_Ok()
    {
        var sc = new ServiceCollection();
        sc.AddModbusFactory();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();
        await using var client = factory.GetOrCreateUdpMaster("test");

        // 连接 Master
        await client.ConnectAsync("127.0.0.1", 504);
        var response = await client.ReadHoldingRegistersAsync(0x01, 0, 10);
        Assert.NotNull(response);
        Assert.Equal(10, response.Length);
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
        await client.ConnectAsync("127.0.0.1", 504);
        var response = await client.ReadInputRegistersAsync(0x01, 0, 10);
        Assert.NotNull(response);
        Assert.Equal(10, response.Length);
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
        await client.ConnectAsync("127.0.0.1", 504);
        var response = await client.WriteCoilAsync(0x01, 0, true);
        Assert.True(response);

        response = await client.WriteCoilAsync(0x01, 0, false);
        Assert.True(response);
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
        await client.ConnectAsync("127.0.0.1", 504);
        var response = await client.WriteMultipleCoilsAsync(0x01, 0, [true, true, true, true, true, true, true, true, false, true]);
        Assert.True(response);
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
        await client.ConnectAsync("127.0.0.1", 504);
        var response = await client.WriteRegisterAsync(0x01, 0, 12);
        Assert.True(response);
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
        await client.ConnectAsync("127.0.0.1", 504);
        var response = await client.WriteMultipleRegistersAsync(0x01, 0, [14, 0, 23, 0, 46, 0, 01, 02, 04, 05]);
        Assert.True(response);
    }
}
