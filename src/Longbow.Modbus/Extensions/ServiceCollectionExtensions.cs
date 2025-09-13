// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using Longbow.Modbus;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Runtime.Versioning;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// IModbusFactory 扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 增加 <see cref="IModbusFactory"/> 服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    [UnsupportedOSPlatform("browser")]
    public static IServiceCollection AddModbusFactory(this IServiceCollection services)
    {
        // 添加 IModbusFactory 服务
        services.AddSingleton<IModbusFactory, DefaultModbusFactory>();

        // 添加解析器服务
        services.TryAddTransient<IModbusTcpMessageBuilder, DefaultTcpMessageBuilder>();
        services.TryAddTransient<IModbusRtuMessageBuilder, DefaultRtuMessageBuilder>();

        // 添加 SerialPort 服务
        services.AddSerialPortFactory();

        // 添加 TcpSocket 服务
        services.AddTcpSocketFactory();

        return services;
    }
}
