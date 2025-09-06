// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using System;

namespace Longbow.Modbus;

/// <summary>
/// Modbus TCP 消息构建器
/// </summary>
public class ModbusTcpMessageBuilder
{
    // 事务标识符计数器
    private uint _transactionId = 0;

    /// <summary>
    /// 构建 Modbus TCP 读取消息方法
    /// </summary>
    /// <param name="slaveAddress"></param>
    /// <param name="functionCode"></param>
    /// <param name="startAddress"></param>
    /// <param name="numberOfPoints"></param>
    /// <returns></returns>
    public ReadOnlyMemory<byte> BuildReadRequest(byte slaveAddress, byte functionCode, ushort startAddress, ushort numberOfPoints)
    {
        var transactionId = GetTransactionId();
        byte[] request =
        [
            // MBAP头（7字节）
            (byte)(transactionId >> 8),   // 00 事务标识符高字节（可随机）
            (byte)(transactionId & 0xFF), // 01 事务标识符低字节
            0x00,                          // 02 协议标识符高字节（Modbus固定0）
            0x00,                          // 03 协议标识符低字节
            0x00,                          // 04 长度高字节（后续字节数）
            0x06,                          // 05 长度低字节（6字节PDU）

            // PDU部分
            slaveAddress,                  // 06 从站地址
            functionCode,                  // 07 功能码
            (byte)(startAddress >> 8),     // 08 起始地址高字节
            (byte)(startAddress & 0xFF),   // 09 起始地址低字节
            (byte)(numberOfPoints >> 8),   // 10 寄存器数量高字节
            (byte)(numberOfPoints & 0xFF), // 11 寄存器数量低字节
        ];

        return request;
    }

    /// <summary>
    /// 构建 Modbus TCP 写入消息方法
    /// </summary>
    /// <param name="slaveAddress"></param>
    /// <param name="functionCode"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public ReadOnlyMemory<byte> BuildWriteRequest(byte slaveAddress, byte functionCode, ReadOnlyMemory<byte> data)
    {
        var transactionId = GetTransactionId();
        var request = new byte[8 + data.Length];

        // MBAP头（7字节）
        request[0] = (byte)(transactionId >> 8);    // 00 事务标识符高字节（可随机）
        request[1] = (byte)(transactionId & 0xFF);  // 01 事务标识符低字节
        request[2] = 0x00;                          // 02 协议标识符高字节（Modbus固定0）
        request[3] = 0x00;                          // 03 协议标识符低字节
        request[4] = 0x00;                          // 04 长度高字节（后续字节数）
        request[5] = (byte)(2 + data.Length);       // 05 长度低字节（PDU数据）

        // PDU部分
        request[6] = slaveAddress;                  // 06 从站地址
        request[7] = functionCode;                  // 07 功能码

        // 写入数据部分
        data.CopyTo(request.AsMemory(8));

        return request;
    }

    private uint GetTransactionId() => _transactionId >= ushort.MaxValue
        ? Interlocked.Exchange(ref _transactionId, 0)
        : Interlocked.Increment(ref _transactionId);

    /// <summary>
    /// 验证 Modbus TCP 读取响应消息方法
    /// </summary>
    /// <param name="response"></param>
    /// <param name="functionCode"></param>
    /// <param name="exception"></param>
    /// <returns></returns>
    public bool TryValidateReadResponse(ReadOnlyMemory<byte> response, byte functionCode, [NotNullWhen(false)] out Exception? exception)
    {
        if (!TryValidateHeader(response, functionCode, out exception))
        {
            return false;
        }

        // 获取数据字节数
        var byteCount = response.Span[8];
        if (byteCount + 9 != response.Length)
        {
            exception = new Exception($"Response length does not match byte count 响应长度与字节计数不匹配 期望值 {byteCount + 9} 实际值 {response.Length}");
            return false;
        }

        exception = null;
        return true;
    }

    /// <summary>
    /// 验证 Modbus TCP 写入响应消息方法
    /// </summary>
    /// <param name="response"></param>
    /// <param name="functionCode"></param>
    /// <param name="data"></param>
    /// <param name="exception"></param>
    /// <returns></returns>
    public bool TryValidateWriteResponse(ReadOnlyMemory<byte> response, byte functionCode, ReadOnlyMemory<byte> data, [NotNullWhen(false)] out Exception? exception)
    {
        if (!TryValidateHeader(response, functionCode, out exception))
        {
            return false;
        }

        if (response.Length == 12 && response.Span[7] == functionCode)
        {
            var expected = data.Length == 4
                ? data
                : data[0..4];
            var actual = data.Length == 4
                ? response[8..]
                : response[8..12];

            if (!expected.Span.SequenceEqual(actual.Span))
            {
                exception = new Exception($"return data does not match 返回值不匹配预期值 期望值: {BitConverter.ToString(expected.ToArray())} 实际值: {BitConverter.ToString(actual.ToArray())}");
                return false;
            }
        }

        exception = null;
        return true;
    }

    private bool TryValidateHeader(ReadOnlyMemory<byte> response, byte functionCode, [NotNullWhen(false)] out Exception? exception)
    {
        // 检查响应长度
        if (response.Length < 9)
        {
            exception = new Exception("Response length is insufficient 响应长度不足");
            return false;
        }

        // 检查事务标识符是否匹配
        if (response.Span[0] != (_transactionId >> 8) || response.Span[1] != (_transactionId & 0xFF))
        {
            exception = new Exception("Transaction identifier mismatch 事务标识符不匹配");
            return false;
        }

        // 检查功能码 (正常响应应与请求相同，异常响应 = 请求功能码 + 0x80)
        if (response.Span[7] == 0x80 + functionCode)
        {
            exception = new Exception($"Modbus abnormal response, error code: {response.Span[8]}. 异常响应，错误码: {response.Span[8]}");
            return false;
        }
        else if (response.Span[7] != functionCode)
        {
            exception = new Exception($"Function code does not match 功能码不匹配期望值 0x{functionCode:X2} 实际值 0x{response.Span[7]:X2}");
            return false;
        }

        exception = null;
        return true;
    }
}
