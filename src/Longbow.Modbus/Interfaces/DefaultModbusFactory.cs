// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using Longbow.SerialPorts;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Longbow.Modbus;

/// <summary>
/// Represents a TCP socket for network communication.
/// </summary>
class DefaultModbusFactory(IServiceProvider provider) : IModbusFactory
{
    private readonly ConcurrentDictionary<string, IModbusTcpClient> _tcpPool = new();
    private readonly ConcurrentDictionary<string, IModbusRtuClient> _rtuPool = new();
    private readonly ConcurrentDictionary<string, IModbusTcpClient> _udpPool = new();
    private readonly ConcurrentDictionary<string, IModbusTcpClient> _rtuOverTcpPool = new();
    private readonly ConcurrentDictionary<string, IModbusTcpClient> _rtuOverUdpPool = new();

    public IModbusTcpClient GetOrCreateTcpMaster(string? name, Action<ModbusTcpClientOptions>? valueFactory = null) => string.IsNullOrEmpty(name)
        ? CreateTcpClient(valueFactory)
        : _tcpPool.GetOrAdd(name, key => CreateTcpClient(valueFactory));

    private DefaultTcpClient CreateTcpClient(Action<ModbusTcpClientOptions>? valueFactory = null)
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
            op.NoDelay = options.NoDelay;
        });
        var builder = provider.GetRequiredService<IModbusTcpMessageBuilder>();
        return new DefaultTcpClient(client, builder);
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

    public IModbusRtuClient GetOrCreateRtuMaster(string? name = null, Action<ModbusRtuClientOptions>? valueFactory = null) => string.IsNullOrEmpty(name)
        ? CreateRtuClient(valueFactory)
        : _rtuPool.GetOrAdd(name, key => CreateRtuClient(valueFactory));


    private DefaultRtuClient CreateRtuClient(Action<ModbusRtuClientOptions>? valueFactory = null)
    {
        var factory = provider.GetRequiredService<ISerialPortFactory>();

        var options = new ModbusRtuClientOptions();
        valueFactory?.Invoke(options);

        var client = factory.GetOrCreate(valueFactory: options.ToSerialPortOptions);
        var builder = provider.GetRequiredService<IModbusRtuMessageBuilder>();
        return new DefaultRtuClient(client, builder);
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

    public IModbusTcpClient GetOrCreateUdpMaster(string? name = null, Action<ModbusUdpClientOptions>? valueFactory = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            var options = new ModbusUdpClientOptions();
            valueFactory?.Invoke(options);
            return new DefaultUdpClient(options, provider.GetRequiredService<IModbusTcpMessageBuilder>());
        }
        if (_udpPool.TryGetValue(name, out var client))
        {
            return client;
        }


        var op = new ModbusUdpClientOptions();
        valueFactory?.Invoke(op);
        client = new DefaultUdpClient(op, provider.GetRequiredService<IModbusTcpMessageBuilder>());
        _udpPool.TryAdd(name, client);
        return client;
    }

    public IModbusTcpClient? RemoveUdpMaster(string name)
    {
        IModbusTcpClient? client = null;
        if (_udpPool.TryRemove(name, out var c))
        {
            client = c;
        }
        return client;
    }

    public IModbusTcpClient GetOrCreateRtuOverTcpMaster(string? name = null, Action<ModbusTcpClientOptions>? valueFactory = null) => string.IsNullOrEmpty(name)
        ? CreateRtuOverTcpClient(valueFactory)
        : _rtuOverTcpPool.GetOrAdd(name, key => CreateRtuOverTcpClient(valueFactory));

    private DefaultRtuOverTcpClient CreateRtuOverTcpClient(Action<ModbusTcpClientOptions>? valueFactory = null)
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
        var builder = provider.GetRequiredService<IModbusRtuMessageBuilder>();
        return new DefaultRtuOverTcpClient(client, builder);
    }

    public IModbusTcpClient? RemoveRtuOverTcpMaster(string name)
    {
        IModbusTcpClient? client = null;
        if (_rtuOverTcpPool.TryRemove(name, out var c))
        {
            client = c;
        }
        return client;
    }

    public IModbusTcpClient GetOrCreateRtuOverUdpMaster(string? name = null, Action<ModbusUdpClientOptions>? valueFactory = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            var options = new ModbusUdpClientOptions();
            valueFactory?.Invoke(options);
            return new DefaultModbusRtuOverUpdClient(options, provider.GetRequiredService<IModbusRtuMessageBuilder>());
        }
        if (_rtuOverUdpPool.TryGetValue(name, out var client))
        {
            return client;
        }


        var op = new ModbusUdpClientOptions();
        valueFactory?.Invoke(op);
        client = new DefaultModbusRtuOverUpdClient(op, provider.GetRequiredService<IModbusRtuMessageBuilder>());
        _rtuOverUdpPool.TryAdd(name, client);
        return client;
    }

    public IModbusTcpClient? RemoveRtuOverUdpMaster(string name)
    {
        IModbusTcpClient? client = null;
        if (_rtuOverUdpPool.TryRemove(name, out var c))
        {
            client = c;
        }
        return client;
    }
}
