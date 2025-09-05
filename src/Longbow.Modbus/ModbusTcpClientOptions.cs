// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using System.Net;

namespace Longbow.Modbus;

/// <summary>
/// ModbusTcpClientOptions 配置类
/// </summary>
public class ModbusTcpClientOptions : ModbusClientOptions
{
    /// <summary>
    /// Gets or sets the timeout duration, in milliseconds, for establishing a connection. Default value is 0
    /// </summary>
    public int ConnectTimeout { get; set; }

    /// <summary>
    /// Gets or sets the local endpoint for the socket client. Default value is <see cref="IPAddress.Any"/>
    /// </summary>
    /// <remarks>This property specifies the local network endpoint that the socket client will bind to when establishing a connection.</remarks>
    public IPEndPoint LocalEndPoint { get; set; } = new(IPAddress.Any, 0);

    /// <summary>
    /// 获得/设置 Modbus 服务器地址
    /// </summary>
    public IPEndPoint? RemoteEndPoint { get; set; }
}
