// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

namespace Longbow.Modbus;

/// <summary>
/// Modbus 客户端配置选项类
/// </summary>
public class ModbusClientOptions
{
    /// <summary>
    /// 获得/设置 读取超时时间 默认 1000ms
    /// </summary>
    public int ReadTimeout { get; set; } = 1000;

    /// <summary>
    /// 获得/设置 写入超时时间 默认 1000ms
    /// </summary>
    public int WriteTimeout { get; set; } = 1000;
}
