// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using Longbow.SerialPorts;

namespace Longbow.Modbus;

class DefaultRtuClient(ISerialPortClient client, IModbusRtuMessageBuilder builder) : ModbusClientBase(builder), IModbusRtuClient
{
    public async ValueTask<bool> ConnectAsync(CancellationToken token = default)
    {
        var ret = false;
        try
        {
            ret = await client.OpenAsync(token);
        }
        catch (Exception ex)
        {
            Exception = ex;
        }

        return ret;
    }

    protected override async Task<ReadOnlyMemory<byte>> SendAsync(ReadOnlyMemory<byte> request, CancellationToken token = default)
    {
        await client.SendAsync(request, token);
        var response = await client.ReceiveAsync(token);
        return response;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override async ValueTask CloseAsync()
    {
        await client.CloseAsync();
    }
}
