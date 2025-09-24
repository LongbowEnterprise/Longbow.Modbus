// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace UnitTest.Tcp;

[Collection("MockTcpModbus")]
public class TcpClientTest
{
    [Fact]
    public async Task Connect_Exception()
    {
        var sc = new ServiceCollection();
        sc.AddModbusFactory();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();
        await using var client = factory.GetOrCreateTcpMaster();

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
        await using var client = factory.GetOrCreateTcpMaster(op =>
        {
            op.LocalEndPoint = new(IPAddress.Any, 0);
            op.ReceiveBufferSize = 1024;
            op.NoDelay = false;
        });

        // 连接 Master
        await client.ConnectAsync("127.0.0.1", TcpModbusFixture.Port);
        var response = await client.ReadCoilsAsync(0x01, 0, 10);
        Assert.NotNull(response);
        Assert.Equal(10, response.ReadBoolValues(10).Length);

        await using var client2 = factory.GetOrCreateTcpMaster();
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
        await using var client = factory.GetOrCreateTcpMaster("test");

        // 连接 Master
        await client.ConnectAsync("127.0.0.1", TcpModbusFixture.Port);
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
        await using var client = factory.GetOrCreateTcpMaster("test", op =>
        {
            op.NoDelay = false;
        });

        // 连接 Master
        await client.ConnectAsync("127.0.0.1", TcpModbusFixture.Port);
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
        await using var client = factory.GetOrCreateTcpMaster("");

        // 连接 Master
        await client.ConnectAsync("127.0.0.1", TcpModbusFixture.Port);
        var response = await client.ReadInputRegistersAsync(0x01, 0, 10);
        Assert.NotNull(response);
        Assert.Equal(10, response.ReadUShortValues(10).Length);
    }

    [Fact]
    public async Task ReadInputRegistersAsync_Failed()
    {
        var sc = new ServiceCollection();
        sc.AddModbusFactory();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();
        await using var client = factory.GetOrCreateTcpMaster("test", op =>
        {
            op.NoDelay = false;
        });

        // 读取寄存器，模拟响应不正确逻辑
        await client.ConnectAsync("127.0.0.1", TcpModbusFixture.Port);
        var response = await client.ReadInputRegistersAsync(0x01, 0, 20);
        Assert.NotNull(response);
        Assert.ThrowsAny<IndexOutOfRangeException>(() => response.ReadUShortValues(20));
        Assert.NotNull(response.Exception);
    }

    [Fact]
    public async Task WriteCoilAsync_Ok()
    {
        var sc = new ServiceCollection();
        sc.AddModbusFactory();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();
        await using var client = factory.GetOrCreateTcpMaster("test");

        // 连接 Master
        await client.ConnectAsync("127.0.0.1", TcpModbusFixture.Port);
        var response = await client.WriteCoilAsync(0x01, 0, true);
        Assert.True(response.IsSuccess);

        response = await client.WriteCoilAsync(0x01, 0, false);
        Assert.True(response.IsSuccess);

        factory.RemoveTcpMaster("test");
    }

    [Fact]
    public async Task WriteCoilAsync_Failed()
    {
        var sc = new ServiceCollection();
        sc.AddModbusFactory();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();
        await using var client = factory.GetOrCreateTcpMaster("test");

        // 连接 Master
        await client.ConnectAsync("127.0.0.1", TcpModbusFixture.Port);
        var response = await client.WriteCoilAsync(0x01, 1, true);
        Assert.False(response.IsSuccess);
    }

    [Fact]
    public async Task WriteMultipleCoilsAsync_Ok()
    {
        var sc = new ServiceCollection();
        sc.AddModbusFactory();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();
        await using var client = factory.GetOrCreateTcpMaster("test");

        // 连接 Master
        await client.ConnectAsync("127.0.0.1", TcpModbusFixture.Port);
        var response = await client.WriteMultipleCoilsAsync(0x01, 0, [true, true, true, true, true, true, true, true, false, true]);
        Assert.True(response.IsSuccess);
    }

    [Fact]
    public async Task WriteRegisterAsync_Ok()
    {
        var sc = new ServiceCollection();
        sc.AddModbusFactory();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();
        await using var client = factory.GetOrCreateTcpMaster("test");

        // 连接 Master
        await client.ConnectAsync("127.0.0.1", TcpModbusFixture.Port);
        var response = await client.WriteRegisterAsync(0x01, 0, 12);
        Assert.True(response.IsSuccess);
    }

    [Fact]
    public async Task WriteRegisterAsync_Failed()
    {
        var sc = new ServiceCollection();
        sc.AddModbusFactory();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();
        await using var client = factory.GetOrCreateTcpMaster("test");

        // 模拟响应不正确逻辑
        await client.ConnectAsync("127.0.0.1", TcpModbusFixture.Port);
        var response = await client.WriteRegisterAsync(0x01, 1, 12);
        Assert.False(response.IsSuccess);
    }

    [Fact]
    public async Task WriteMultipleRegistersAsync_Ok()
    {
        var sc = new ServiceCollection();
        sc.AddModbusFactory();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();
        await using var client = factory.GetOrCreateTcpMaster("test");

        // 连接 Master
        await client.ConnectAsync("127.0.0.1", TcpModbusFixture.Port);
        var response = await client.WriteMultipleRegistersAsync(0x01, 0, [12, 0, 23, 0, 46, 0, 01, 02, 04, 05]);
        Assert.True(response.IsSuccess);
    }

    [Fact]
    public async Task ThreadSafe_Ok()
    {
        var clientCount = 1;
        var taskCount = 5;
        var sc = new ServiceCollection();
        sc.AddModbusFactory();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();

        var clients = new List<IModbusClient>();
        for (var index = 0; index < clientCount; index++)
        {
            var client = factory.GetOrCreateTcpMaster();
            await client.ConnectAsync("127.0.0.1", TcpModbusFixture.Port);

            clients.Add(client);
        }

        var results = new List<ushort[]>();
        var tasks = clients.SelectMany(c =>
        {
            var tasks = new List<Task>();
            for (int i = 0; i < taskCount; i++)
            {
                var task = Task.Run(async () =>
                {
                    var v = await c.ReadHoldingRegistersAsync(1, 0, 10);
                    results.Add(v.ReadUShortValues(10));
                });
                tasks.Add(task);
            }
            return tasks;
        }).ToList();

        await Task.WhenAll(tasks);

        Assert.Equal(clientCount * taskCount, results.Count);

        var failed = results.Count(i => i.All(v => v == 0));
        Assert.Equal(0, failed);
    }
}
