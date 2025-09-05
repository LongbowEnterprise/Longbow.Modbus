// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using Longbow.TcpSocket;
using System.Runtime.Versioning;

namespace Longbow.Modbus;

/// <summary>
/// <see cref="IModbusTcpClient"/> 扩展方法类
/// </summary>
[UnsupportedOSPlatform("browser")]
public static class IModbusClientExtensions
{
    /// <summary>
    /// Establishes an asynchronous connection to the specified host and port.
    /// </summary>
    /// <param name="client">The TCP socket client to which the content will be sent. Cannot be <see langword="null"/>.</param>
    /// <param name="ipString">The hostname or IP address of the server to connect to. Cannot be null or empty.</param>
    /// <param name="port">The port number on the server to connect to. Must be a valid port number between 0 and 65535.</param>
    /// <param name="token">An optional <see cref="CancellationToken"/> to cancel the connection attempt. Defaults to <see
    /// langword="default"/> if not provided.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the connection
    /// is successfully established; otherwise, <see langword="false"/>.</returns>
    public static ValueTask<bool> ConnectAsync(this IModbusTcpClient client, string ipString, int port, CancellationToken token = default)
    {
        var endPoint = TcpSocketUtility.ConvertToIpEndPoint(ipString, port);
        return client.ConnectAsync(endPoint, token);
    }
}
