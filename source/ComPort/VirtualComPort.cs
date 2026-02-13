using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace ComPort
{
    class VirtualComPort

    // delivers the calculations to the LED hardware through the USB cable

    {
        private readonly SerialPort _vcp;
            
        private string _portName;
        private int _baudRate;
        private int _dataBits;
        private StopBits _stopBits;
        private Parity _parity;
        private Handshake _handshake;

        public VirtualComPort()
        {
            _vcp = new SerialPort();

            // Default values loaded from config
            SerialPortName = "COM3";
            SerialBaudRate = 9600;
            SerialDataBits = 8;
            SerialStopBits = StopBits.One;
            SerialParity = Parity.None;
            SerialHandshake = Handshake.None;

            UpdateParameters();

        }
        public string SerialPortName
        {
            get => _portName; set
            {
                _portName = value;
            }
        }
        public int SerialBaudRate
        {
            get => _baudRate; set
            {
                _baudRate = value;
            }
        }
        public int SerialDataBits
        {
            get => _dataBits; set
            {
                _dataBits = value;
            }
        }
        public StopBits SerialStopBits
        {
            get => _stopBits; set
            {
                _stopBits = value;
            }
        }
        public Parity SerialParity
        {
            get => _parity; set
            {
                _parity = value;
            }
        }
        public Handshake SerialHandshake
        {
            get => _handshake; set
            {
                _handshake = value;
            }
        }
        public void UpdateParameters()
        {
            // The parameters can't be modified if the Serial port is open
            if (_vcp.IsOpen)
            {
                _vcp.Close();
            }

            if (!_vcp.IsOpen)
            {
                _vcp.BaudRate = SerialBaudRate;
                _vcp.StopBits = SerialStopBits;
                _vcp.PortName = SerialPortName;
                _vcp.Parity = SerialParity;
                _vcp.DataBits = SerialDataBits;
                _vcp.Handshake = SerialHandshake;
            }
        }

        public void SendLedIntensity(int Led, int intensity)
        {
            // Check the Led is valid
            if (Led > 19 || Led < 0)
            {
                return;
            }

            // Check the intensity is valid
            if (intensity < 0 || intensity > 65535)
            {
                return;
            }

            if (TryOpen())
            {
                string json = "{\"LED\":{\"LedIndex\":" + Led + ",\"Duty\":" + intensity + "}}";

                _vcp.Write(json);
            }
        }

        public void SendJsonContent(string content)
        {

            if (TryOpen())
            {
                _vcp.Write(content);
            }
        }

        public bool TryOpen()
        {

            if (!_vcp.IsOpen)
            {
                try
                {
                    _vcp.Open();
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return true;
        }
        public int read(char[] buff, int offset , int count)
        {
            return _vcp.Read(buff, offset, count);
        }
    }
}
