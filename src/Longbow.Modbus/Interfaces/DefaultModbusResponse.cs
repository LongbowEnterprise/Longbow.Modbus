// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

namespace Longbow.Modbus;

sealed class DefaultModbusResponse(ReadOnlyMemory<byte> response, IModbusMessageBuilder builder) : IModbusResponse
{
    public ReadOnlyMemory<byte> Buffer { get; } = response;

    public IModbusMessageBuilder Builder { get; } = builder;
}
