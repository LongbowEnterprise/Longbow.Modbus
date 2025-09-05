// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using Microsoft.Extensions.DependencyInjection;

namespace UnitTestModbus;

public class TcpClientTest
{
    [Fact]
    public async Task ReadCoilsAsync_Ok()
    {
        var sc = new ServiceCollection();
        sc.AddTcpSocketFactory();
        sc.AddModbusFactory();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();
        await using var client = factory.GetOrCreateTcpMaster("test");

        // 连接 Master
        await client.ConnectAsync("127.0.0.1", 502);
        var response = await client.ReadCoilsAsync(0x01, 0, 10);
        Assert.NotNull(response);
        Assert.Equal(10, response.Length);

        response = await client.ReadCoilsAsync(0x01, 0, 5);
        Assert.NotNull(response);
        Assert.Equal(5, response.Length);
    }

    [Fact]
    public async Task ReadInputsAsync_Ok()
    {
        var sc = new ServiceCollection();
        sc.AddTcpSocketFactory();
        sc.AddModbusFactory();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();
        await using var client = factory.GetOrCreateTcpMaster("test");

        // 连接 Master
        await client.ConnectAsync("127.0.0.1", 502);
        var response = await client.ReadInputsAsync(0x01, 0, 10);
        Assert.NotNull(response);
        Assert.Equal(10, response.Length);
    }

    [Fact]
    public async Task ReadHoldingRegistersAsync_Ok()
    {
        var sc = new ServiceCollection();
        sc.AddTcpSocketFactory();
        sc.AddModbusFactory();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();
        await using var client = factory.GetOrCreateTcpMaster("test");

        // 连接 Master
        await client.ConnectAsync("127.0.0.1", 502);
        var response = await client.ReadHoldingRegistersAsync(0x01, 0, 10);
        Assert.NotNull(response);
        Assert.Equal(10, response.Length);
    }

    [Fact]
    public async Task ReadInputRegistersAsync_Ok()
    {
        var sc = new ServiceCollection();
        sc.AddTcpSocketFactory();
        sc.AddModbusFactory();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();
        await using var client = factory.GetOrCreateTcpMaster("test");

        // 连接 Master
        await client.ConnectAsync("127.0.0.1", 502);
        var response = await client.ReadInputRegistersAsync(0x01, 0, 10);
        Assert.NotNull(response);
        Assert.Equal(10, response.Length);
    }

    [Fact]
    public async Task WriteCoilAsync_Ok()
    {
        var sc = new ServiceCollection();
        sc.AddTcpSocketFactory();
        sc.AddModbusFactory();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();
        await using var client = factory.GetOrCreateTcpMaster("test");

        // 连接 Master
        await client.ConnectAsync("127.0.0.1", 502);
        var response = await client.WriteCoilAsync(0x01, 0, true);
        Assert.True(response);
    }

    [Fact]
    public async Task WriteMultipleCoilsAsync_Ok()
    {
        var sc = new ServiceCollection();
        sc.AddTcpSocketFactory();
        sc.AddModbusFactory();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();
        await using var client = factory.GetOrCreateTcpMaster("test");

        // 连接 Master
        await client.ConnectAsync("127.0.0.1", 502);
        var response = await client.WriteMultipleCoilsAsync(0x01, 0, [true, true, true, true, true, true, true, true, false, true]);
        Assert.True(response);
    }

    [Fact]
    public async Task WriteRegisterAsync()
    {
        var sc = new ServiceCollection();
        sc.AddTcpSocketFactory();
        sc.AddModbusFactory();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();
        await using var client = factory.GetOrCreateTcpMaster("test");

        // 连接 Master
        await client.ConnectAsync("127.0.0.1", 502);
        var response = await client.WriteRegisterAsync(0x01, 0, 12);
        Assert.True(response);
    }

    [Fact]
    public async Task WriteMultipleRegistersAsync_Ok()
    {
        var sc = new ServiceCollection();
        sc.AddTcpSocketFactory();
        sc.AddModbusFactory();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();
        await using var client = factory.GetOrCreateTcpMaster("test");

        // 连接 Master
        await client.ConnectAsync("127.0.0.1", 502);
        var response = await client.WriteMultipleRegistersAsync(0x01, 0, [12, 0, 23, 0, 46, 0, 01, 02, 04, 05]);
        Assert.True(response);
    }
}
