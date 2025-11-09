// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

namespace UnitTest;

public class MockRtuResponse
{
    public static ReadOnlyMemory<byte> ReadCoilResponse() =>
        HexConverter.ToBytes("01 01 02 FD 02 78 AD", " ");

    public static ReadOnlyMemory<byte> ReadInputsResponse() =>
        HexConverter.ToBytes("01 02 02 00 00 B9 B8", " ");

    public static ReadOnlyMemory<byte> ReadHoldingRegistersResponse() =>
        HexConverter.ToBytes("01 03 14 00 0C 00 00 00 17 00 00 00 2E 00 00 00 01 00 02 00 04 00 05 90 D2", " ");

    public static ReadOnlyMemory<byte> ReadInputRegistersResponse() =>
        HexConverter.ToBytes("01 04 14 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 95 81", " ");

    public static ReadOnlyMemory<byte> WriteCoilResponse(ReadOnlyMemory<byte> request)
    {
        var v = request.Span[4] == 0xFF ? "01 05 00 00 FF 00 8C 3A" : "01 05 00 01 00 00 9C 0A";
        return HexConverter.ToBytes(v, " ");
    }

    public static ReadOnlyMemory<byte> WriteMultipleCoilsResponse() =>
        HexConverter.ToBytes("01 06 00 00 00 0C 89 CF", " ");

    public static ReadOnlyMemory<byte> WriteRegisterResponse() =>
        HexConverter.ToBytes("01 0F 00 00 00 0A D5 CC", " ");

    public static ReadOnlyMemory<byte> WriteMultipleRegistersResponse() =>
        HexConverter.ToBytes("01 10 00 00 00 0A 40 0E", " ");
}
