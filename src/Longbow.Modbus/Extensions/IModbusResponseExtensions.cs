// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

namespace Longbow.Modbus;

/// <summary>
/// <see cref="IModbusResponse"/> 扩展方法
/// </summary>
public static class IModbusResponseExtensions
{
    /// <summary>
    /// 将 <see cref="IModbusResponse"/> 实例中 <see cref="IModbusResponse.Buffer"/> 转换成布尔数组
    /// </summary>
    /// <param name="response"></param>
    /// <param name="numberOfPoints"></param>
    /// <returns></returns>
    public static bool[] ReadBoolValues(this IModbusResponse response, ushort numberOfPoints)
    {
        return response.Builder is IModbusTcpMessageBuilder
            ? ModbusTcpMessageConverter.ReadBoolValues(response.Buffer, numberOfPoints)
            : ModbusRtuMessageConverter.ReadBoolValues(response.Buffer, numberOfPoints);
    }

    /// <summary>
    /// 将 <see cref="IModbusResponse"/> 实例中 <see cref="IModbusResponse.Buffer"/> 转换成无符号短整型数组
    /// </summary>
    /// <param name="response"></param>
    /// <param name="numberOfPoints"></param>
    /// <returns></returns>
    public static ushort[] ReadUShortValues(this IModbusResponse response, ushort numberOfPoints)
    {
        return response.Builder is IModbusTcpMessageBuilder
            ? ModbusTcpMessageConverter.ReadUShortValues(response.Buffer, numberOfPoints)
            : ModbusRtuMessageConverter.ReadUShortValues(response.Buffer, numberOfPoints);
    }
}
