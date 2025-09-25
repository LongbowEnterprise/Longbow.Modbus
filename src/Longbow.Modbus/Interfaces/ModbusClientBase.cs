// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using System.Buffers;

namespace Longbow.Modbus;

abstract class ModbusClientBase(IModbusMessageBuilder builder) : IModbusClient
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    protected abstract Task<ReadOnlyMemory<byte>> SendAsync(ReadOnlyMemory<byte> request, CancellationToken token = default);

    public ValueTask<IModbusResponse> ReadCoilsAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints, CancellationToken token = default)
    {
        MessageBuilder.ValidateNumberOfPoints(nameof(numberOfPoints), numberOfPoints, 2000);

        return ReadAsync(slaveAddress, 0x01, startAddress, numberOfPoints, token);
    }

    public ValueTask<IModbusResponse> ReadInputsAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints, CancellationToken token = default)
    {
        MessageBuilder.ValidateNumberOfPoints(nameof(numberOfPoints), numberOfPoints, 2000);

        return ReadAsync(slaveAddress, 0x02, startAddress, numberOfPoints, token);
    }

    public ValueTask<IModbusResponse> ReadHoldingRegistersAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints, CancellationToken token = default)
    {
        MessageBuilder.ValidateNumberOfPoints(nameof(numberOfPoints), numberOfPoints, 125);

        return ReadAsync(slaveAddress, 0x03, startAddress, numberOfPoints, token);
    }

    public ValueTask<IModbusResponse> ReadInputRegistersAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints, CancellationToken token = default)
    {
        MessageBuilder.ValidateNumberOfPoints(nameof(numberOfPoints), numberOfPoints, 125);

        return ReadAsync(slaveAddress, 0x04, startAddress, numberOfPoints, token);
    }

    public ValueTask<IModbusResponse> WriteCoilAsync(byte slaveAddress, ushort coilAddress, bool value, CancellationToken token = default) => WriteBoolValuesAsync(slaveAddress, 0x05, coilAddress, [value], token);

    public ValueTask<IModbusResponse> WriteRegisterAsync(byte slaveAddress, ushort registerAddress, ushort value, CancellationToken token = default) => WriteUShortValuesAsync(slaveAddress, 0x06, registerAddress, [value], token);

    public ValueTask<IModbusResponse> WriteMultipleCoilsAsync(byte slaveAddress, ushort startAddress, bool[] values, CancellationToken token = default)
    {
        MessageBuilder.ValidateData(nameof(values), values, 1968);

        return WriteBoolValuesAsync(slaveAddress, 0x0F, startAddress, values, token);
    }

    public ValueTask<IModbusResponse> WriteMultipleRegistersAsync(byte slaveAddress, ushort registerAddress, ushort[] values, CancellationToken token = default)
    {
        MessageBuilder.ValidateData(nameof(values), values, 123);

        return WriteUShortValuesAsync(slaveAddress, 0x10, registerAddress, values, token);
    }

    private async ValueTask<IModbusResponse> ReadAsync(byte slaveAddress, byte functionCode, ushort startAddress, ushort numberOfPoints, CancellationToken token = default)
    {
        try
        {
            await _semaphore.WaitAsync(token).ConfigureAwait(false);

            return await SendReadRequestAsync(slaveAddress, functionCode, startAddress, numberOfPoints, token);
        }
        finally
        {
            Release();
        }
    }

    private async ValueTask<IModbusResponse> SendReadRequestAsync(byte slaveAddress, byte functionCode, ushort startAddress, ushort numberOfPoints, CancellationToken token = default)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(12);

        try
        {
            // 构建请求报文
            var len = builder.BuildReadRequest(buffer, slaveAddress, functionCode, startAddress, numberOfPoints);

            // 发送请求
            var received = await SendAsync(buffer.AsMemory()[..len], token);

            // 验证响应报文
            builder.TryValidateReadResponse(received, slaveAddress, functionCode, out var exception);

            return new DefaultModbusResponse(received, builder, exception);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private async ValueTask<IModbusResponse> WriteBoolValuesAsync(byte slaveAddress, byte functionCode, ushort address, bool[] values, CancellationToken token = default)
    {
        try
        {
            await _semaphore.WaitAsync(token).ConfigureAwait(false);

            return await SendWriteValuesRequestAsync(slaveAddress, functionCode, address, values, MessageBuilder.WriteBoolValues, token);
        }
        finally
        {
            Release();
        }
    }

    private async ValueTask<IModbusResponse> WriteUShortValuesAsync(byte slaveAddress, byte functionCode, ushort address, ushort[] values, CancellationToken token = default)
    {
        try
        {
            await _semaphore.WaitAsync(token).ConfigureAwait(false);

            return await SendWriteValuesRequestAsync(slaveAddress, functionCode, address, values, MessageBuilder.WriteUShortValues, token);
        }
        finally
        {
            Release();
        }
    }

    private async ValueTask<IModbusResponse> SendWriteValuesRequestAsync<TValue>(byte slaveAddress, byte functionCode, ushort address, TValue[] values, Func<Memory<byte>, ushort, TValue[], int> writeCallback, CancellationToken token = default)
    {
        // 构建数据值集合
        var buffer = ArrayPool<byte>.Shared.Rent(2000);

        try
        {
            var len = writeCallback(buffer, address, values);
            var data = buffer.AsMemory()[..len];
            var received = await SendWriteValuesRequestCoreAsync(slaveAddress, functionCode, data, token);

            // 验证响应报文
            var valid = builder.TryValidateWriteResponse(received, slaveAddress, functionCode, data, out var exception);

            return new DefaultModbusResponse(received, builder, exception);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private async ValueTask<ReadOnlyMemory<byte>> SendWriteValuesRequestCoreAsync(byte slaveAddress, byte functionCode, ReadOnlyMemory<byte> data, CancellationToken token = default)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(2000);

        try
        {
            // 构建请求报文
            var len = builder.BuildWriteRequest(buffer, slaveAddress, functionCode, data);

            // 发送请求
            return await SendAsync(buffer.AsMemory()[..len], token);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
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
