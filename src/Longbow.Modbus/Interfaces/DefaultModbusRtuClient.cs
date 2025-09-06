// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using Longbow.Socket.Algorithm;
using System.IO.Ports;

namespace Longbow.Modbus;

class DefaultModbusRtuClient(ModbusRtuClientOptions options) : DefaultModbusClientBase, IModbusRtuClient
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

    private void DataReceived(object? sender, SerialDataReceivedEventArgs e)
    {
        if (e.EventType == SerialData.Chars && _serialPort is { IsOpen: true })
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

        _readTaskCompletionSource?.TrySetResult();
    }

    private void ErrorReceived(object? sender, SerialErrorReceivedEventArgs e)
    {
        // 处理串口错误
        Exception = new IOException($"Serial port error: {e.EventType}");
        _readTaskCompletionSource?.TrySetResult();
    }

    protected override async ValueTask<ReadOnlyMemory<byte>> ReadAsync(byte slaveAddress, byte functionCode, ushort startAddress, ushort numberOfPoints)
    {
        if (_serialPort is not { IsOpen: true })
        {
            throw new InvalidOperationException("站点未连接请先调用 ConnectAsync 方法连接设备");
        }

        // 发送数据
        _readTaskCompletionSource = new TaskCompletionSource();
        _receiveCancellationTokenSource = new();
        _buffer = ReadOnlyMemory<byte>.Empty;

        var request = ModbusRtuMessageBuilder.BuildReadRequest(slaveAddress, functionCode, startAddress, numberOfPoints);
        _serialPort.DiscardInBuffer();
        _serialPort.DiscardOutBuffer();
        _serialPort.Write(request.ToArray(), 0, request.Length);

        await _readTaskCompletionSource.Task.WaitAsync(_receiveCancellationTokenSource.Token);

        var received = _buffer.ToArray();
        if (!ModbusRtuMessageBuilder.TryValidateReadResponse(received, slaveAddress, functionCode, out var exception))
        {
            Exception = exception;
            return default;
        }

        return received;
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

    protected override async ValueTask<bool> WriteBoolValuesAsync(byte slaveAddress, byte functionCode, ushort address, bool[] values)
    {
        if (_serialPort is not { IsOpen: true })
        {
            throw new InvalidOperationException("站点未连接请先调用 ConnectAsync 方法连接设备");
        }

        _readTaskCompletionSource = new TaskCompletionSource();
        _receiveCancellationTokenSource = new();
        _buffer = ReadOnlyMemory<byte>.Empty;

        var data = WriteBoolValues(address, values);
        var request = ModbusRtuMessageBuilder.BuildWriteRequest(slaveAddress, functionCode, data);
        _serialPort.DiscardInBuffer();
        _serialPort.DiscardOutBuffer();
        _serialPort.Write(request.ToArray(), 0, request.Length);

        await _readTaskCompletionSource.Task.WaitAsync(_receiveCancellationTokenSource.Token);

        if (!ModbusRtuMessageBuilder.TryValidateWriteResponse(_buffer, functionCode, request, out var exception))
        {
            Exception = exception;
            return false;
        }

        return true;
    }

    protected override async ValueTask<bool> WriteUShortValuesAsync(byte slaveAddress, byte functionCode, ushort address, ushort[] values)
    {
        if (_serialPort is not { IsOpen: true })
        {
            throw new InvalidOperationException("站点未连接请先调用 ConnectAsync 方法连接设备");
        }

        _readTaskCompletionSource = new TaskCompletionSource();
        _receiveCancellationTokenSource = new();
        _buffer = ReadOnlyMemory<byte>.Empty;

        var data = WriteUShortValues(address, values);
        var request = ModbusRtuMessageBuilder.BuildWriteRequest(slaveAddress, functionCode, data);
        _serialPort.DiscardInBuffer();
        _serialPort.DiscardOutBuffer();
        _serialPort.Write(request.ToArray(), 0, request.Length);

        await _readTaskCompletionSource.Task.WaitAsync(_receiveCancellationTokenSource.Token);

        if (!ModbusRtuMessageBuilder.TryValidateWriteResponse(_buffer, functionCode, request, out var exception))
        {
            Exception = exception;
            return false;
        }

        return true;
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
