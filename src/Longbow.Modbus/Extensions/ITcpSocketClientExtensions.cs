// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using System.Net.Sockets;

namespace Longbow.Modbus;

internal static class ITcpSocketClientExtensions
{
    public static void ThrowIfNotConnected(this ITcpSocketClient client)
    {
        if (!client.IsConnected)
        {
            throw new InvalidOperationException("站点未连接请先调用 ConnectAsync 方法连接设备");
        }
    }

    public static void ThrowIfNotConnected(this UdpClient client)
    {
        if (client == null)
        {
            throw new InvalidOperationException("站点未连接请先调用 ConnectAsync 方法连接设备");
        }
    }
}
