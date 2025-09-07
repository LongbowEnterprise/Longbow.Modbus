// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using System.Reflection;

namespace UnitTestModbus;

public class TcpBuilderTest
{
    [Fact]
    public void TryValidateReadResponse_Ok()
    {
        // 00 02 00 00 00 04 01 01 01 1F

        // 长度小于 9
        var response = new byte[] { 0x01, 0x01, 0x01, 0x1F };
        var v = TryValidateReadResponse(response, 0x01, 0x01, out var ex);
        Assert.False(v);
        Assert.NotNull(ex);

        // 事务标识符不匹配
        response = [0x00, 0x02, 0x00, 0x00, 0x00, 0x04, 0x01, 0x01, 0x01, 0x1F, 0x10, 0x40];
        v = TryValidateReadResponse(response, 0x01, 0x01, out ex);
        Assert.False(v);
        Assert.NotNull(ex);

        response = [0x01, 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x01, 0x01, 0x1F, 0x10, 0x40];
        v = TryValidateReadResponse(response, 0x01, 0x01, out ex);
        Assert.False(v);
        Assert.NotNull(ex);

        // 从站地址不匹配
        response = [0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x01, 0x01, 0x1F, 0x10, 0x40];
        v = TryValidateReadResponse(response, 0x02, 0x01, out ex);
        Assert.False(v);
        Assert.NotNull(ex);

        // 功能码不匹配
        response = [0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x01, 0x01, 0x1F, 0x10, 0x40];
        v = TryValidateReadResponse(response, 0x01, 0x02, out ex);
        Assert.False(v);
        Assert.NotNull(ex);

        response = [0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x81, 0x01, 0x1F, 0x10, 0x40];
        TryValidateReadResponse(response, 0x01, 0x01, out _);

        response = [0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x81, 0x02, 0x1F, 0x10, 0x40];
        TryValidateReadResponse(response, 0x01, 0x01, out _);

        response = [0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x81, 0x03, 0x1F, 0x10, 0x40];
        TryValidateReadResponse(response, 0x01, 0x01, out _);

        response = [0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x81, 0x04, 0x1F, 0x10, 0x40];
        TryValidateReadResponse(response, 0x01, 0x01, out _);

        response = [0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x81, 0x05, 0x1F, 0x10, 0x40];
        TryValidateReadResponse(response, 0x01, 0x01, out _);

        // 数据长度不合规
        response = [0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x01, 0x05, 0x1F, 0x10, 0x40];
        v = TryValidateReadResponse(response, 0x01, 0x01, out ex);
        Assert.False(v);
        Assert.NotNull(ex);
    }

    [Fact]
    public void TryValidateWriteResponse_Ok()
    {
        // 00 01 00 00 00 06 01 05 00 00 FF 00

        // 长度小于 12
        var data = new byte[] { 0x00, 0x00, 0xFF, 0x00 };
        var response = new byte[] { 0x00, 0x01, 0x00, 0x00, 0x00, 0x06, 0x01, 0x05, 0x00, 0x00, 0xFF, 0x00 };
        var v = TryValidateWriteResponse(response, 0x01, 0x01, data, out var ex);
        Assert.False(v);
        Assert.NotNull(ex);

        // 功能码不匹配
        response = [0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x01, 0x05, 0x00, 0x00, 0xFF, 0x00];
        TryValidateWriteResponse(response, 0x01, 0x06, data, out _);

        // 数据不匹配
        response = [0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x01, 0x05, 0x01, 0x00, 0xFF, 0x00];
        TryValidateWriteResponse(response, 0x01, 0x05, data, out _);

        data = [0x00, 0x00, 0xFF, 0x01];
        response = [0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x01, 0x05, 0x01, 0x00, 0xFF, 0x00];
        TryValidateWriteResponse(response, 0x01, 0x05, data, out _);
    }

    [Fact]
    public void GetTransactionId_Ok()
    {
        var id = GetTransactionId();
        Assert.Equal((uint)1, id);

        id = GetTransactionId();
        Assert.Equal((uint)2, id);

        SetTransactionId(0xFFFF - 1);
        id = GetTransactionId();
        Assert.Equal((uint)0xFFFF, id);

        id = GetTransactionId();
        Assert.Equal((uint)0, id);
    }

    private static bool? TryValidateReadResponse(ReadOnlyMemory<byte> response, byte slaveAddress, byte functionCode, [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;
        var type = Type.GetType("Longbow.Modbus.ModbusTcpMessageBuilder, Longbow.Modbus");
        if (type == null)
        {
            return null;
        }

        var method = type.GetMethod("TryValidateReadResponse", BindingFlags.Instance | BindingFlags.Public);
        if (method == null)
        {
            return null;
        }

        var instance = Activator.CreateInstance(type);
        Assert.NotNull(instance);

        var parameters = new object?[] { response, slaveAddress, functionCode, null };
        var val = method.Invoke(instance, parameters);
        if (val is bool b)
        {
            exception = (Exception)parameters[3]!;
            return b;
        }
        return null;
    }

    private static bool? TryValidateWriteResponse(ReadOnlyMemory<byte> response, byte slaveAddress, byte functionCode, ReadOnlyMemory<byte> data, [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;
        var type = Type.GetType("Longbow.Modbus.ModbusTcpMessageBuilder, Longbow.Modbus");
        if (type == null)
        {
            return null;
        }

        var method = type.GetMethod("TryValidateWriteResponse", BindingFlags.Instance | BindingFlags.Public);
        if (method == null)
        {
            return null;
        }

        var instance = Activator.CreateInstance(type);
        Assert.NotNull(instance);

        var parameters = new object?[] { response, slaveAddress, functionCode, data, null };
        var val = method.Invoke(instance, parameters);
        if (val is bool b)
        {
            exception = (Exception)parameters[4]!;
            return b;
        }
        return null;
    }

    private object? _instance;
    private uint GetTransactionId()
    {
        var type = Type.GetType("Longbow.Modbus.ModbusTcpMessageBuilder, Longbow.Modbus") ?? throw new InvalidOperationException();
        var method = type.GetMethod("GetTransactionId", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException();

        _instance ??= Activator.CreateInstance(type);
        Assert.NotNull(_instance);

        var val = method.Invoke(_instance, []);
        if (val is uint v)
        {
            return v;
        }

        throw new InvalidOperationException();
    }

    private void SetTransactionId(uint value)
    {
        var type = Type.GetType("Longbow.Modbus.ModbusTcpMessageBuilder, Longbow.Modbus") ?? throw new InvalidOperationException();
        var field = type.GetField("_transactionId", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException();

        _instance ??= Activator.CreateInstance(type);
        Assert.NotNull(_instance);

        field.SetValue(_instance, value);
    }
}
