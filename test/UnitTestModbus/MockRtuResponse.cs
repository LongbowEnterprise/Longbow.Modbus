// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

namespace UnitTestModbus;

public class MockRtuResponse
{
    public static ReadOnlyMemory<byte> ReadCoilResponse() =>
        HexConverter.ToBytes("00 00 00 05 01 01 02 05 00", " ");

    public static ReadOnlyMemory<byte> ReadInputsResponse() =>
        HexConverter.ToBytes("00 00 00 05 01 02 02 00 00", " ");

    public static ReadOnlyMemory<byte> ReadHoldingRegistersResponse() =>
        HexConverter.ToBytes("00 00 00 17 01 03 14 00 0C 00 00 00 17 00 00 00 2E 00 00 00 01 00 02 00 04 00 05", " ");

    public static ReadOnlyMemory<byte> ReadInputRegistersResponse() =>
        HexConverter.ToBytes("00 00 00 17 01 04 14 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00", " ");

    public static ReadOnlyMemory<byte> WriteCoilResponse(ReadOnlyMemory<byte> request)
    {
        var v = request.Span[4] == 0xFF ? "01 05 00 00 FF 00 8C 3A" : "01 05 00 01 00 00 9C 0A";
        return HexConverter.ToBytes(v, " ");
    }

    public static ReadOnlyMemory<byte> WriteMultipleCoilsResponse() =>
        HexConverter.ToBytes("00 00 00 06 01 06 00 00 00 0C", " ");

    public static ReadOnlyMemory<byte> WriteRegisterResponse() =>
        HexConverter.ToBytes("00 00 00 06 01 0F 00 00 00 0A", " ");

    public static ReadOnlyMemory<byte> WriteMultipleRegistersResponse() =>
        HexConverter.ToBytes("00 00 00 06 01 10 00 00 00 0A", " ");
}
