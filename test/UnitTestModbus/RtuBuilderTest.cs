// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using System.Reflection;

namespace UnitTestModbus;

public class RtuBuilderTest
{
    [Fact]
    public void TryValidateReadResponse_Ok()
    {
        // 01 01 01 1F 10 40

        // 长度小于 5
        var response = new byte[] { 0x01, 0x01, 0x01, 0x1F };
        var v = TryValidateReadResponse(response, 0x01, 0x01, out var ex);
        Assert.False(v);
        Assert.NotNull(ex);

        // 从站地址不匹配
        response = [0x01, 0x01, 0x01, 0x1F, 0x10, 0x40];
        v = TryValidateReadResponse(response, 0x02, 0x03, out ex);
        Assert.False(v);
        Assert.NotNull(ex);

        // 功能码不匹配
        response = [0x01, 0x81, 0x01, 0x1F, 0x10, 0x40];
        v = TryValidateReadResponse(response, 0x01, 0x01, out ex);
        Assert.False(v);
        Assert.NotNull(ex);

        response = [0x01, 0x81, 0x02, 0x1F, 0x10, 0x40];
        TryValidateReadResponse(response, 0x01, 0x01, out _);

        response = [0x01, 0x81, 0x03, 0x1F, 0x10, 0x40];
        TryValidateReadResponse(response, 0x01, 0x01, out _);

        response = [0x01, 0x81, 0x04, 0x1F, 0x10, 0x40];
        TryValidateReadResponse(response, 0x01, 0x01, out _);

        response = [0x01, 0x81, 0x05, 0x1F, 0x10, 0x40];
        TryValidateReadResponse(response, 0x01, 0x01, out _);

        response = [0x01, 0x02, 0x01, 0x1F, 0x10, 0x40];
        TryValidateReadResponse(response, 0x01, 0x01, out _);

        // 数据长度不合规
        response = [0x01, 0x01, 0x02, 0x1F, 0x10, 0x40];
        v = TryValidateReadResponse(response, 0x01, 0x01, out ex);
        Assert.False(v);
        Assert.NotNull(ex);

        // CRC 校验失败
        response = [0x01, 0x01, 0x01, 0x1F, 0x10, 0x44];
        v = TryValidateReadResponse(response, 0x01, 0x01, out ex);
        Assert.False(v);
        Assert.NotNull(ex);
    }

    private static bool? TryValidateReadResponse(ReadOnlyMemory<byte> response, byte slaveAddress, byte functionCode, [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;
        var type = Type.GetType("Longbow.Modbus.ModbusRtuMessageBuilder, Longbow.Modbus");
        if (type == null)
        {
            return null;
        }

        var method = type.GetMethod("TryValidateReadResponse", BindingFlags.Static | BindingFlags.Public);
        if (method == null)
        {
            return null;
        }

        var parameters = new object?[] { response, slaveAddress, functionCode, null };
        var val = method.Invoke(null, parameters);
        if (val is bool b)
        {
            exception = (Exception)parameters[3]!;
            return b;
        }
        return null;
    }
}
