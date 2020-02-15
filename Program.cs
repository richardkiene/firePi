using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Mcp23xxx;

namespace firePi
{
    public struct Relay
    {
        public Mcp23017 mcp;
        public Port port;
        public byte value;

        public Relay(Mcp23017 _mcp, Port _port, byte _value)
        {
            mcp = _mcp;
            port = _port;
            value = _value;
        }
    }

    public struct Cue
    {
        public Relay positiveRelay;
        public Relay negativeRelay;

        public Cue(Relay posRelay, Relay negRelay)
        {
            positiveRelay = posRelay;
            negativeRelay = negRelay;
        }
    }

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

            // Staticly configure each relay on board A (negative)
            Mcp23017 mcp23017_a = new Mcp23017(i2cDevice_a);
            Relay[] relayBoard_a = new Relay[16];
            relayBoard_a[0] = new Relay(mcp23017_a, Port.PortA, 0xfe);
            relayBoard_a[1] = new Relay(mcp23017_a, Port.PortA, 0xfd);
            relayBoard_a[2] = new Relay(mcp23017_a, Port.PortA, 0xfb);
            relayBoard_a[3] = new Relay(mcp23017_a, Port.PortA, 0xf7);
            relayBoard_a[4] = new Relay(mcp23017_a, Port.PortA, 0xef);
            relayBoard_a[5] = new Relay(mcp23017_a, Port.PortA, 0xdf);
            relayBoard_a[6] = new Relay(mcp23017_a, Port.PortA, 0xbf);
            relayBoard_a[7] = new Relay(mcp23017_a, Port.PortA, 0x7f);
            relayBoard_a[8] = new Relay(mcp23017_a, Port.PortB, 0xfe);
            relayBoard_a[9] = new Relay(mcp23017_a, Port.PortB, 0xfd);
            relayBoard_a[10] = new Relay(mcp23017_a, Port.PortB, 0xfb);
            relayBoard_a[11] = new Relay(mcp23017_a, Port.PortB, 0xf7);
            relayBoard_a[12] = new Relay(mcp23017_a, Port.PortB, 0xef);
            relayBoard_a[13] = new Relay(mcp23017_a, Port.PortB, 0xdf);
            relayBoard_a[14] = new Relay(mcp23017_a, Port.PortB, 0xbf);
            relayBoard_a[15] = new Relay(mcp23017_a, Port.PortB, 0x7f);

            // Staticly configure each relay on board B (positive)
            Mcp23017 mcp23017_b = new Mcp23017(i2cDevice_b);
            Relay[] relayBoard_b = new Relay[16];
            relayBoard_b[0] = new Relay(mcp23017_b, Port.PortA, 0xfe);
            relayBoard_b[1] = new Relay(mcp23017_b, Port.PortA, 0xfd);
            relayBoard_b[2] = new Relay(mcp23017_b, Port.PortA, 0xfb);
            relayBoard_b[3] = new Relay(mcp23017_b, Port.PortA, 0xf7);
            relayBoard_b[4] = new Relay(mcp23017_b, Port.PortA, 0xef);
            relayBoard_b[5] = new Relay(mcp23017_b, Port.PortA, 0xdf);
            relayBoard_b[6] = new Relay(mcp23017_b, Port.PortA, 0xbf);
            relayBoard_b[7] = new Relay(mcp23017_b, Port.PortA, 0x7f);
            relayBoard_b[8] = new Relay(mcp23017_b, Port.PortB, 0xfe);
            relayBoard_b[9] = new Relay(mcp23017_b, Port.PortB, 0xfd);
            relayBoard_b[10] = new Relay(mcp23017_b, Port.PortB, 0xfb);
            relayBoard_b[11] = new Relay(mcp23017_b, Port.PortB, 0xf7);
            relayBoard_b[12] = new Relay(mcp23017_b, Port.PortB, 0xef);
            relayBoard_b[13] = new Relay(mcp23017_b, Port.PortB, 0xdf);
            relayBoard_b[14] = new Relay(mcp23017_b, Port.PortB, 0xbf);
            relayBoard_b[15] = new Relay(mcp23017_b, Port.PortB, 0x7f);

            // Populate all cues
            int cueCount = relayBoard_a.Length * relayBoard_b.Length;
            Cue[] cues = new Cue[cueCount];
            int currentCue = 0;
            for (int i=0; i<relayBoard_b.Length; i++) {
                for (int j=0; j<relayBoard_a.Length; j++){
                    cues[currentCue++] = new Cue(relayBoard_b[i], relayBoard_a[j]);
                }
            }

            // Fire all cues
            for (int i=0; i<cues.Length; i++) {
                    WriteByteWithDelay(cues[i], 1000);
                    Console.WriteLine("Firing cue: {0:d}", i);
            }
        }

        private static void WriteByteWithDelay(Cue cue, int delay)
        {
            Register register = Register.IODIR;

            cue.positiveRelay.mcp.WriteByte(register, cue.positiveRelay.value, cue.positiveRelay.port);
            cue.negativeRelay.mcp.WriteByte(register, cue.negativeRelay.value, cue.negativeRelay.port);

            Thread.Sleep(delay);

            cue.positiveRelay.mcp.WriteByte(register, 0xff, cue.positiveRelay.port);
            cue.negativeRelay.mcp.WriteByte(register, 0xff, cue.negativeRelay.port);
        }
    }
}