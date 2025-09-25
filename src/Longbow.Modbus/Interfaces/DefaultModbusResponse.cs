// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

namespace Longbow.Modbus;

sealed class DefaultModbusResponse(ReadOnlyMemory<byte> buffer, IModbusMessageBuilder builder, Exception? exception) : IModbusResponse
{
    public ReadOnlyMemory<byte> Buffer => buffer;

    public IModbusMessageBuilder Builder => builder;

    public Exception? Exception => exception;

    public bool IsSuccess => Exception == null;
}
