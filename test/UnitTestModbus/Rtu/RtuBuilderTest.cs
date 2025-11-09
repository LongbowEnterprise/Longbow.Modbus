// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using Microsoft.Extensions.DependencyInjection;

namespace UnitTest.Rtu;

public class RtuBuilderTest
{
    [Fact]
    public void TryValidateReadResponse_Ok()
    {
        var sc = new ServiceCollection();
        sc.AddModbusFactory();

        var provider = sc.BuildServiceProvider();
        var builder = provider.GetRequiredService<IModbusRtuMessageBuilder>();

        // 01 01 01 1F 10 40

        // 长度小于 5
        var response = new byte[] { 0x01, 0x01, 0x01, 0x1F };
        var v = builder.TryValidateReadResponse(response, 0x01, 0x01, out var ex);
        Assert.False(v);
        Assert.NotNull(ex);

        // 从站地址不匹配
        response = [0x01, 0x01, 0x01, 0x1F, 0x10, 0x40];
        v = builder.TryValidateReadResponse(response, 0x02, 0x03, out ex);
        Assert.False(v);
        Assert.NotNull(ex);

        // 功能码不匹配
        response = [0x01, 0x81, 0x01, 0x1F, 0x10, 0x40];
        v = builder.TryValidateReadResponse(response, 0x01, 0x01, out ex);
        Assert.False(v);
        Assert.NotNull(ex);

        response = [0x01, 0x81, 0x02, 0x1F, 0x10, 0x40];
        builder.TryValidateReadResponse(response, 0x01, 0x01, out _);

        response = [0x01, 0x81, 0x03, 0x1F, 0x10, 0x40];
        builder.TryValidateReadResponse(response, 0x01, 0x01, out _);

        response = [0x01, 0x81, 0x04, 0x1F, 0x10, 0x40];
        builder.TryValidateReadResponse(response, 0x01, 0x01, out _);

        response = [0x01, 0x81, 0x05, 0x1F, 0x10, 0x40];
        builder.TryValidateReadResponse(response, 0x01, 0x01, out _);

        response = [0x01, 0x02, 0x01, 0x1F, 0x10, 0x40];
        builder.TryValidateReadResponse(response, 0x01, 0x01, out _);

        // 数据长度不合规
        response = [0x01, 0x01, 0x02, 0x1F, 0x10, 0x40];
        v = builder.TryValidateReadResponse(response, 0x01, 0x01, out ex);
        Assert.False(v);
        Assert.NotNull(ex);

        // CRC 校验失败
        response = [0x01, 0x01, 0x01, 0x1F, 0x10, 0x44];
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
        var builder = provider.GetRequiredService<IModbusRtuMessageBuilder>();

        // 01 05 00 00 FF 00 8C 3A

        // 长度小于 5
        var data = new byte[] { 0x01, 0x05, 0x00, 0x00, 0xFF, 0x00, 0x8C, 0x3A };
        var response = new byte[] { 0x01, 0x01, 0x01, 0x1F };
        var v = builder.TryValidateWriteResponse(response, 0x01, 0x01, data, out var ex);
        Assert.False(v);
        Assert.NotNull(ex);

        // 从站地址不匹配
        response = [0x01, 0x05, 0x00, 0x01, 0xFF, 0x00, 0x8C, 0x3A];
        v = builder.TryValidateWriteResponse(response, 0x01, 0x05, data, out ex);
        Assert.False(v);
        Assert.NotNull(ex);

        // 功能码不匹配
        response = [0x01, 0x05, 0x00, 0x00, 0xFF, 0x00, 0x8C];
        v = builder.TryValidateWriteResponse(response, 0x01, 0x05, data, out _);

        response = [0x01, 0x05, 0x00, 0x01, 0xFF, 0x00, 0x8C, 0x3A];
        v = builder.TryValidateWriteResponse(response, 0x01, 0x05, data, out ex);
        Assert.False(v);
        Assert.NotNull(ex);

        // CRC 校验失败
        response = [0x01, 0x05, 0x00, 0x00, 0xFF, 0x00, 0x8C, 0x3B];
        v = builder.TryValidateWriteResponse(response, 0x01, 0x05, data, out ex);
        Assert.False(v);
        Assert.NotNull(ex);
    }
}
