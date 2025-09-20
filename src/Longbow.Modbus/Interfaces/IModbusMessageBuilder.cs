// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

namespace Longbow.Modbus;

/// <summary>
/// Modbus 消息构建器接口
/// </summary>
public interface IModbusMessageBuilder
{
    /// <summary>
    /// 构建 Modbus 读取消息方法
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="slaveAddress"></param>
    /// <param name="functionCode"></param>
    /// <param name="startAddress"></param>
    /// <param name="numberOfPoints"></param>
    /// <returns></returns>
    int BuildReadRequest(Memory<byte> buffer, byte slaveAddress, byte functionCode, ushort startAddress, ushort numberOfPoints);

    /// <summary>
    /// 构建 Modbus 写入消息方法
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="slaveAddress"></param>
    /// <param name="functionCode"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    int BuildWriteRequest(Memory<byte> buffer, byte slaveAddress, byte functionCode, ReadOnlyMemory<byte> data);

    /// <summary>
    /// 验证 Modbus 读取响应消息方法
    /// </summary>
    /// <param name="response"></param>
    /// <param name="slaveAddress"></param>
    /// <param name="functionCode"></param>
    /// <param name="exception"></param>
    /// <returns></returns>
    bool TryValidateReadResponse(ReadOnlyMemory<byte> response, byte slaveAddress, byte functionCode, [NotNullWhen(false)] out Exception? exception);

    /// <summary>
    /// 验证 Modbus 写入响应消息方法
    /// </summary>
    /// <param name="response"></param>
    /// <param name="slaveAddress"></param>
    /// <param name="functionCode"></param>
    /// <param name="data"></param>
    /// <param name="exception"></param>
    /// <returns></returns>
    bool TryValidateWriteResponse(ReadOnlyMemory<byte> response, byte slaveAddress, byte functionCode, ReadOnlyMemory<byte> data, [NotNullWhen(false)] out Exception? exception);
}
