// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace UnitTest.Tcp;

public class TcpBuilderTest
{
    [Fact]
    public void TryValidateReadResponse_Ok()
    {
        var sc = new ServiceCollection();
        sc.AddModbusFactory();

        var provider = sc.BuildServiceProvider();
        var builder = provider.GetRequiredService<IModbusTcpMessageBuilder>();

        // 00 02 00 00 00 04 01 01 01 1F

        // 长度小于 9
        var response = new byte[] { 0x01, 0x01, 0x01, 0x1F };
        var v = builder.TryValidateReadResponse(response, 0x01, 0x01, out var ex);
        Assert.False(v);
        Assert.NotNull(ex);

        // 事务标识符不匹配
        response = [0x00, 0x02, 0x00, 0x00, 0x00, 0x04, 0x01, 0x01, 0x01, 0x1F, 0x10, 0x40];
        v = builder.TryValidateReadResponse(response, 0x01, 0x01, out ex);
        Assert.False(v);
        Assert.NotNull(ex);

        response = [0x01, 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x01, 0x01, 0x1F, 0x10, 0x40];
        v = builder.TryValidateReadResponse(response, 0x01, 0x01, out ex);
        Assert.False(v);
        Assert.NotNull(ex);

        // 从站地址不匹配
        response = [0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x01, 0x01, 0x1F, 0x10, 0x40];
        v = builder.TryValidateReadResponse(response, 0x02, 0x01, out ex);
        Assert.False(v);
        Assert.NotNull(ex);

        // 功能码不匹配
        response = [0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x01, 0x01, 0x1F, 0x10, 0x40];
        v = builder.TryValidateReadResponse(response, 0x01, 0x02, out ex);
        Assert.False(v);
        Assert.NotNull(ex);

        response = [0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x81, 0x01, 0x1F, 0x10, 0x40];
        builder.TryValidateReadResponse(response, 0x01, 0x01, out _);

        response = [0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x81, 0x02, 0x1F, 0x10, 0x40];
        builder.TryValidateReadResponse(response, 0x01, 0x01, out _);

        response = [0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x81, 0x03, 0x1F, 0x10, 0x40];
        builder.TryValidateReadResponse(response, 0x01, 0x01, out _);

        response = [0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x81, 0x04, 0x1F, 0x10, 0x40];
        builder.TryValidateReadResponse(response, 0x01, 0x01, out _);

        response = [0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x81, 0x05, 0x1F, 0x10, 0x40];
        builder.TryValidateReadResponse(response, 0x01, 0x01, out _);

        // 数据长度不合规
        response = [0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x01, 0x05, 0x1F, 0x10, 0x40];
        v = builder.TryValidateReadResponse(response, 0x01, 0x01, out ex);
        Assert.False(v);
        Assert.NotNull(ex);
    }

    [Fact]
    public void TryValidateWriteResponse_Ok()
    {
        var sc = new ServiceCollection();
        sc.AddModbusFactory();

        var provider = sc.BuildServiceProvider();
        var builder = provider.GetRequiredService<IModbusTcpMessageBuilder>();

        // 00 01 00 00 00 06 01 05 00 00 FF 00

        // 长度小于 12
        var data = new byte[] { 0x00, 0x00, 0xFF, 0x00 };
        var response = new byte[] { 0x00, 0x01, 0x00, 0x00, 0x00, 0x06, 0x01, 0x05, 0x00, 0x00, 0xFF, 0x00 };
        var v = builder.TryValidateWriteResponse(response, 0x01, 0x01, data, out var ex);
        Assert.False(v);
        Assert.NotNull(ex);

        // 功能码不匹配
        response = [0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x01, 0x05, 0x00, 0x00, 0xFF, 0x00];
        builder.TryValidateWriteResponse(response, 0x01, 0x06, data, out _);

        // 数据不匹配
        response = [0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x01, 0x05, 0x01, 0x00, 0xFF, 0x00];
        builder.TryValidateWriteResponse(response, 0x01, 0x05, data, out _);

        data = [0x00, 0x00, 0xFF, 0x01];
        response = [0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x01, 0x05, 0x01, 0x00, 0xFF, 0x00];
        builder.TryValidateWriteResponse(response, 0x01, 0x05, data, out _);
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

    private object? _instance;
    private uint GetTransactionId()
    {
        var type = Type.GetType("Longbow.Modbus.DefaultTcpMessageBuilder, Longbow.Modbus") ?? throw new InvalidOperationException();
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
        var type = Type.GetType("Longbow.Modbus.DefaultTcpMessageBuilder, Longbow.Modbus") ?? throw new InvalidOperationException();
        var field = type.GetField("_transactionId", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException();

        _instance ??= Activator.CreateInstance(type);
        Assert.NotNull(_instance);

        field.SetValue(_instance, value);
    }
}
