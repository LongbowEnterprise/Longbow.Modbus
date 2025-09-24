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

    public IModbusTcpClient GetOrCreateTcpMaster(string name, Action<ModbusTcpClientOptions>? configureOptions = null) => string.IsNullOrEmpty(name)
        ? CreateTcpClient(configureOptions)
        : _tcpPool.GetOrAdd(name, key => CreateTcpClient(configureOptions));

    public IModbusTcpClient GetOrCreateTcpMaster(Action<ModbusTcpClientOptions>? configureOptions = null) => CreateTcpClient(configureOptions);

    private DefaultTcpClient CreateTcpClient(Action<ModbusTcpClientOptions>? configureOptions = null)
    {
        var factory = provider.GetRequiredService<ITcpSocketFactory>();

        var options = new ModbusTcpClientOptions();
        configureOptions?.Invoke(options);

        var client = factory.GetOrCreate(configureOptions: op =>
        {
            op.ReceiveBufferSize = options.ReceiveBufferSize;
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

    public IModbusRtuClient GetOrCreateRtuMaster(string name, Action<ModbusRtuClientOptions>? configureOptions = null) => string.IsNullOrEmpty(name)
        ? CreateRtuClient(configureOptions)
        : _rtuPool.GetOrAdd(name, key => CreateRtuClient(configureOptions));

    public IModbusRtuClient GetOrCreateRtuMaster(Action<ModbusRtuClientOptions>? configureOptions = null) => CreateRtuClient(configureOptions);

    private DefaultRtuClient CreateRtuClient(Action<ModbusRtuClientOptions>? configureOptions = null)
    {
        var factory = provider.GetRequiredService<ISerialPortFactory>();

        var options = new ModbusRtuClientOptions();
        configureOptions?.Invoke(options);

        var client = factory.GetOrCreate(options.ToSerialPortOptions);
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

    public IModbusTcpClient GetOrCreateUdpMaster(string name, Action<ModbusUdpClientOptions>? configureOptions = null) => string.IsNullOrEmpty(name)
        ? CreateUdpClient(configureOptions)
        : _udpPool.GetOrAdd(name, key => CreateUdpClient(configureOptions));

    public IModbusTcpClient GetOrCreateUdpMaster(Action<ModbusUdpClientOptions>? configureOptions = null) => CreateUdpClient(configureOptions);

    private DefaultUdpClient CreateUdpClient(Action<ModbusUdpClientOptions>? configureOptions = null)
    {
        var op = new ModbusUdpClientOptions();
        configureOptions?.Invoke(op);
        return new DefaultUdpClient(op, provider.GetRequiredService<IModbusTcpMessageBuilder>());
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

    public IModbusTcpClient GetOrCreateRtuOverTcpMaster(string name, Action<ModbusTcpClientOptions>? configureOptions = null) => string.IsNullOrEmpty(name)
        ? CreateRtuOverTcpClient(configureOptions)
        : _rtuOverTcpPool.GetOrAdd(name, key => CreateRtuOverTcpClient(configureOptions));

    public IModbusTcpClient GetOrCreateRtuOverTcpMaster(Action<ModbusTcpClientOptions>? valueFactory = null) => CreateRtuOverTcpClient(valueFactory);

    private DefaultTcpClient CreateRtuOverTcpClient(Action<ModbusTcpClientOptions>? configureOptions = null)
    {
        var factory = provider.GetRequiredService<ITcpSocketFactory>();

        var options = new ModbusTcpClientOptions();
        configureOptions?.Invoke(options);

        var client = factory.GetOrCreate(configureOptions: op =>
        {
            op.ReceiveBufferSize = options.ReceiveBufferSize;
            op.IsAutoReceive = false;
            op.IsAutoReconnect = false;
            op.LocalEndPoint = options.LocalEndPoint;
        });
        var builder = provider.GetRequiredService<IModbusRtuMessageBuilder>();
        return new DefaultTcpClient(client, builder);
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

    public IModbusTcpClient GetOrCreateRtuOverUdpMaster(string name, Action<ModbusUdpClientOptions>? configureOptions = null) => string.IsNullOrEmpty(name)
        ? CreateRtuOverUdpClient(configureOptions)
        : _rtuOverUdpPool.GetOrAdd(name, key => CreateRtuOverUdpClient(configureOptions));

    public IModbusTcpClient GetOrCreateRtuOverUdpMaster(Action<ModbusUdpClientOptions>? configureOptions = null) => CreateRtuOverUdpClient(configureOptions);

    private DefaultUdpClient CreateRtuOverUdpClient(Action<ModbusUdpClientOptions>? configureOptions = null)
    {
        var op = new ModbusUdpClientOptions();
        configureOptions?.Invoke(op);
        return new DefaultUdpClient(op, provider.GetRequiredService<IModbusRtuMessageBuilder>());
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
