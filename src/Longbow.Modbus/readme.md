# Longbow.Modbus

[![License](https://img.shields.io/github/license/LongbowEnterprise/Longbow.Modbus.svg)](https://github.com/LongbowEnterprise/Longbow.Modbus/blob/master/LICENSE)
[![Nuget](https://img.shields.io/nuget/v/Longbow.Modbus.svg?color=red&logo=nuget&logoColor=green)](https://www.nuget.org/packages/Longbow.Modbus/)
[![Nuget](https://img.shields.io/nuget/dt/Longbow.Modbus.svg?logo=nuget&logoColor=green)](https://www.nuget.org/packages/Longbow.Modbus/)

一个高性能、异步的 .NET Modbus TCP 客户端库，支持多种 .NET 平台。

## 🚀 特性

- **异步操作**：所有 Modbus 操作都是异步的，提供更好的性能和响应能力
- **多框架支持**：支持 .NET 6、.NET 7、.NET 8 和 .NET 9
- **标准 Modbus 功能**：完整实现 Modbus TCP 协议的所有基础功能码
- **依赖注入友好**：原生支持 Microsoft.Extensions.DependencyInjection
- **连接池管理**：内置连接池，支持多个 Modbus 客户端实例管理
- **异常处理**：详细的异常信息和错误处理机制
- **高性能**：基于现代 .NET 异步编程模型，性能优异

## 📦 安装

通过 NuGet 包管理器安装：

`dotnet add package Longbow.Modbus`

或在 PackageReference 中添加：

`<PackageReference Include="Longbow.Modbus" Version="9.*" />`

## 🛠️ 快速开始

### 1. 注册服务
```
using Microsoft.Extensions.DependencyInjection;
using Longbow.Modbus;

var services = new ServiceCollection();
services.AddTcpSocketFactory();
services.AddModbusFactory();
var provider = services.BuildServiceProvider();
```

### 2. 创建 Modbus 客户端
```
var factory = provider.GetRequiredService<IModbusFactory>();
await using var client = factory.GetOrCreateTcpMaster("MyDevice");
```

### 3. 连接到 Modbus 设备
```
// 连接到 IP 地址为 192.168.1.100，端口为 502 的设备
await client.ConnectAsync("192.168.1.100", 502);
```

### 4. 读取数据
```
// 读取线圈状态 (功能码 0x01)
var coils = await client.ReadCoilsAsync(0x01, 0, 16);

// 读取离散输入 (功能码 0x02)
var discreteInputs = await client.ReadInputsAsync(0x01, 0, 16);

// 读取保持寄存器 (功能码 0x03)
var holdingRegisters = await client.ReadHoldingRegistersAsync(0x01, 0, 10);

// 读取输入寄存器 (功能码 0x04)
var inputRegisters = await client.ReadInputRegistersAsync(0x01, 0, 10);
```

### 5. 写入数据
```
// 写单个线圈 (功能码 0x05)
await client.WriteCoilAsync(0x01, 0, true);

// 写多个线圈 (功能码 0x0F)
bool[] coilValues = { true, false, true, false, true };
await client.WriteMultipleCoilsAsync(0x01, 0, coilValues);

// 写单个寄存器 (功能码 0x06)
await client.WriteRegisterAsync(0x01, 0, 1234);

// 写多个寄存器 (功能码 0x10)
ushort[] registerValues = { 100, 200, 300, 400 };
await client.WriteMultipleRegistersAsync(0x01, 0, registerValues);
```

## 🔧 支持的 Modbus 功能码

| 功能码 | 功能描述 | 方法名 |
|-------|----------|--------|
| 0x01  | 读线圈状态 | `ReadCoilsAsync` |
| 0x02  | 读离散输入 | `ReadInputsAsync` |
| 0x03  | 读保持寄存器 | `ReadHoldingRegistersAsync` |
| 0x04  | 读输入寄存器 | `ReadInputRegistersAsync` |
| 0x05  | 写单个线圈 | `WriteCoilAsync` |
| 0x06  | 写单个寄存器 | `WriteRegisterAsync` |
| 0x0F  | 写多个线圈 | `WriteMultipleCoilsAsync` |
| 0x10  | 写多个寄存器 | `WriteMultipleRegistersAsync` |

## 🚧 注意事项

1. **连接管理**：使用完毕后请调用 `CloseAsync()` 或使用 `await using` 确保连接正确关闭
2. **异常处理**：所有操作都可能抛出异常，请适当处理
3. **线程安全**：单个客户端实例不是线程安全的，多线程环境请创建多个实例
4. **超时设置**：根据网络环境调整超时时间，避免操作超时

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启 Pull Request

## 📄 许可证

本项目基于 [Apache License 2.0](LICENSE) 许可证开源。

## 🔗 相关链接

- [GitHub 仓库](https://github.com/LongbowEnterprise/Longbow.Modbus)
- [NuGet 包](https://www.nuget.org/packages/Longbow.Modbus/)
- [问题反馈](https://github.com/LongbowEnterprise/Longbow.Modbus/issues)

## 📞 联系方式

- 作者：Argo Zhang
- 邮箱：argo@live.ca
- 组织：Longbow Enterprise

---

如果这个项目对您有帮助，请考虑给我们一个 ⭐️！
