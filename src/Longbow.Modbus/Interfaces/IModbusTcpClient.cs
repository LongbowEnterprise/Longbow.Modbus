// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using System.Net;

namespace Longbow.Modbus;

/// <summary>
/// Modbus 客户端接口
/// </summary>
public interface IModbusTcpClient : IAsyncDisposable
{
    /// <summary>
    /// 异步连接方法
    /// </summary>
    /// <param name="endPoint"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    ValueTask<bool> ConnectAsync(IPEndPoint endPoint, CancellationToken token = default);

    /// <summary>
    /// 断开连接方法
    /// </summary>
    ValueTask CloseAsync();

    /// <summary>
    /// 获得 上一次操作异常信息。操作正常时为 null
    /// </summary>
    Exception? Exception { get; }

    /// <summary>
    /// 从指定站点异步读取线圈方法 功能码 0x01
    /// <para>Asynchronously reads from 1 to 2000 contiguous coils status.</para>
    /// </summary>
    /// <param name="slaveAddress">Address of device to read values from.</param>
    /// <param name="startAddress">Address to begin reading.</param>
    /// <param name="numberOfPoints">Number of coils to read.</param>
    /// <returns>A task that represents the asynchronous read operation.</returns>
    ValueTask<bool[]?> ReadCoilsAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints);

    /// <summary>
    /// 从指定站点异步读取离散输入方法 功能码 0x02
    /// <para>Asynchronously reads from 1 to 2000 contiguous discrete input status.</para>
    /// </summary>
    /// <param name="slaveAddress">Address of device to read values from.</param>
    /// <param name="startAddress">Address to begin reading.</param>
    /// <param name="numberOfPoints">Number of discrete inputs to read.</param>
    /// <returns>A task that represents the asynchronous read operation.</returns>
    ValueTask<bool[]?> ReadInputsAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints);

    /// <summary>
    /// 从指定站点异步读取保持寄存器方法 功能码 0x03
    /// <para>Asynchronously reads contiguous block of holding registers.</para>
    /// </summary>
    /// <param name="slaveAddress">Address of device to read values from.</param>
    /// <param name="startAddress">Address to begin reading.</param>
    /// <param name="numberOfPoints">Number of holding registers to read.</param>
    /// <returns>A task that represents the asynchronous read operation.</returns>
    ValueTask<ushort[]?> ReadHoldingRegistersAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints);

    /// <summary>
    /// 从指定站点异步读取输入寄存器方法 功能码 0x04
    /// <para>Asynchronously reads contiguous block of input registers.</para>
    /// </summary>
    /// <param name="slaveAddress">Address of device to read values from.</param>
    /// <param name="startAddress">Address to begin reading.</param>
    /// <param name="numberOfPoints">Number of holding registers to read.</param>
    /// <returns>A task that represents the asynchronous read operation.</returns>
    ValueTask<ushort[]?> ReadInputRegistersAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints);

    /// <summary>
    /// Asynchronously writes a single coil value.
    /// </summary>
    /// <param name="slaveAddress">Address of the device to write to.</param>
    /// <param name="coilAddress">Address to write value to.</param>
    /// <param name="value">Value to write.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    ValueTask<bool> WriteCoilAsync(byte slaveAddress, ushort coilAddress, bool value);

    /// <summary>
    /// Asynchronously writes a sequence of coils.
    /// </summary>
    /// <param name="slaveAddress">Address of the device to write to.</param>
    /// <param name="startAddress">Address to begin writing values.</param>
    /// <param name="data">Values to write.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    ValueTask<bool> WriteMultipleCoilsAsync(byte slaveAddress, ushort startAddress, bool[] data);

    /// <summary>
    /// Asynchronously writes a single holding register.
    /// </summary>
    /// <param name="slaveAddress">Address of the device to write to.</param>
    /// <param name="registerAddress">Address to write.</param>
    /// <param name="value">Value to write.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    ValueTask<bool> WriteRegisterAsync(byte slaveAddress, ushort registerAddress, ushort value);

    /// <summary>
    /// Asynchronously writes a block of 1 to 123 contiguous registers.
    /// </summary>
    /// <param name="slaveAddress">Address of the device to write to.</param>
    /// <param name="startAddress">Address to begin writing values.</param>
    /// <param name="data">Values to write.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    ValueTask<bool> WriteMultipleRegistersAsync(byte slaveAddress, ushort startAddress, ushort[] data);
}
