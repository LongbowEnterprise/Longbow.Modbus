// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using Longbow.Socket.Algorithm;

namespace Longbow.Modbus;

/// <summary>
/// Modbus RTU 消息构建器
/// </summary>
class ModbusRtuMessageBuilder : IModbusRtuMessageBuilder
{
    /// <summary>
    /// 构建 Modbus RTU 读取消息方法
    /// </summary>
    /// <param name="slaveAddress"></param>
    /// <param name="functionCode"></param>
    /// <param name="startAddress"></param>
    /// <param name="numberOfPoints"></param>
    /// <returns></returns>
    public ReadOnlyMemory<byte> BuildReadRequest(byte slaveAddress, byte functionCode, ushort startAddress, ushort numberOfPoints)
    {
        byte[] request =
        [
            slaveAddress,                  // 00 从站地址
            functionCode,                  // 01 功能码
            (byte)(startAddress >> 8),     // 02 起始地址高字节
            (byte)(startAddress & 0xFF),   // 03 起始地址低字节
            (byte)(numberOfPoints >> 8),   // 04 寄存器数量高字节
            (byte)(numberOfPoints & 0xFF), // 05 寄存器数量低字节
        ];

        return ModbusCrc16.Append(request).ToArray();
    }

    /// <summary>
    /// 构建 Modbus RTU 写入消息方法
    /// </summary>
    /// <param name="slaveAddress"></param>
    /// <param name="functionCode"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public ReadOnlyMemory<byte> BuildWriteRequest(byte slaveAddress, byte functionCode, ReadOnlyMemory<byte> data)
    {
        var request = new byte[2 + data.Length];

        request[0] = slaveAddress;                  // 00 从站地址
        request[1] = functionCode;                  // 01 功能码

        // 写入数据部分
        data.CopyTo(request.AsMemory(2));

        return ModbusCrc16.Append(request).ToArray();
    }

    /// <summary>
    /// 验证 Modbus RTU 读取响应消息方法
    /// </summary>
    /// <param name="response"></param>
    /// <param name="slaveAddress"></param>
    /// <param name="functionCode"></param>
    /// <param name="exception"></param>
    /// <returns></returns>
    public bool TryValidateReadResponse(ReadOnlyMemory<byte> response, byte slaveAddress, byte functionCode, [NotNullWhen(false)] out Exception? exception)
    {
        if (!TryValidateHeader(response, slaveAddress, functionCode, out exception))
        {
            return false;
        }

        // 获取数据字节数
        var byteCount = response.Span[2];
        if (byteCount + 5 != response.Length)
        {
            exception = new Exception($"Response length does not match byte count 响应长度与字节计数不匹配 期望值 {byteCount + 5} 实际值 {response.Length}");
            return false;
        }

        // 验证 CRC 校验码
        if (!ModbusCrc16.Validate(response.Span))
        {
            exception = new Exception("CRC check failed CRC 校验失败");
            return false;
        }

        exception = null;
        return true;
    }

    /// <summary>
    /// 验证 Modbus RTU 写入响应消息方法
    /// </summary>
    /// <param name="response"></param>
    /// <param name="slaveAddress"></param>
    /// <param name="functionCode"></param>
    /// <param name="data"></param>
    /// <param name="exception"></param>
    /// <returns></returns>
    public bool TryValidateWriteResponse(ReadOnlyMemory<byte> response, byte slaveAddress, byte functionCode, ReadOnlyMemory<byte> data, [NotNullWhen(false)] out Exception? exception)
    {
        if (!TryValidateHeader(response, slaveAddress, functionCode, out exception))
        {
            return false;
        }

        if (response.Length == 8 && response.Span[1] == functionCode)
        {
            var expected = data[2..6];
            var actual = response[2..6];

            if (!expected.Span.SequenceEqual(actual.Span))
            {
                exception = new Exception($"return data does not match 返回值不匹配预期值 期望值: {BitConverter.ToString(expected.ToArray())} 实际值: {BitConverter.ToString(actual.ToArray())}");
                return false;
            }
        }

        // 验证 CRC 校验码
        if (!ModbusCrc16.Validate(response.Span))
        {
            exception = new Exception("CRC check failed CRC 校验失败");
            return false;
        }

        exception = null;
        return true;
    }

    private static bool TryValidateHeader(ReadOnlyMemory<byte> response, byte slaveAddress, byte functionCode, [NotNullWhen(false)] out Exception? exception)
    {
        // 检查响应长度
        if (response.Length < 5)
        {
            exception = new Exception("Response length is insufficient 响应长度不足");
            return false;
        }

        // 检查从站地址
        if (response.Span[0] != slaveAddress)
        {
            exception = new Exception($"Slave address is insufficient 从站地址不匹配 期望值 0x{slaveAddress:X2} 实际值 0x{response.Span[0]:X2}");
            return false;
        }

        // 检查功能码 (正常响应应与请求相同，异常响应 = 请求功能码 + 0x80)
        if (response.Span[1] == 0x80 + functionCode)
        {
            exception = new Exception($"Modbus abnormal response, error code: {response.Span[2]}. 异常响应，错误码: {response.Span[2]} {GetErrorMessage(response.Span[2])}");
            return false;
        }
        else if (response.Span[1] != functionCode)
        {
            exception = new Exception($"Function code does not match 功能码不匹配期望值 0x{functionCode:X2} 实际值 0x{response.Span[1]:X2}");
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
