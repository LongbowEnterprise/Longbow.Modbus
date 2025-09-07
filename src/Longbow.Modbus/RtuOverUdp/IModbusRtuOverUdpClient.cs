﻿// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using System.Net;

namespace Longbow.Modbus;

/// <summary>
/// Modbus RTU Over UdpClient 客户端接口
/// </summary>
public interface IModbusRtuOverUdpClient : IModbusClient
{
    /// <summary>
    /// 异步连接方法
    /// </summary>
    /// <param name="endPoint"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    ValueTask<bool> ConnectAsync(IPEndPoint endPoint, CancellationToken token = default);

    /// <summary>
    /// 断开连接方法
    /// </summary>
    ValueTask CloseAsync();
}
