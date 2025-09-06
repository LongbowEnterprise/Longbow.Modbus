// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using System.Collections.Concurrent;
using System.Runtime.Versioning;

namespace Longbow.Modbus;

/// <summary>
/// Represents a TCP socket for network communication.
/// </summary>
[UnsupportedOSPlatform("browser")]
class DefaultModbusFactory(ITcpSocketFactory factory) : IModbusFactory
{
    private readonly ConcurrentDictionary<string, IModbusTcpClient> _pool = new();

    public IModbusTcpClient GetOrCreateTcpMaster(string? name, Action<ModbusTcpClientOptions>? valueFactory = null) => string.IsNullOrEmpty(name)
        ? CreateTcpClient(valueFactory)
        : _pool.GetOrAdd(name, key => CreateTcpClient(valueFactory));

    private DefaultModbusTcpClient CreateTcpClient(Action<ModbusTcpClientOptions>? valueFactory = null)
    {
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
        if (_pool.TryRemove(name, out var c))
        {
            client = c;
        }
        return client;
    }
}
