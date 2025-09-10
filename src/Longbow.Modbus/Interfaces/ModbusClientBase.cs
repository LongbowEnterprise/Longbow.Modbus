// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

namespace Longbow.Modbus;

abstract class ModbusClientBase(IModbusMessageBuilder builder) : IModbusClient
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public Exception? Exception { get; protected set; }

    protected abstract Task<ReadOnlyMemory<byte>> SendAsync(ReadOnlyMemory<byte> request, CancellationToken token = default);

    public async ValueTask<bool[]> ReadCoilsAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints, CancellationToken token = default)
    {
        var response = await ReadAsync(slaveAddress, 0x01, startAddress, numberOfPoints, token);
        return builder.ReadBoolValues(response, numberOfPoints);
    }

    public async ValueTask<bool[]> ReadInputsAsync(byte slaveAddress, ushort startAddress, ushort numberOfInputs, CancellationToken token = default)
    {
        var response = await ReadAsync(slaveAddress, 0x02, startAddress, numberOfInputs, token);
        return builder.ReadBoolValues(response, numberOfInputs);
    }

    public async ValueTask<ushort[]> ReadHoldingRegistersAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints, CancellationToken token = default)
    {
        var response = await ReadAsync(slaveAddress, 0x03, startAddress, numberOfPoints, token);
        return builder.ReadUShortValues(response, numberOfPoints);
    }

    public async ValueTask<ushort[]> ReadInputRegistersAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints, CancellationToken token = default)
    {
        var response = await ReadAsync(slaveAddress, 0x04, startAddress, numberOfPoints, token);
        return builder.ReadUShortValues(response, numberOfPoints);
    }

    public ValueTask<bool> WriteCoilAsync(byte slaveAddress, ushort coilAddress, bool value, CancellationToken token = default) => WriteBoolValuesAsync(slaveAddress, 0x05, coilAddress, [value], token);

    public ValueTask<bool> WriteRegisterAsync(byte slaveAddress, ushort registerAddress, ushort value, CancellationToken token = default) => WriteUShortValuesAsync(slaveAddress, 0x06, registerAddress, [value], token);

    public ValueTask<bool> WriteMultipleCoilsAsync(byte slaveAddress, ushort startAddress, bool[] values, CancellationToken token = default) => WriteBoolValuesAsync(slaveAddress, 0x0F, startAddress, values, token);

    public ValueTask<bool> WriteMultipleRegistersAsync(byte slaveAddress, ushort registerAddress, ushort[] values, CancellationToken token = default) => WriteUShortValuesAsync(slaveAddress, 0x10, registerAddress, values, token);

    private async ValueTask<ReadOnlyMemory<byte>> ReadAsync(byte slaveAddress, byte functionCode, ushort startAddress, ushort numberOfPoints, CancellationToken token = default)
    {
        try
        {
            await _semaphore.WaitAsync(token).ConfigureAwait(false);

            // 构建请求报文
            var request = builder.BuildReadRequest(slaveAddress, functionCode, startAddress, numberOfPoints);

            // 发送请求
            var received = await SendAsync(request, token);

            // 验证响应报文
            var valid = builder.TryValidateReadResponse(received, slaveAddress, functionCode, out var exception);

            Exception = valid ? null : exception;
            return valid ? received : default;
        }
        finally
        {
            Release();
        }
    }

    private async ValueTask<bool> WriteBoolValuesAsync(byte slaveAddress, byte functionCode, ushort address, bool[] values, CancellationToken token = default)
    {
        try
        {
            await _semaphore.WaitAsync(token).ConfigureAwait(false);

            // 构建请求报文
            var data = builder.WriteBoolValues(address, values);
            var request = builder.BuildWriteRequest(slaveAddress, functionCode, data);

            // 发送请求
            var received = await SendAsync(request, token);

            // 验证响应报文
            var valid = builder.TryValidateWriteResponse(received, slaveAddress, functionCode, data, out var exception);

            Exception = valid ? null : exception;
            return valid;
        }
        finally
        {
            Release();
        }
    }

    private async ValueTask<bool> WriteUShortValuesAsync(byte slaveAddress, byte functionCode, ushort address, ushort[] values, CancellationToken token = default)
    {
        try
        {
            await _semaphore.WaitAsync(token).ConfigureAwait(false);

            // 构建请求报文
            var data = builder.WriteUShortValues(address, values);
            var request = builder.BuildWriteRequest(slaveAddress, functionCode, data);

            // 发送请求
            var received = await SendAsync(request, token);

            // 验证响应报文
            var valid = builder.TryValidateWriteResponse(received, slaveAddress, functionCode, data, out var exception);

            Exception = valid ? null : exception;
            return valid;
        }
        finally
        {
            Release();
        }
    }

    private void Release()
    {
        if (_semaphore.CurrentCount == 0)
        {
            _semaphore.Release();
        }
    }

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
