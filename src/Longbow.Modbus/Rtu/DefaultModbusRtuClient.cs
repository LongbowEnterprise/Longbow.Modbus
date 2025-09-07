// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using System.IO.Ports;

namespace Longbow.Modbus;

class DefaultModbusRtuClient(ModbusRtuClientOptions options, IModbusRtuMessageBuilder builder) : ModbusClientBase, IModbusRtuClient
{
    private TaskCompletionSource? _readTaskCompletionSource;
    private CancellationTokenSource? _receiveCancellationTokenSource;
    private SerialPort? _serialPort;
    private ReadOnlyMemory<byte> _buffer = ReadOnlyMemory<byte>.Empty;

    public async ValueTask<bool> ConnectAsync(CancellationToken token = default)
    {
        _serialPort ??= new(options.PortName, options.BaudRate, options.Parity, options.DataBits, options.StopBits);
        _serialPort.RtsEnable = options.RtsEnable;
        _serialPort.DtrEnable = options.DtrEnable;

        _serialPort.Handshake = options.Handshake;
        _serialPort.DiscardNull = options.DiscardNull;
        _serialPort.ReadBufferSize = options.ReadBufferSize;
        _serialPort.WriteBufferSize = options.WriteBufferSize;

        _serialPort.ReadTimeout = options.ReadTimeout;
        _serialPort.WriteTimeout = options.WriteTimeout;

        _serialPort.DataReceived += DataReceived;
        _serialPort.ErrorReceived += ErrorReceived;

        var ret = false;
        try
        {
            await Task.Run(() => _serialPort.Open(), token);
            ret = true;
        }
        catch (Exception ex)
        {
            Exception = ex;
        }

        return ret;
    }

    private async Task<ReadOnlyMemory<byte>> SendAsync(ReadOnlyMemory<byte> data)
    {
        // 取消等待读取的任务
        _readTaskCompletionSource?.TrySetCanceled();
        _readTaskCompletionSource = new TaskCompletionSource();

        // 取消接收数据的任务
        _receiveCancellationTokenSource?.Cancel();
        _receiveCancellationTokenSource?.Dispose();
        _receiveCancellationTokenSource = new();

        // 清空缓存
        _buffer = ReadOnlyMemory<byte>.Empty;

        var serialPort = GetSerialPort();
        serialPort.Write(data.ToArray(), 0, data.Length);

        await _readTaskCompletionSource.Task.WaitAsync(_receiveCancellationTokenSource.Token);

        return _buffer.ToArray();
    }

    protected override async ValueTask<ReadOnlyMemory<byte>> ReadAsync(byte slaveAddress, byte functionCode, ushort startAddress, ushort numberOfPoints)
    {
        // 构建请求报文
        var request = builder.BuildReadRequest(slaveAddress, functionCode, startAddress, numberOfPoints);

        // 发送请求
        var received = await SendAsync(request);

        // 验证响应报文
        if (!builder.TryValidateReadResponse(received, slaveAddress, functionCode, out var exception))
        {
            Exception = exception;
            return default;
        }

        return received;
    }

    protected override async ValueTask<bool> WriteBoolValuesAsync(byte slaveAddress, byte functionCode, ushort address, bool[] values)
    {
        // 构建请求报文
        var data = WriteBoolValues(address, values);
        var request = builder.BuildWriteRequest(slaveAddress, functionCode, data);

        // 发送请求
        var received = await SendAsync(request);

        // 验证响应报文
        if (!builder.TryValidateWriteResponse(received, slaveAddress, functionCode, data, out var exception))
        {
            Exception = exception;
            return false;
        }

        return true;
    }

    protected override async ValueTask<bool> WriteUShortValuesAsync(byte slaveAddress, byte functionCode, ushort address, ushort[] values)
    {
        // 构建请求报文
        var data = WriteUShortValues(address, values);
        var request = builder.BuildWriteRequest(slaveAddress, functionCode, data);

        // 发送请求
        var received = await SendAsync(request);

        // 验证响应报文
        if (!builder.TryValidateWriteResponse(received, slaveAddress, functionCode, data, out var exception))
        {
            Exception = exception;
            return false;
        }

        return true;
    }

