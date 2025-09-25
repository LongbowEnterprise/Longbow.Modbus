// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

namespace Longbow.Modbus;

/// <summary>
/// Modbus 客户端接口
/// </summary>
public interface IModbusClient : IAsyncDisposable
{
    /// <summary>
    /// 从指定站点异步读取线圈方法 功能码 0x01
    /// <para>Asynchronously reads from 1 to 2000 contiguous coils status.</para>
    /// </summary>
    /// <param name="slaveAddress">Address of device to read values from.</param>
    /// <param name="startAddress">Address to begin reading.</param>
    /// <param name="numberOfPoints">Number of coils to read.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous read operation.</returns>
    ValueTask<IModbusResponse> ReadCoilsAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints, CancellationToken token = default);

    /// <summary>
    /// 从指定站点异步读取离散输入方法 功能码 0x02
    /// <para>Asynchronously reads from 1 to 2000 contiguous discrete input status.</para>
    /// </summary>
    /// <param name="slaveAddress">Address of device to read values from.</param>
    /// <param name="startAddress">Address to begin reading.</param>
    /// <param name="numberOfPoints">Number of discrete inputs to read.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous read operation.</returns>
    ValueTask<IModbusResponse> ReadInputsAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints, CancellationToken token = default);

    /// <summary>
    /// 从指定站点异步读取保持寄存器方法 功能码 0x03
    /// <para>Asynchronously reads contiguous block of holding registers.</para>
    /// </summary>
    /// <param name="slaveAddress">Address of device to read values from.</param>
    /// <param name="startAddress">Address to begin reading.</param>
    /// <param name="numberOfPoints">Number of holding registers to read.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous read operation.</returns>
    ValueTask<IModbusResponse> ReadHoldingRegistersAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints, CancellationToken token = default);

    /// <summary>
    /// 从指定站点异步读取输入寄存器方法 功能码 0x04
    /// <para>Asynchronously reads contiguous block of input registers.</para>
    /// </summary>
    /// <param name="slaveAddress">Address of device to read values from.</param>
    /// <param name="startAddress">Address to begin reading.</param>
    /// <param name="numberOfPoints">Number of holding registers to read.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous read operation.</returns>
    ValueTask<IModbusResponse> ReadInputRegistersAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints, CancellationToken token = default);

    /// <summary>
    /// Asynchronously writes a single coil value.
    /// </summary>
    /// <param name="slaveAddress">Address of the device to write to.</param>
    /// <param name="coilAddress">Address to write value to.</param>
    /// <param name="value">Value to write.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    ValueTask<IModbusResponse> WriteCoilAsync(byte slaveAddress, ushort coilAddress, bool value, CancellationToken token = default);

    /// <summary>
    /// Asynchronously writes a sequence of coils.
    /// </summary>
    /// <param name="slaveAddress">Address of the device to write to.</param>
    /// <param name="startAddress">Address to begin writing values.</param>
    /// <param name="data">Values to write.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    ValueTask<IModbusResponse> WriteMultipleCoilsAsync(byte slaveAddress, ushort startAddress, bool[] data, CancellationToken token = default);

    /// <summary>
    /// Asynchronously writes a single holding register.
    /// </summary>
    /// <param name="slaveAddress">Address of the device to write to.</param>
    /// <param name="registerAddress">Address to write.</param>
    /// <param name="value">Value to write.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    ValueTask<IModbusResponse> WriteRegisterAsync(byte slaveAddress, ushort registerAddress, ushort value, CancellationToken token = default);

    /// <summary>
    /// Asynchronously writes a block of 1 to 123 contiguous registers.
    /// </summary>
    /// <param name="slaveAddress">Address of the device to write to.</param>
    /// <param name="startAddress">Address to begin writing values.</param>
    /// <param name="data">Values to write.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    ValueTask<IModbusResponse> WriteMultipleRegistersAsync(byte slaveAddress, ushort startAddress, ushort[] data, CancellationToken token = default);
}
