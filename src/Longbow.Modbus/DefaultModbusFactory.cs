// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using Longbow.TcpSocket;
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
    private readonly ConcurrentDictionary<string, IModbusTcpClient> _pool = new();

    public IModbusTcpClient GetOrCreateTcpMaster(string name, Action<ModbusTcpClientOptions>? valueFactory = null)
    {
        return _pool.GetOrAdd(name, key =>
        {
            var options = new ModbusTcpClientOptions();
            valueFactory?.Invoke(options);

            var factory = provider.GetRequiredService<ITcpSocketFactory>();
            var client = factory.GetOrCreate(name, op =>
            {
                op.ConnectTimeout = options.ConnectTimeout;
                op.SendTimeout = options.WriteTimeout;
                op.ReceiveTimeout = options.ReadTimeout;
                op.IsAutoReceive = false;
                op.IsAutoReconnect = false;
                op.LocalEndPoint = options.LocalEndPoint;
            });
            return new DefaultModbusTcpClient(client);
        });
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
