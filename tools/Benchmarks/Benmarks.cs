// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using BenchmarkDotNet.Attributes;
using Longbow.Modbus;
using Longbow.TcpSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Sockets;
using TouchSocket.Modbus;
using TouchSocket.Sockets;

namespace UnitTest.Benchmarks;

[MemoryDiagnoser]
public class Benchmarks
{
    private readonly List<IModbusClient> _lgbModbusClients = [];
    private readonly List<NModbus.IModbusMaster> _nModbusClients = [];
    private readonly List<ModbusTcpMaster> _touchSocketModbusClients = [];

    private const int ClientCount = 10;
    private const int TaskNumberOfClient = 10;
    private const int NumberOfTask = 10;
    private const int Quantity = 100;

    public Benchmarks()
    {
        Task.WaitAll(InitLongbowModbus(), InitNModbus(), InitTouchSocketModbus());
    }

    private async Task InitTouchSocketModbus()
    {
        for (var index = 0; index < ClientCount; index++)
        {
            var client = new TouchSocket.Modbus.ModbusTcpMaster();
            await client.SetupAsync(new TouchSocket.Core.TouchSocketConfig().SetRemoteIPHost("127.0.0.1:502"));
            await client.ConnectAsync();
            await client.ReadHoldingRegistersAsync(0x01, 0x00, Quantity);
            _touchSocketModbusClients.Add(client);
        }
    }

    private async Task InitNModbus()
    {
        var nModbusFactory = new NModbus.ModbusFactory();

        for (var index = 0; index < ClientCount; index++)
        {
            var client = nModbusFactory.CreateMaster(new System.Net.Sockets.TcpClient("127.0.0.1", 502));
            await client.ReadHoldingRegistersAsync(0x01, 0x00, Quantity);

            _nModbusClients.Add(client);
        }
    }

    private async Task InitLongbowModbus()
    {
        var sc = new ServiceCollection();
        sc.AddModbusFactory();

        var provider = sc.BuildServiceProvider();
        var factory = provider.GetRequiredService<IModbusFactory>();

        for (var index = 0; index < ClientCount; index++)
        {
            var client = factory.GetOrCreateTcpMaster();
            await client.ConnectAsync("127.0.0.1", 502);
            await client.ReadHoldingRegistersAsync(0x01, 0x00, Quantity);

            _lgbModbusClients.Add(client);
        }
    }

    [Benchmark]
    public async Task LongbowModbus()
    {
        var tasks = _lgbModbusClients.SelectMany(c =>
        {
            var tasks = new List<Task>();
            for (int i = 0; i < TaskNumberOfClient; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    for (int i = 0; i < NumberOfTask; i++)
                    {
                        await c.ReadHoldingRegistersAsync(1, 0, Quantity);
                    }
                }));
            }
            return tasks;
        }).ToList();

        await Task.WhenAll(tasks);
    }

    //[Benchmark]
    public async Task NModbus()
    {
        var tasks = _nModbusClients.SelectMany(c =>
        {
            var tasks = new List<Task>();
            for (int i = 0; i < TaskNumberOfClient; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    for (int i = 0; i < NumberOfTask; i++)
                    {
                        await c.ReadHoldingRegistersAsync(1, 0, Quantity);
                    }
                }));
            }
            return tasks;
        }).ToList();

        await Task.WhenAll(tasks);
    }

    //[Benchmark]
    public async Task TouchSocketModbus()
    {
        var tasks = _touchSocketModbusClients.SelectMany(c =>
        {
            var tasks = new List<Task>();
            for (int i = 0; i < TaskNumberOfClient; i++)
            {
                var task = Task.Run(async () =>
                {
                    for (int i = 0; i < NumberOfTask; i++)
                    {
                        await c.ReadHoldingRegistersAsync(1, 0, Quantity);
                    }
                });
                tasks.Add(task);
            }
            return tasks;
        }).ToList();

        await Task.WhenAll(tasks);
    }
}
