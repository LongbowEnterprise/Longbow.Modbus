// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://github.com/LongbowExtensions/

using System.IO.Ports;

namespace Longbow.Modbus;

/// <summary>
/// ModbusRtuClientOptions 配置类
/// </summary>
public class ModbusRtuClientOptions
{
    /// <summary>
    /// Gets or sets the name of the serial port to use for communication.
    /// </summary>
    /// <remarks>The port name typically follows the format "COM1", "COM2", etc. Ensure that the specified
    /// port exists and is available on the system before attempting to use it.</remarks>
    public string PortName { get; set; } = "COM1";

    /// <summary>
    /// Gets or sets the baud rate for serial communication.
    /// </summary>
    /// <remarks>The baud rate determines the speed at which data is transmitted over the serial port. Common values
    /// include 9600, 19200, and 115200. Ensure that the baud rate matches the settings of the connected device to avoid
    /// communication errors.</remarks>
    public int BaudRate { get; set; } = 9600;

    /// <summary>
    /// Gets or sets the number of data bits per character for the serial communication.
    /// </summary>
    /// <remarks>Typical values are 7 or 8, depending on the protocol and device requirements. Ensure that the
    /// value matches the configuration of the connected device to avoid communication errors.</remarks>
    public int DataBits { get; set; } = 8;

    /// <summary>
    /// Gets or sets the parity scheme used for serial communication.
    /// </summary>
    /// <remarks>Set this property to specify whether and how parity checking is performed on transmitted and
    /// received data. The default value is <see cref="Parity.None"/>, indicating that no parity checking is
    /// used.</remarks>
    public Parity Parity { get; set; } = Parity.None;

    /// <summary>
    /// Gets or sets the number of stop bits used for serial communication.
    /// </summary>
    /// <remarks>The value of this property determines how the end of a byte is signaled during transmission.
    /// The default is <see cref="StopBits.One"/>. Ensure that the stop bits setting matches the requirements of the
    /// connected serial device.</remarks>
    public StopBits StopBits { get; set; } = StopBits.One;

    /// <summary>
    /// Gets or sets the handshake configuration used for establishing a connection.
    /// </summary>
    public Handshake Handshake { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the Data Terminal Ready (DTR) signal is enabled for the serial port
    /// connection.
    /// </summary>
    /// <remarks>Enabling the DTR signal is commonly used to indicate that the device is ready to communicate.
    /// The effect of setting this property depends on the connected hardware and may be required for certain devices to
    /// establish communication.</remarks>
    public bool DtrEnable { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether Request to Send (RTS) hardware flow control is enabled for the serial
    /// port.
    /// </summary>
    /// <remarks>When enabled, RTS flow control allows the serial port to signal readiness to transmit data
    /// using the RTS line. This setting is typically used to manage data flow between devices and prevent buffer
    /// overruns. The effect of this property depends on the capabilities of the underlying hardware and
    /// driver.</remarks>
    public bool RtsEnable { get; set; }

    /// <summary>
    /// Gets a value indicating whether null values should be discarded during processing.
    /// </summary>
    public bool DiscardNull { get; set; }

    /// <summary>
    /// Gets the size, in bytes, of the buffer used for reading data from the underlying stream. Default value is 4096
    /// </summary>
    public int ReadBufferSize { get; set; } = 4096;

    /// <summary>
    /// Gets the size, in bytes, of the buffer used for write operations. Default value is 2048
    /// </summary>
    public int WriteBufferSize { get; set; } = 2048;
}
