// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

namespace UnitTest;

static class MockTcpResponse
{
    public static ReadOnlyMemory<byte> ReadCoilResponse(ReadOnlyMemory<byte> request) =>
        GenerateResponse(request, "00 00 00 05 01 01 02 05 00");

    public static ReadOnlyMemory<byte> ReadInputsResponse(ReadOnlyMemory<byte> request) =>
        GenerateResponse(request, "00 00 00 05 01 02 02 00 00");

    public static ReadOnlyMemory<byte> ReadHoldingRegistersResponse(ReadOnlyMemory<byte> request) =>
        GenerateResponse(request, "00 00 00 17 01 03 14 00 0C 00 00 00 17 00 00 00 2E 00 00 00 01 00 02 00 04 00 05");

    public static ReadOnlyMemory<byte> ReadInputRegistersResponse(ReadOnlyMemory<byte> request) =>
        GenerateResponse(request, "00 00 00 17 01 04 14 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");

    public static ReadOnlyMemory<byte> ReadInputRegistersErrorResponse(ReadOnlyMemory<byte> request) =>
        GenerateResponse(request, "00 00 00 17 01 03 14 00 0C 00 00 00 17 00 00 00 2E 00 00 00 01 00 02 00 04");

    public static ReadOnlyMemory<byte> WriteCoilResponse(ReadOnlyMemory<byte> request)
    {
        var v = request.Span[10] == 0xFF ? "FF" : "00";
        return GenerateResponse(request, $"00 00 00 06 01 05 00 00 {v} 00");
    }

    public static ReadOnlyMemory<byte> WriteMultipleCoilsResponse(ReadOnlyMemory<byte> request) =>
        GenerateResponse(request, "00 00 00 06 01 06 00 00 00 0C");

    public static ReadOnlyMemory<byte> WriteRegisterResponse(ReadOnlyMemory<byte> request) =>
        GenerateResponse(request, "00 00 00 06 01 0F 00 00 00 0A");

    public static ReadOnlyMemory<byte> WriteMultipleRegistersResponse(ReadOnlyMemory<byte> request) =>
        GenerateResponse(request, "00 00 00 06 01 10 00 00 00 0A");

    private static ReadOnlyMemory<byte> GenerateResponse(ReadOnlyMemory<byte> request, string data)
    {
        var buffer = HexConverter.ToBytes(data, " ");

        var response = new byte[buffer.Length + 2];
        response[0] = request.Span[0];
        response[1] = request.Span[1];
        buffer.CopyTo(response.AsSpan(2));

        return response;
    }
}
