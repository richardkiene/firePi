using System;
using System.Collections;
using System.Collections.Generic;
using System.Device.I2c;
using System.IO;
using System.Text.Json;
using System.Threading;

using CommandLine;
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

        public Cue(Relay _positiveRelay, Relay _negativeRelay)
        {
            positiveRelay = _positiveRelay;
            negativeRelay = _negativeRelay;
        }
    }

    public class FiringSequence
    {
        public Instruction[] instructions { get; set;}
    }

    public class Instruction
    {
        public int[] CueNumbers { get; set; }
        public int Delay { get; set; }
        public int Duration { get; set; }
    }

    class Program
    {
        private static readonly int s_deviceAddress_a = 0x20;
        private static readonly int s_deviceAddress_b = 0x22;

        private static bool s_verbose = false;

        public class Options
        {
            [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
            public bool Verbose { get; set; }

            [Option('b', "build", Required = false, HelpText = "Build a firing order json file")]
            public bool Build{ get; set; }

            [Option('o', "output", Required = false, HelpText ="File to output configuration to")]
            public string OutputFile { get; set; }

            [Option('i', "interactive", Required = false, HelpText = "Interactively trigger cues")]
            public bool Interactive { get; set; }

            [Option('f', "file", Required = false, HelpText = "Json firing order")]
            public string File { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    s_verbose = o.Verbose;
                    if (String.IsNullOrEmpty(o.OutputFile) && !String.IsNullOrEmpty(o.File) && File.Exists(o.File) && !o.Interactive && !o.Build)
                    {
                        string jsonString = File.ReadAllText(o.File);
                        FiringSequence firingSequence = JsonSerializer.Deserialize<FiringSequence>(jsonString);
                        RunFiringSequence(firingSequence);
                    }
                    else if (o.Interactive && !o.Build && String.IsNullOrEmpty(o.File) && String.IsNullOrEmpty(o.OutputFile))
                    {
                        while(true)
                        {
                            Console.WriteLine("Enter cue to fire: ");
                            try
                            {
                                int cueNum = Convert.ToInt32(Console.ReadLine());
                                if (cueNum > 255)
                                {
                                    throw new Exception();
                                }

                                FiringSequence firingSequence = new FiringSequence();
                                firingSequence.instructions = new Instruction[1];
                                firingSequence.instructions[0] = new Instruction();
                                firingSequence.instructions[0].CueNumbers = new int[1];
                                firingSequence.instructions[0].CueNumbers[0] = cueNum;
                                firingSequence.instructions[0].Delay = 1000;
                                firingSequence.instructions[0].Duration = 1000;
                                RunFiringSequence(firingSequence);
                            }
                            catch
                            {
                                Console.WriteLine("Cue must be a integer between 0 and 255");
                            }
                        }
                    }
                    else if (o.Build && !o.Interactive && !String.IsNullOrEmpty(o.OutputFile) && String.IsNullOrEmpty(o.File))
                    {
                        List<Instruction> instructions = new List<Instruction>();

                        for (int i=0; i < 255; i++)
                        {
                            Console.WriteLine("Type add for a new cue or type exit:";
                            try
                            {
                                string command = Console.ReadLine();
                                if (command.Equals("exit"))
                                {
                                    break;
                                }
                                else if (command.Equals("add"))
                                {
                                    Instruction instruction = new Instruction();
                                    instruction.CueNumbers = new int[] { i };
                                    Console.WriteLine("Enter cue delay:");
                                    instruction.Delay = Convert.ToInt32(Console.ReadLine());
                                    Console.WriteLine("Enter cue duration:");
                                    instruction.Duration = Convert.ToInt32(Console.ReadLine());
                                }
                            }
                            catch
                            {

                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Yeah that does not work yet or you were dumb");
                    }
                });
        }

        private static void RunFiringSequence(FiringSequence firingSequence)
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

            // Populate cues
            int cueCount = relayBoard_a.Length * relayBoard_b.Length;
            Cue[] cues = new Cue[cueCount];
            int currentCue = 0;
            for (int i=0; i<relayBoard_b.Length; i++)
            {
                for (int j=0; j<relayBoard_a.Length; j++)
                {
                    cues[currentCue++] = new Cue(relayBoard_b[i], relayBoard_a[j]);
                }
            }

            // Fire instructions
            for (int i=0; i<firingSequence.instructions.Length; i++)
            {
                FireCues(cues, firingSequence.instructions[i]);
            }
        }

        private static void FireCues(Cue[] cues, Instruction instruction)
        {
            Register register = Register.IODIR;

            for (int i=0; i<instruction.CueNumbers.Length; i++)
            {
                Cue cue = cues[instruction.CueNumbers[i]];
                cue.positiveRelay.mcp.WriteByte(register, cue.positiveRelay.value, cue.positiveRelay.port);
                cue.negativeRelay.mcp.WriteByte(register, cue.negativeRelay.value, cue.negativeRelay.port);

                if (s_verbose)
                {
                    Console.WriteLine("Firing cue: {0:d}", instruction.CueNumbers[i]);
                }
            }

            Thread.Sleep(instruction.Duration);

            for (int i=0; i<instruction.CueNumbers.Length; i++)
            {
                Cue cue = cues[instruction.CueNumbers[i]];
                cue.positiveRelay.mcp.WriteByte(register, 0xff, cue.positiveRelay.port);
                cue.negativeRelay.mcp.WriteByte(register, 0xff, cue.negativeRelay.port);
            }

            Thread.Sleep(instruction.Delay);
        }
    }
}