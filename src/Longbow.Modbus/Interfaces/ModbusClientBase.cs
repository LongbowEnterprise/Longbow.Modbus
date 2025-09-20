// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using System.Buffers;

namespace Longbow.Modbus;

abstract class ModbusClientBase(IModbusMessageBuilder builder) : IModbusClient
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public Exception? Exception { get; protected set; }

    protected abstract Task<ReadOnlyMemory<byte>> SendAsync(ReadOnlyMemory<byte> request, CancellationToken token = default);

    public async ValueTask<bool[]> ReadCoilsAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints, CancellationToken token = default)
    {
        MessageBuilder.ValidateNumberOfPoints(nameof(numberOfPoints), numberOfPoints, 2000);

        var response = await ReadAsync(slaveAddress, 0x01, startAddress, numberOfPoints, token);
        return builder.ReadBoolValues(response, numberOfPoints);
    }

    public async ValueTask<bool[]> ReadInputsAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints, CancellationToken token = default)
    {
        MessageBuilder.ValidateNumberOfPoints(nameof(numberOfPoints), numberOfPoints, 2000);

        var response = await ReadAsync(slaveAddress, 0x02, startAddress, numberOfPoints, token);
        return builder.ReadBoolValues(response, numberOfPoints);
    }

    public async ValueTask<ushort[]> ReadHoldingRegistersAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints, CancellationToken token = default)
    {
        MessageBuilder.ValidateNumberOfPoints(nameof(numberOfPoints), numberOfPoints, 125);

        var response = await ReadAsync(slaveAddress, 0x03, startAddress, numberOfPoints, token);
        return builder.ReadUShortValues(response, numberOfPoints);
    }

    public async ValueTask<ushort[]> ReadInputRegistersAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints, CancellationToken token = default)
    {
        MessageBuilder.ValidateNumberOfPoints(nameof(numberOfPoints), numberOfPoints, 125);

        var response = await ReadAsync(slaveAddress, 0x04, startAddress, numberOfPoints, token);
        return builder.ReadUShortValues(response, numberOfPoints);
    }

    public ValueTask<bool> WriteCoilAsync(byte slaveAddress, ushort coilAddress, bool value, CancellationToken token = default) => WriteBoolValuesAsync(slaveAddress, 0x05, coilAddress, [value], token);

    public ValueTask<bool> WriteRegisterAsync(byte slaveAddress, ushort registerAddress, ushort value, CancellationToken token = default) => WriteUShortValuesAsync(slaveAddress, 0x06, registerAddress, [value], token);

    public ValueTask<bool> WriteMultipleCoilsAsync(byte slaveAddress, ushort startAddress, bool[] values, CancellationToken token = default)
    {
        MessageBuilder.ValidateData(nameof(values), values, 1968);

        return WriteBoolValuesAsync(slaveAddress, 0x0F, startAddress, values, token);
    }

    public ValueTask<bool> WriteMultipleRegistersAsync(byte slaveAddress, ushort registerAddress, ushort[] values, CancellationToken token = default)
    {
        MessageBuilder.ValidateData(nameof(values), values, 123);

        return WriteUShortValuesAsync(slaveAddress, 0x10, registerAddress, values, token);
    }

    private async ValueTask<ReadOnlyMemory<byte>> ReadAsync(byte slaveAddress, byte functionCode, ushort startAddress, ushort numberOfPoints, CancellationToken token = default)
    {
        byte[]? buffer = null;

        try
        {
            await _semaphore.WaitAsync(token).ConfigureAwait(false);

            buffer = ArrayPool<byte>.Shared.Rent(12);

            // 构建请求报文
            var len = builder.BuildReadRequest(buffer, slaveAddress, functionCode, startAddress, numberOfPoints);

            // 发送请求
            var received = await SendAsync(buffer.AsMemory()[0..12], token);

            // 验证响应报文
            var valid = builder.TryValidateReadResponse(received, slaveAddress, functionCode, out var exception);

            Exception = valid ? null : exception;
            return valid ? received : default;
        }
        finally
        {
            if (buffer != null)
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            Release();
        }
    }

    private async ValueTask<bool> WriteBoolValuesAsync(byte slaveAddress, byte functionCode, ushort address, bool[] values, CancellationToken token = default)
    {
        byte[]? valueBuffer = null;
        byte[]? buffer = null;

        try
        {
            await _semaphore.WaitAsync(token).ConfigureAwait(false);

            // 构建数据值集合
            valueBuffer = ArrayPool<byte>.Shared.Rent(2000);
            var len = MessageBuilder.WriteBoolValues(valueBuffer, address, values);
            var data = valueBuffer.AsMemory()[0..len];

            // 构建请求报文
            buffer = ArrayPool<byte>.Shared.Rent(2000);
            var request = builder.BuildWriteRequest(buffer, slaveAddress, functionCode, data);

            // 发送请求
            var received = await SendAsync(buffer.AsMemory()[0..request], token);

            // 验证响应报文
            var valid = builder.TryValidateWriteResponse(received, slaveAddress, functionCode, data, out var exception);

            Exception = valid ? null : exception;
            return valid;
        }
        finally
        {
            if (valueBuffer != null)
            {
                ArrayPool<byte>.Shared.Return(valueBuffer);
            }
            if (buffer != null)
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            Release();
        }
    }

    private async ValueTask<bool> WriteUShortValuesAsync(byte slaveAddress, byte functionCode, ushort address, ushort[] values, CancellationToken token = default)
    {
        byte[]? valueBuffer = null;
        byte[]? buffer = null;

        try
        {
            await _semaphore.WaitAsync(token).ConfigureAwait(false);

            // 构建数据值集合
            valueBuffer = ArrayPool<byte>.Shared.Rent(2000);
            var len = MessageBuilder.WriteUShortValues(valueBuffer, address, values);
            var data = valueBuffer.AsMemory()[0..len];

            // 构建请求报文
            buffer = ArrayPool<byte>.Shared.Rent(2000);
            var request = builder.BuildWriteRequest(buffer, slaveAddress, functionCode, data);

            // 发送请求
            var received = await SendAsync(buffer.AsMemory()[0..request], token);

            // 验证响应报文
            var valid = builder.TryValidateWriteResponse(received, slaveAddress, functionCode, data, out var exception);

            Exception = valid ? null : exception;
            return valid;
        }
        finally
        {
            if (valueBuffer != null)
            {
                ArrayPool<byte>.Shared.Return(valueBuffer);
            }
            if (buffer != null)
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

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
