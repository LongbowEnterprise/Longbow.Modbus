// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

namespace Longbow.Modbus;

/// <summary>
/// <see cref="IModbusTcpClient"/> 扩展方法类
/// </summary>
public static class IModbusClientExtensions
{
    /// <summary>
    /// Establishes an asynchronous connection to the specified host and port.
    /// </summary>
    /// <param name="client">The TCP socket client to which the content will be sent. Cannot be <see langword="null"/>.</param>
    /// <param name="ipString">The hostname or IP address of the server to connect to. Cannot be null or empty.</param>
    /// <param name="port">The port number on the server to connect to. Must be a valid port number between 0 and 65535.</param>
    /// <param name="token">An optional <see cref="CancellationToken"/> to cancel the connection attempt. Defaults to <see
    /// langword="default"/> if not provided.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the connection
    /// is successfully established; otherwise, <see langword="false"/>.</returns>
    public static ValueTask<bool> ConnectAsync(this IModbusTcpClient client, string ipString, int port, CancellationToken token = default)
    {
        var endPoint = TcpSocketUtility.ConvertToIpEndPoint(ipString, port);
        return client.ConnectAsync(endPoint, token);
    }

    /// <summary>
    /// 从指定站点异步读取线圈方法 功能码 0x01
    /// <para>Asynchronously reads from 1 to 2000 contiguous coils status.</para>
    /// </summary>
    public static async ValueTask<bool[]> ReadCoilsAsync(this IModbusClient client, byte slaveAddress, ushort startAddress, ushort numberOfPoints, CancellationToken token = default)
    {
        var response = await client.ReadCoilsAsync(slaveAddress, startAddress, numberOfPoints, token);
        return ModbusTcpMessageConverter.ReadBoolValues(response, numberOfPoints);
    }

    /// <summary>
    /// 从指定站点异步读取离散输入方法 功能码 0x02
    /// <para>Asynchronously reads from 1 to 2000 contiguous discrete input status.</para>
    /// </summary>
    public static async ValueTask<bool[]> ReadInputsAsync(this IModbusClient client, byte slaveAddress, ushort startAddress, ushort numberOfPoints, CancellationToken token = default)
    {
        var response = await client.ReadInputsAsync(slaveAddress, startAddress, numberOfPoints, token);
        return ModbusTcpMessageConverter.ReadBoolValues(response, numberOfPoints);
    }

    /// <summary>
    /// 从指定站点异步读取保持寄存器方法 功能码 0x03
    /// <para>Asynchronously reads contiguous block of holding registers.</para>
    /// </summary>
    public static async ValueTask<ushort[]> ReadHoldingRegistersAsync(this IModbusClient client, byte slaveAddress, ushort startAddress, ushort numberOfPoints, CancellationToken token = default)
    {
        var response = await client.ReadHoldingRegistersAsync(slaveAddress, startAddress, numberOfPoints, token);
        return ModbusTcpMessageConverter.ReadUShortValues(response, numberOfPoints);
    }

    /// <summary>
    /// 从指定站点异步读取输入寄存器方法 功能码 0x04
    /// <para>Asynchronously reads contiguous block of input registers.</para>
    /// </summary>
    public static async ValueTask<ushort[]> ReadInputRegistersAsync(this IModbusClient client, byte slaveAddress, ushort startAddress, ushort numberOfPoints, CancellationToken token = default)
    {
        var response = await client.ReadInputRegistersAsync(slaveAddress, startAddress, numberOfPoints, token);
        return ModbusTcpMessageConverter.ReadUShortValues(response, numberOfPoints);
    }
}