    private SerialPort GetSerialPort()
    {
        if (_serialPort is not { IsOpen: true })
        {
            throw new InvalidOperationException("站点未连接请先调用 ConnectAsync 方法连接设备");
        }

        _serialPort.DiscardInBuffer();
        _serialPort.DiscardOutBuffer();

        return _serialPort;
    }

    private void DataReceived(object? sender, SerialDataReceivedEventArgs e)
    {
        if (e.EventType == SerialData.Chars)
        {
            if (_serialPort != null)
            {
                var bytesToRead = _serialPort.BytesToRead;
                if (bytesToRead > 0)
                {
                    var buffer = new byte[bytesToRead];
                    if (_serialPort.Read(buffer, 0, bytesToRead) == buffer.Length)
                    {
                        _buffer = buffer;
                    }
                }
            }
        }

        _readTaskCompletionSource?.TrySetResult();
    }

    private void ErrorReceived(object? sender, SerialErrorReceivedEventArgs e)
    {
        // 处理串口错误
        Exception = new IOException($"Serial port error: {e.EventType}");
        _readTaskCompletionSource?.TrySetResult();
    }

    protected override bool[] ReadBoolValues(ReadOnlyMemory<byte> response, ushort numberOfPoints)
    {
        var values = new bool[numberOfPoints];
        for (var i = 0; i < numberOfPoints; i++)
        {
            var byteIndex = 3 + i / 8;
            var bitIndex = i % 8;
            values[i] = (response.Span[byteIndex] & (1 << bitIndex)) != 0;
        }

        return values;
    }

    protected override ushort[] ReadUShortValues(ReadOnlyMemory<byte> response, ushort numberOfPoints)
    {
        var values = new ushort[numberOfPoints];
        for (var i = 0; i < numberOfPoints; i++)
        {
            int offset = 3 + (i * 2);
            values[i] = (ushort)((response.Span[offset] << 8) | response.Span[offset + 1]);
        }

        return values;
    }

    private static ReadOnlyMemory<byte> WriteBoolValues(ushort address, bool[] values)
    {
        int byteCount = (values.Length + 7) / 8;
        var data = new byte[values.Length > 1 ? 5 + byteCount : 4];
        data[0] = (byte)(address >> 8);
        data[1] = (byte)address;

        if (values.Length > 1)
        {
            // 多值时，写入数量
            data[2] = (byte)(values.Length >> 8);
            data[3] = (byte)(values.Length);

            // 字节数
            data[4] = (byte)(byteCount);

            for (var i = 0; i < values.Length; i++)
            {
                if (values[i])
                {
                    int byteIndex = 5 + i / 8;
                    int bitIndex = i % 8;
                    data[byteIndex] |= (byte)(1 << bitIndex);
                }
            }
        }
        else
        {
            // 组装数据
            data[2] = values[0] ? (byte)0xFF : (byte)0x00;
            data[3] = 0x00;
        }

        return data;
    }

    private static ReadOnlyMemory<byte> WriteUShortValues(ushort address, ushort[] values)
    {
        int byteCount = values.Length * 2;
        var data = new byte[values.Length > 1 ? 5 + byteCount : 4];
        data[0] = (byte)(address >> 8);
        data[1] = (byte)address;

        if (values.Length > 1)
        {
            // 多值时，写入数量
            data[2] = (byte)(values.Length >> 8);
            data[3] = (byte)(values.Length);

            // 字节数
            data[4] = (byte)(byteCount);

            for (var i = 0; i < values.Length; i++)
            {
                data[i * 2 + 5] = (byte)(values[i] >> 8);
                data[i * 2 + 6] = (byte)(values[i] & 0xFF);
            }
        }
        else
        {
            data[2] = (byte)(values[0] >> 8);
            data[3] = (byte)(values[0] & 0xFF);
        }

        return data;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public ValueTask CloseAsync()
    {
        // 取消等待读取的任务
        if (_readTaskCompletionSource != null)
        {
            _readTaskCompletionSource.TrySetCanceled();
            _readTaskCompletionSource = null;
        }

        // 取消接收数据的任务
        if (_receiveCancellationTokenSource != null)
        {
            _receiveCancellationTokenSource.Cancel();
            _receiveCancellationTokenSource.Dispose();
            _receiveCancellationTokenSource = null;
        }

        if (_serialPort is { IsOpen: true })
        {
            _serialPort.Close();
        }
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="disposing"></param>
    /// <returns></returns>
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            await CloseAsync();
        }
    }
}
