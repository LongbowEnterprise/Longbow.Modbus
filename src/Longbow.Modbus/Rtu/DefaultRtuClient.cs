// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using Longbow.SerialPorts;

namespace Longbow.Modbus;

class DefaultRtuClient(ISerialPortClient client, IModbusRtuMessageBuilder builder) : ModbusClientBase(builder), IModbusRtuClient
{
    public ValueTask<bool> ConnectAsync(CancellationToken token = default) => client.OpenAsync(token);

    protected override async Task<ReadOnlyMemory<byte>> SendAsync(ReadOnlyMemory<byte> request, CancellationToken token = default)
    {
        var ret = await client.SendAsync(request, token);

        return ret ? await client.ReceiveAsync(token) : ReadOnlyMemory<byte>.Empty;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override async ValueTask CloseAsync()
    {
        if (client.IsOpen)
        {
            await client.CloseAsync();
        }
    }
}
