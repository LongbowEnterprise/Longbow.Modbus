// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

namespace Longbow.Modbus;

static class MessageBuilder
{
    public static void ValidateNumberOfPoints(string argumentName, ushort numberOfPoints, ushort maxNumberOfPoints)
    {
        if (numberOfPoints < 1 || numberOfPoints > maxNumberOfPoints)
        {
            string msg = $"Argument {argumentName} must be between 1 and {maxNumberOfPoints} inclusive.";
            throw new ArgumentException(msg, argumentName);
        }
    }

    public static void ValidateData<T>(string argumentName, T[] data, int maxDataLength)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (data.Length == 0 || data.Length > maxDataLength)
        {
            string msg = $"The length of argument {argumentName} must be between 1 and {maxDataLength} inclusive.";
            throw new ArgumentException(msg, argumentName);
        }
    }
}
