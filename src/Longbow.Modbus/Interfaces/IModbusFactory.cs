// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

namespace Longbow.Modbus;

/// <summary>
/// ITcpSocketFactory Interface
/// </summary>
public interface IModbusFactory
{
    /// <summary>
    /// 获得/创建 <see cref="IModbusTcpClient"/> TcpClient 客户端实例
    /// </summary>
    /// <param name="name"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    IModbusTcpClient GetOrCreateTcpMaster(string name, Action<ModbusTcpClientOptions>? configureOptions = null);

    /// <summary>
    /// 获得/创建 <see cref="IModbusTcpClient"/> TcpClient 客户端实例
    /// </summary>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    IModbusTcpClient GetOrCreateTcpMaster(Action<ModbusTcpClientOptions>? configureOptions = null);

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
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    IModbusRtuClient GetOrCreateRtuMaster(string name, Action<ModbusRtuClientOptions>? configureOptions = null);

    /// <summary>
    /// 获得/创建 <see cref="IModbusRtuClient"/> RtuClient 客户端实例
    /// </summary>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    IModbusRtuClient GetOrCreateRtuMaster(Action<ModbusRtuClientOptions>? configureOptions = null);

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
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    IModbusTcpClient GetOrCreateUdpMaster(string name, Action<ModbusUdpClientOptions>? configureOptions = null);

    /// <summary>
    /// 获得/创建 <see cref="IModbusTcpClient"/> UdpClient 客户端实例
    /// </summary>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    IModbusTcpClient GetOrCreateUdpMaster(Action<ModbusUdpClientOptions>? configureOptions = null);

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
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    IModbusTcpClient GetOrCreateRtuOverTcpMaster(string name, Action<ModbusTcpClientOptions>? configureOptions = null);

    /// <summary>
    /// 获得/创建 <see cref="IModbusTcpClient"/> RTU Over TcpClient 客户端实例
    /// </summary>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    IModbusTcpClient GetOrCreateRtuOverTcpMaster(Action<ModbusTcpClientOptions>? configureOptions = null);

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
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    IModbusTcpClient GetOrCreateRtuOverUdpMaster(string name, Action<ModbusUdpClientOptions>? configureOptions = null);

    /// <summary>
    /// 获得/创建 <see cref="IModbusTcpClient"/> RTU Over UdpClient 客户端实例
    /// </summary>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    IModbusTcpClient GetOrCreateRtuOverUdpMaster(Action<ModbusUdpClientOptions>? configureOptions = null);

    /// <summary>
    /// 移除指定名称的 <see cref="IModbusTcpClient"/> 客户端实例
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    IModbusTcpClient? RemoveRtuOverUdpMaster(string name);
}
