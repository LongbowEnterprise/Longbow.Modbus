// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

namespace Longbow.Modbus;

abstract class ModbusClientBase(IModbusMessageBuilder builder) : IModbusClient
{
    public Exception? Exception { get; protected set; }

    protected abstract Task<ReadOnlyMemory<byte>> SendAsync(ReadOnlyMemory<byte> request);

    public async ValueTask<bool[]> ReadCoilsAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints)
    {
        var response = await ReadAsync(slaveAddress, 0x01, startAddress, numberOfPoints);
        return ReadBoolValues(response, numberOfPoints);
    }

    public async ValueTask<bool[]> ReadInputsAsync(byte slaveAddress, ushort startAddress, ushort numberOfInputs)
    {
        var response = await ReadAsync(slaveAddress, 0x02, startAddress, numberOfInputs);
        return ReadBoolValues(response, numberOfInputs);
    }

    public async ValueTask<ushort[]> ReadHoldingRegistersAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints)
    {
        var response = await ReadAsync(slaveAddress, 0x03, startAddress, numberOfPoints);
        return ReadUShortValues(response, numberOfPoints);
    }

    public async ValueTask<ushort[]> ReadInputRegistersAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints)
    {
        var response = await ReadAsync(slaveAddress, 0x04, startAddress, numberOfPoints);
        return ReadUShortValues(response, numberOfPoints);
    }

    public ValueTask<bool> WriteCoilAsync(byte slaveAddress, ushort coilAddress, bool value) => WriteBoolValuesAsync(slaveAddress, 0x05, coilAddress, [value]);

    public ValueTask<bool> WriteRegisterAsync(byte slaveAddress, ushort registerAddress, ushort value) => WriteUShortValuesAsync(slaveAddress, 0x06, registerAddress, [value]);

    public ValueTask<bool> WriteMultipleCoilsAsync(byte slaveAddress, ushort startAddress, bool[] values) => WriteBoolValuesAsync(slaveAddress, 0x0F, startAddress, values);

    public ValueTask<bool> WriteMultipleRegistersAsync(byte slaveAddress, ushort registerAddress, ushort[] values) => WriteUShortValuesAsync(slaveAddress, 0x10, registerAddress, values);

    private async ValueTask<ReadOnlyMemory<byte>> ReadAsync(byte slaveAddress, byte functionCode, ushort startAddress, ushort numberOfPoints)
    {
        // 构建请求报文
        var request = builder.BuildReadRequest(slaveAddress, functionCode, startAddress, numberOfPoints);

        // 发送请求
        var received = await SendAsync(request);

        // 验证响应报文
        var valid = builder.TryValidateReadResponse(received, slaveAddress, functionCode, out var exception);

        Exception = valid ? null : exception;
        return valid ? received : default;
    }

    private async ValueTask<bool> WriteBoolValuesAsync(byte slaveAddress, byte functionCode, ushort address, bool[] values)
    {
        // 构建请求报文
        var data = builder.WriteBoolValues(address, values);
        var request = builder.BuildWriteRequest(slaveAddress, functionCode, data);

        // 发送请求
        var received = await SendAsync(request);

        // 验证响应报文
        var valid = builder.TryValidateWriteResponse(received, slaveAddress, functionCode, data, out var exception);

        Exception = valid ? null : exception;
        return valid;
    }

    private async ValueTask<bool> WriteUShortValuesAsync(byte slaveAddress, byte functionCode, ushort address, ushort[] values)
    {
        // 构建请求报文
        var data = builder.WriteUShortValues(address, values);
        var request = builder.BuildWriteRequest(slaveAddress, functionCode, data);

        // 发送请求
        var received = await SendAsync(request);

        // 验证响应报文
        var valid = builder.TryValidateWriteResponse(received, slaveAddress, functionCode, data, out var exception);

        Exception = valid ? null : exception;
        return valid;
    }

    private bool[] ReadBoolValues(ReadOnlyMemory<byte> response, ushort numberOfPoints) => builder.ReadBoolValues(response, numberOfPoints);

    private ushort[] ReadUShortValues(ReadOnlyMemory<byte> response, ushort numberOfPoints) => builder.ReadUShortValues(response, numberOfPoints);

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public abstract ValueTask CloseAsync();

    /// <summary>
    /// 资源销毁方法
    /// </summary>
    /// <param name="disposing"></param>
    /// <returns></returns>
    protected async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            await CloseAsync();
        }
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(true);
        GC.SuppressFinalize(this);
    }
}
