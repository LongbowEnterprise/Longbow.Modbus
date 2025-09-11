# Longbow.Modbus

Longbow.Modbus 是一个用于与支持 Modbus 协议的设备进行通信的 .NET 库。它支持多种传输方式，包括 TCP、RTU、UDP 以及 RTU over TCP/UDP。

## 🚀 特性

- 支持 Modbus TCP、RTU、UDP 以及 RTU over TCP/UDP 通信。
- 提供异步 API 以实现高效的非阻塞通信。
- 支持常见的 Modbus 功能码，如读取线圈、输入寄存器、保持寄存器，以及写入单个或多个线圈和寄存器。
- 提供依赖注入集成，便于在现代 .NET 应用程序中使用。
- 支持连接池管理，提高性能和资源利用率。

## 📦 安装

你可以通过 NuGet 安装 Longbow.Modbus：

```bash
dotnet add package Longbow.Modbus
```

## 🛠️ 快速开始

### 1. 注册服务

如果你使用的是依赖注入（如 ASP.NET Core），可以在 `Startup.cs` 或 `Program.cs` 中注册 Modbus 服务：

```csharp
services.AddModbusFactory();
```

### 2. 创建 Modbus 客户端

你可以通过 `IModbusFactory` 创建不同类型的 Modbus 客户端：

```csharp
var modbusFactory = serviceProvider.GetRequiredService<IModbusFactory>();

// 创建 TCP 客户端
var tcpClient = modbusFactory.GetOrCreateTcpMaster("tcp-client", options =>
{
    options.ConnectTimeout = 5000;
    options.LocalEndPoint = new IPEndPoint(IPAddress.Any, 0);
});

// 创建 RTU 客户端
var rtuClient = modbusFactory.GetOrCreateRtuMaster("rtu-client", options =>
{
    options.PortName = "COM1";
    options.BaudRate = 9600;
});
```

### 3. 连接到 Modbus 设备

根据使用的协议，连接到设备：

```csharp
// TCP 连接
await tcpClient.ConnectAsync(new IPEndPoint(IPAddress.Parse("192.168.0.1"), 502));

// RTU 连接
await rtuClient.ConnectAsync();
```

### 4. 读取数据

你可以使用以下方法读取不同类型的数据：

```csharp
// 读取线圈
bool[] coils = await client.ReadCoilsAsync(1, 0, 10);

// 读取输入寄存器
ushort[] inputRegisters = await client.ReadInputRegistersAsync(1, 0, 10);

// 读取保持寄存器
ushort[] holdingRegisters = await client.ReadHoldingRegistersAsync(1, 0, 10);
```

### 5. 写入数据

你可以使用以下方法写入数据：

```csharp
// 写入单个线圈
await client.WriteCoilAsync(1, 0, true);

// 写入多个线圈
await client.WriteMultipleCoilsAsync(1, 0, new bool[] { true, false, true });

// 写入单个寄存器
await client.WriteRegisterAsync(1, 0, 1234);

// 写入多个寄存器
await client.WriteMultipleRegistersAsync(1, 0, new ushort[] { 1234, 5678 });
```

## 🔧 支持的 Modbus 功能码

- **0x01** - 读取线圈状态
- **0x02** - 读取输入状态
- **0x03** - 读取保持寄存器
- **0x04** - 读取输入寄存器
- **0x05** - 写入单个线圈
- **0x06** - 写入单个寄存器
- **0x0F** - 写入多个线圈
- **0x10** - 写入多个寄存器

## 🚧 注意事项

- 确保设备的 IP 地址、端口、串口设置等配置正确。
- 使用异步方法以避免阻塞主线程。
- 在使用 RTU 协议时，确保串口设备已正确连接并配置。
- 在使用连接池时，确保合理管理客户端实例以避免资源泄漏。

## 🤝 贡献

欢迎贡献代码和文档！请参考 [CONTRIBUTING.md](CONTRIBUTING.md) 获取更多信息。

## 📄 许可证

本项目采用 [MIT License](LICENSE)，请查看 `LICENSE` 文件以获取详细信息。

## 🔗 相关链接

- [GitHub 项目主页](https://gitee.com/LongbowEnterprise/Longbow.Modbus)
- [NuGet 包](https://www.nuget.org/packages/Longbow.Modbus)

## 📞 联系方式

如需联系开发者，请查看项目主页或提交问题到 [Issues](https://gitee.com/LongbowEnterprise/Longbow.Modbus/issues)。
