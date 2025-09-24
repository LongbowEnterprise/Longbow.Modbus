// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

namespace Longbow.Modbus;

/// <summary>
/// Modbus 返回数据接口
/// </summary>
public interface IModbusResponse
{
    /// <summary>
    /// 获得 数据缓冲区
    /// </summary>
    ReadOnlyMemory<byte> Buffer { get; }

    /// <summary>
    /// 获得 <see cref="IModbusMessageBuilder"/> 实例
    /// </summary>
    IModbusMessageBuilder Builder { get; }

    /// <summary>
    /// 获得 上一次操作异常信息。操作正常时为 null
    /// </summary>
    Exception? Exception { get; }

    /// <summary>
    /// 指示响应是否成功
    /// </summary>
    bool IsSuccess { get; }
}
