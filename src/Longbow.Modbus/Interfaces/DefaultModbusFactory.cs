// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Runtime.Versioning;

namespace Longbow.Modbus;

/// <summary>
/// Represents a TCP socket for network communication.
/// </summary>
[UnsupportedOSPlatform("browser")]
class DefaultModbusFactory(IServiceProvider provider) : IModbusFactory
{
    private readonly ConcurrentDictionary<string, IModbusTcpClient> _tcpPool = new();
    private readonly ConcurrentDictionary<string, IModbusRtuClient> _rtuPool = new();

    public IModbusTcpClient GetOrCreateTcpMaster(string? name, Action<ModbusTcpClientOptions>? valueFactory = null) => string.IsNullOrEmpty(name)
        ? CreateTcpClient(valueFactory)
        : _tcpPool.GetOrAdd(name, key => CreateTcpClient(valueFactory));

    private DefaultModbusTcpClient CreateTcpClient(Action<ModbusTcpClientOptions>? valueFactory = null)
    {
        var factory = provider.GetRequiredService<ITcpSocketFactory>();

        var options = new ModbusTcpClientOptions();
        valueFactory?.Invoke(options);

        var client = factory.GetOrCreate(valueFactory: op =>
        {
            op.ConnectTimeout = options.ConnectTimeout;
            op.SendTimeout = options.WriteTimeout;
            op.ReceiveTimeout = options.ReadTimeout;
            op.IsAutoReceive = false;
            op.IsAutoReconnect = false;
            op.LocalEndPoint = options.LocalEndPoint;
        });
        return new DefaultModbusTcpClient(client);
    }

    public IModbusTcpClient? RemoveTcpMaster(string name)
    {
        IModbusTcpClient? client = null;
        if (_tcpPool.TryRemove(name, out var c))
        {
            client = c;
        }
        return client;
    }

    public IModbusRtuClient GetOrCreateRtuMaster(string? name = null, Action<ModbusRtuClientOptions>? valueFactory = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            var options = new ModbusRtuClientOptions();
            return new DefaultModbusRtuClient(options);
        }

        if (_rtuPool.TryGetValue(name, out var client))
        {
            return client;
        }

        var op = new ModbusRtuClientOptions();
        valueFactory?.Invoke(op);
        client = new DefaultModbusRtuClient(op);
        _rtuPool.TryAdd(name, client);
        return client;
    }

    public IModbusRtuClient? RemoveRtuMaster(string name)
    {
        IModbusRtuClient? client = null;
        if (_rtuPool.TryRemove(name, out var c))
        {
            client = c;
        }
        return client;
    }
}
