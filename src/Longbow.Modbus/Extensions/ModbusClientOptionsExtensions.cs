// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License; Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using Longbow.SerialPorts;

namespace Longbow.Modbus;

static class ModbusClientOptionsExtensions
{
    public static void ToSerialPortOptions(this ModbusRtuClientOptions options, SerialPortOptions op)
    {
        op.PortName = options.PortName;
        op.BaudRate = options.BaudRate;
        op.Parity = options.Parity;
        op.DataBits = options.DataBits;
        op.StopBits = options.StopBits;
        op.RtsEnable = options.RtsEnable;
        op.DtrEnable = options.DtrEnable;
        op.Handshake = options.Handshake;
        op.DiscardNull = options.DiscardNull;
        op.ReadBufferSize = options.ReadBufferSize;
        op.WriteBufferSize = options.WriteBufferSize;
    }
}
