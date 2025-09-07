﻿// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using System.Runtime.Versioning;

namespace Longbow.Modbus;

/// <summary>
/// ITcpSocketFactory Interface
/// </summary>
[UnsupportedOSPlatform("browser")]
public interface IModbusFactory
{
    /// <summary>
    /// 获得/创建 <see cref="IModbusTcpClient"/> TcpClient 客户端实例
    /// </summary>
    /// <param name="name"></param>
    /// <param name="valueFactory"></param>
    /// <returns></returns>
    IModbusTcpClient GetOrCreateTcpMaster(string? name = null, Action<ModbusTcpClientOptions>? valueFactory = null);

    /// <summary>
    /// 移除指定名称的 <see cref="IModbusTcpClient"/> 客户端实例
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    IModbusTcpClient? RemoveTcpMaster(string name);

    /// <summary>
    /// 获得/创建 <see cref="IModbusRtuClient"/> RtuClient 客户端实例
    /// </summary>
    /// <param name="name"></param>
    /// <param name="valueFactory"></param>
    /// <returns></returns>
    IModbusRtuClient GetOrCreateRtuMaster(string? name = null, Action<ModbusRtuClientOptions>? valueFactory = null);

    /// <summary>
    /// 移除指定名称的 <see cref="IModbusRtuClient"/> 客户端实例
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    IModbusRtuClient? RemoveRtuMaster(string name);

    /// <summary>
    /// 获得/创建 <see cref="IModbusTcpClient"/> UdpClient 客户端实例
    /// </summary>
    /// <param name="name"></param>
    /// <param name="valueFactory"></param>
    /// <returns></returns>
    IModbusTcpClient GetOrCreateUdpMaster(string? name = null, Action<ModbusUdpClientOptions>? valueFactory = null);

    /// <summary>
    /// 移除指定名称的 <see cref="IModbusTcpClient"/> 客户端实例
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    IModbusTcpClient? RemoveUdpMaster(string name);

    /// <summary>
    /// 获得/创建 <see cref="IModbusTcpClient"/> RTU Over TcpClient 客户端实例
    /// </summary>
    /// <param name="name"></param>
    /// <param name="valueFactory"></param>
    /// <returns></returns>
    IModbusTcpClient GetOrCreateRtuOverTcpMaster(string? name = null, Action<ModbusTcpClientOptions>? valueFactory = null);

    /// <summary>
    /// 移除指定名称的 <see cref="IModbusTcpClient"/> 客户端实例
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    IModbusTcpClient? RemoveRtuOverTcpMaster(string name);

    /// <summary>
    /// 获得/创建 <see cref="IModbusTcpClient"/> RTU Over UdpClient 客户端实例
    /// </summary>
    /// <param name="name"></param>
    /// <param name="valueFactory"></param>
    /// <returns></returns>
    IModbusTcpClient GetOrCreateRtuOverUdpMaster(string? name = null, Action<ModbusUdpClientOptions>? valueFactory = null);

    /// <summary>
    /// 移除指定名称的 <see cref="IModbusTcpClient"/> 客户端实例
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    IModbusTcpClient? RemoveRtuOverUdpMaster(string name);
}
