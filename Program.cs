using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Mcp23xxx;

namespace firePi
{
    class Program
    {
        private static readonly int s_deviceAddress_a = 0x20;
        private static readonly int s_deviceAddress_b = 0x22;

        static void Main(string[] args)
        {
            var i2cConnectionSettings_a = new I2cConnectionSettings(1, s_deviceAddress_a);
            var i2cConnectionSettings_b = new I2cConnectionSettings(1, s_deviceAddress_b);
            var i2cDevice_a = I2cDevice.Create(i2cConnectionSettings_a);
            var i2cDevice_b = I2cDevice.Create(i2cConnectionSettings_b);

            Mcp23017 mcp23017_a = new Mcp23017(i2cDevice_a);
            Mcp23017 mcp23017_b = new Mcp23017(i2cDevice_b);

            WriteByte(mcp23017_a);
            WriteByte(mcp23017_b);
        }

        private static void WriteByte(Mcp23017 mcp)
        {
            Console.WriteLine("Write Individual Byte");

            Register register = Register.IODIR;

            byte dataRead = mcp.ReadByte(register, Port.PortB);
            Console.WriteLine($"\tIODIRB: 0x{dataRead:X2}");

            mcp.WriteByte(register, 0xfe, Port.PortB);

            dataRead = mcp.ReadByte(register, Port.PortB);
            Console.WriteLine($"\tIODIRB: 0x{dataRead:X2}");

            mcp.WriteByte(register, 0xff, Port.PortB);

            dataRead = mcp.ReadByte(register, Port.PortB);
            Console.WriteLine($"\tIODIRB: 0x{dataRead:X2}");

            dataRead = mcp.ReadByte(register, Port.PortA);
            Console.WriteLine($"\tIODIRB: 0x{dataRead:X2}");

            mcp.WriteByte(register, 0xfe, Port.PortA);

            dataRead = mcp.ReadByte(register, Port.PortA);
            Console.WriteLine($"\tIODIRB: 0x{dataRead:X2}");

            mcp.WriteByte(register, 0xff, Port.PortA);

            dataRead = mcp.ReadByte(register, Port.PortA);
            Console.WriteLine($"\tIODIRB: 0x{dataRead:X2}");
        }
    }
}