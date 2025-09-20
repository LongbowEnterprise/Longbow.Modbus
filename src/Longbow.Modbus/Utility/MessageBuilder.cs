// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

namespace Longbow.Modbus;

static class MessageBuilder
{
    public static void ValidateNumberOfPoints(string argumentName, ushort numberOfPoints, ushort maxNumberOfPoints)
    {
        if (numberOfPoints < 1 || numberOfPoints > maxNumberOfPoints)
        {
            string msg = $"Argument {argumentName} must be between 1 and {maxNumberOfPoints} inclusive.";
            throw new ArgumentException(msg, argumentName);
        }
    }

    public static void ValidateData<T>(string argumentName, T[] data, int maxDataLength)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (data.Length == 0 || data.Length > maxDataLength)
        {
            string msg = $"The length of argument {argumentName} must be between 1 and {maxDataLength} inclusive.";
            throw new ArgumentException(msg, argumentName);
        }
    }


    public static int WriteBoolValues(Memory<byte> buffer, ushort address, bool[] values)
    {
        int byteCount = (values.Length + 7) / 8;
        var len = values.Length > 1 ? 5 + byteCount : 4;
        var span = buffer.Span;

        span[0] = (byte)(address >> 8);
        span[1] = (byte)address;

        if (values.Length > 1)
        {
            // 多值时，写入数量
            span[2] = (byte)(values.Length >> 8);
            span[3] = (byte)(values.Length);

            // 字节数
            span[4] = (byte)(byteCount);

            for (var i = 0; i < values.Length; i++)
            {
                if (values[i])
                {
                    int byteIndex = 5 + i / 8;
                    int bitIndex = i % 8;
                    span[byteIndex] |= (byte)(1 << bitIndex);
                }
            }
        }
        else
        {
            // 组装数据
            span[2] = values[0] ? (byte)0xFF : (byte)0x00;
            span[3] = 0x00;
        }

        return len;
    }

    public static int WriteUShortValues(Memory<byte> buffer, ushort address, ushort[] values)
    {
        int byteCount = values.Length * 2;
        var len = values.Length > 1 ? 5 + byteCount : 4;
        var span = buffer.Span;

        span[0] = (byte)(address >> 8);
        span[1] = (byte)address;

        if (values.Length > 1)
        {
            // 多值时，写入数量
            span[2] = (byte)(values.Length >> 8);
            span[3] = (byte)(values.Length);

            // 字节数
            span[4] = (byte)(byteCount);

            for (var i = 0; i < values.Length; i++)
            {
                span[i * 2 + 5] = (byte)(values[i] >> 8);
                span[i * 2 + 6] = (byte)(values[i] & 0xFF);
            }
        }
        else
        {
            span[2] = (byte)(values[0] >> 8);
            span[3] = (byte)(values[0] & 0xFF);
        }

        return len;
    }
}
