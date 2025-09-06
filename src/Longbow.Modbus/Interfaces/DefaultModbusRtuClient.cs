// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using System.IO.Ports;

namespace Longbow.Modbus;

class DefaultModbusRtuClient(ModbusRtuClientOptions options) : DefaultModbusClientBase, IModbusRtuClient
{
    private TaskCompletionSource? _readTaskCompletionSource;
    private CancellationTokenSource? _receiveCancellationTokenSource;
    private readonly ModbusTcpMessageBuilder _builder = new();
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
                _serialPort.Read(buffer, 0, bytesToRead);
            }
        }
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
        var request = _builder.BuildReadRequest(slaveAddress, functionCode, startAddress, numberOfPoints);
        _serialPort.Write(request.ToArray(), 0, request.Length);

        _readTaskCompletionSource = new TaskCompletionSource();
        _receiveCancellationTokenSource ??= new();
        await _readTaskCompletionSource.Task.WaitAsync(_receiveCancellationTokenSource.Token);

        var received = ReadOnlyMemory<byte>.Empty;
        if (!_builder.TryValidateReadResponse(received, functionCode, out var exception))
        {
            Exception = exception;
            return default;
        }

        return received;
    }

    protected override async ValueTask<bool> WriteBoolValuesAsync(byte slaveAddress, byte functionCode, ushort address, bool[] values)
    {
        if (_serialPort is not { IsOpen: true })
        {
            throw new InvalidOperationException("站点未连接请先调用 ConnectAsync 方法连接设备");
        }

        var data = WriteBoolValues(address, values);
        var request = _builder.BuildWriteRequest(slaveAddress, functionCode, data);
        _serialPort.Write(request.ToArray(), 0, request.Length);

        _readTaskCompletionSource = new TaskCompletionSource();
        _receiveCancellationTokenSource ??= new();
        await _readTaskCompletionSource.Task.WaitAsync(_receiveCancellationTokenSource.Token);

        var received = ReadOnlyMemory<byte>.Empty;
        if (!_builder.TryValidateReadResponse(received, functionCode, out var exception))
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

        var data = WriteUShortValues(address, values);
        var request = _builder.BuildWriteRequest(slaveAddress, functionCode, data);
        _serialPort.Write(request.ToArray(), 0, request.Length);

        _readTaskCompletionSource = new TaskCompletionSource();
        _receiveCancellationTokenSource ??= new();
        await _readTaskCompletionSource.Task.WaitAsync(_receiveCancellationTokenSource.Token);

        var received = ReadOnlyMemory<byte>.Empty;
        if (!_builder.TryValidateReadResponse(received, functionCode, out var exception))
        {
            Exception = exception;
            return false;
        }

        return true;
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
