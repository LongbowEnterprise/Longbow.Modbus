// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

namespace Longbow.Modbus;

/// <summary>
/// Modbus 消息转换器将 <see cref="ReadOnlyMemory{T}"/> 转换成指定的数据类型
/// </summary>
static class ModbusMessageConverter
{
    public static bool[] ReadBoolValues(this IModbusClient client, ReadOnlyMemory<byte> response, ushort numberOfPoints)
    {
        return client is IModbusTcpClient
            ? ModbusTcpMessageConverter.ReadBoolValues(response, numberOfPoints)
            : ModbusRtuMessageConverter.ReadBoolValues(response, numberOfPoints);
    }

    public static ushort[] ReadUShortValues(this IModbusClient client, ReadOnlyMemory<byte> response, ushort numberOfPoints)
    {
        return client is IModbusTcpClient
            ? ModbusTcpMessageConverter.ReadUShortValues(response, numberOfPoints)
            : ModbusRtuMessageConverter.ReadUShortValues(response, numberOfPoints);
    }
}
