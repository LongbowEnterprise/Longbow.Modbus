// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

namespace Longbow.Modbus;

/// <summary>
/// Modbus RTU 消息转换器将 <see cref="ReadOnlyMemory{T}"/> 转换成指定的数据类型
/// </summary>
public static class ModbusRtuMessageConverter
{
    /// <summary>
    /// 解析布尔值数组算法
    /// </summary>
    /// <param name="response"></param>
    /// <param name="numberOfPoints"></param>
    /// <returns></returns>
    public static bool[] ReadBoolValues(ReadOnlyMemory<byte> response, ushort numberOfPoints)
    {
        var values = new bool[numberOfPoints];
        if (!response.IsEmpty)
        {
            for (var i = 0; i < numberOfPoints; i++)
            {
                var byteIndex = 3 + i / 8;
                var bitIndex = i % 8;
                values[i] = (response.Span[byteIndex] & (1 << bitIndex)) != 0;
            }
        }

        return values;
    }

    /// <summary>
    /// 解析无符号短整数数组算法
    /// </summary>
    /// <param name="response"></param>
    /// <param name="numberOfPoints"></param>
    /// <returns></returns>
    public static ushort[] ReadUShortValues(ReadOnlyMemory<byte> response, ushort numberOfPoints)
    {
        var values = new ushort[numberOfPoints];
        if (!response.IsEmpty)
        {
            for (var i = 0; i < numberOfPoints; i++)
            {
                int offset = 3 + (i * 2);
                values[i] = (ushort)((response.Span[offset] << 8) | response.Span[offset + 1]);
            }
        }

        return values;
    }
}
