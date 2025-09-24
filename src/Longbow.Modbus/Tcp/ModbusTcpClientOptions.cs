// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using System.Net;

namespace Longbow.Modbus;

/// <summary>
/// ModbusTcpClientOptions 配置类
/// </summary>
public class ModbusTcpClientOptions
{
    /// <summary>
    /// Gets or sets the local endpoint for the socket client. Default value is <see cref="IPAddress.Any"/>
    /// </summary>
    /// <remarks>This property specifies the local network endpoint that the socket client will bind to when establishing a connection.</remarks>
    public IPEndPoint LocalEndPoint { get; set; } = new(IPAddress.Any, 0);

    /// <summary>
    /// 获得/设置 接收缓冲区大小 默认 1024
    /// </summary>
    public int ReceiveBufferSize { get; set; } = 1024;

    /// <summary>
    /// Gets or sets a value indicating whether the Nagle algorithm is disabled for the socket connection. Default value is false.
    /// </summary>
    public bool NoDelay { get; set; }
}
