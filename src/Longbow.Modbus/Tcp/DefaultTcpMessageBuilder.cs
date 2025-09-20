// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

namespace Longbow.Modbus;

/// <summary>
/// Modbus TCP 消息构建器
/// </summary>
sealed class DefaultTcpMessageBuilder : IModbusTcpMessageBuilder
{
    // 事务标识符计数器
    private uint _transactionId = 0;

    public int BuildReadRequest(Memory<byte> buffer, byte slaveAddress, byte functionCode, ushort startAddress, ushort numberOfPoints)
    {
        var transactionId = GetTransactionId();

        var request = buffer.Span;

        // MBAP头（7字节）
        request[0] = (byte)(transactionId >> 8);    // 00 事务标识符高字节（可随机）
        request[1] = (byte)(transactionId & 0xFF);  // 01 事务标识符低字节
        request[2] = 0x00;                          // 02 协议标识符高字节（Modbus固定0）
        request[3] = 0x00;                          // 03 协议标识符低字节
        request[4] = 0x00;                          // 04 长度高字节（后续字节数）
        request[5] = 0x06;                          // 05 长度低字节（6字节PDU）

        // PDU部分
        request[6] = slaveAddress;                  // 06 从站地址
        request[7] = functionCode;                  // 07 功能码
        request[8] = (byte)(startAddress >> 8);     // 08 起始地址高字节
        request[9] = (byte)(startAddress & 0xFF);   // 09 起始地址低字节
        request[10] = (byte)(numberOfPoints >> 8);  // 10 寄存器数量高字节
        request[11] = (byte)(numberOfPoints & 0xFF);// 11 寄存器数量低字节

        return 12;
    }

    public int BuildWriteRequest(Memory<byte> buffer, byte slaveAddress, byte functionCode, ReadOnlyMemory<byte> data)
    {
        var transactionId = GetTransactionId();

        var request = buffer.Span;

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
        data.CopyTo(buffer[8..]);

        return 8 + data.Length;
    }

    private uint GetTransactionId()
    {
        if (_transactionId >= 0xFFFF)
        {
            Interlocked.Exchange(ref _transactionId, 0);
            return 0;
        }

        return Interlocked.Increment(ref _transactionId);
    }

    public bool TryValidateReadResponse(ReadOnlyMemory<byte> response, byte slaveAddress, byte functionCode, [NotNullWhen(false)] out Exception? exception)
    {
        if (!TryValidateHeader(response, slaveAddress, functionCode, out exception))
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

    public bool TryValidateWriteResponse(ReadOnlyMemory<byte> response, byte slaveAddress, byte functionCode, ReadOnlyMemory<byte> data, [NotNullWhen(false)] out Exception? exception)
    {
        if (!TryValidateHeader(response, slaveAddress, functionCode, out exception))
        {
            return false;
        }

        if (response.Length == 12)
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

    private bool TryValidateHeader(ReadOnlyMemory<byte> response, byte slaveAddress, byte functionCode, [NotNullWhen(false)] out Exception? exception)
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

        // 检查从站地址
        if (response.Span[6] != slaveAddress)
        {
            exception = new Exception($"Slave address is insufficient 从站地址不匹配 期望值 0x{slaveAddress:X2} 实际值 0x{response.Span[6]:X2}");
            return false;
        }

        // 检查功能码 (正常响应应与请求相同，异常响应 = 请求功能码 + 0x80)
        if (response.Span[7] == 0x80 + functionCode)
        {
            exception = new Exception($"Modbus abnormal response, error code: {response.Span[8]}. 异常响应，错误码: {response.Span[8]} {GetErrorMessage(response.Span[8])}");
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

    private static string GetErrorMessage(byte errorCode)
    {
        return errorCode switch
        {
            0x01 => "非法功能码",
            0x02 => "非法数据地址",
            0x03 => "非法数据值",
            0x04 => "从站设备故障",
            _ => $"未知错误码: 0x{errorCode:X2}"
        };
    }
}
