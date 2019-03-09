using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace frc.team5190.diagnostics
{
    class LogReader
    {    
        Int32 Version;
        DateTime StartTime;
        TimeSeries packetLossTS = new TimeSeries("Packet Loss", 2, 2.0, false);
        TimeSeries intakeLeftTS = new TimeSeries("Intake (Left)", 25, 2.0, true);
        TimeSeries intakeRightTS = new TimeSeries("Intake (Right)", 25, 2.0, true);
        TimeSeries armTS = new TimeSeries("Arm        ", 25, 2.0, true);
        CoupledTimeSeries leftDriveTS = new CoupledTimeSeries("Drive (Left)", new TimeSeries("Left drive", 60, 1.0, false), new TimeSeries("Left drive", 60, 1.0, false), 0.3, 1.0);
        CoupledTimeSeries rightDriveTS = new CoupledTimeSeries("Drive (Right)", new TimeSeries("Right drive", 60, 1.0, false), new TimeSeries("Right drive", 60, 1.0, false), 0.3, 1.0);
        CoupledTimeSeries elevatorTS = new CoupledTimeSeries("Elevator", new TimeSeries("Elevator", 25, 0.5, false), new TimeSeries("Elevator", 25, 0.5, false), new TimeSeries("Elevator", 25, 0.5, false), new TimeSeries("Elevator", 25, 0.5, false), 0.2, 0.5);
        TimeSeries climbForwardTS = new TimeSeries("Climb (Fwd) ", 30, 2.0, true);
        CoupledTimeSeries climbRearTS = new CoupledTimeSeries("Climb (Rear)", new TimeSeries("Climb", 40, 0.5, false), new TimeSeries("Climb", 40, 0.5, false), 0.2, 0.5);
        CoupledTimeSeries climbFrontTS = new CoupledTimeSeries("Climb (Front)", new TimeSeries("Climb", 40, 2.0, false), new TimeSeries("Climb", 40, 2.0, false), 0.2, 0.5);

        bool brownout = false;

        public LogReader(string path)
        {
            if (File.Exists(path))
            {
                using (BinaryReader2 reader = new BinaryReader2(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    Version = reader.ReadInt32();
                    if (Version == 3)
                    {
                        StartTime = FromLVTime(reader.ReadInt64(), reader.ReadUInt64());
                        int i = 0;
                        while (reader.BaseStream.Position != reader.BaseStream.Length)
                        {
                            readEntry(reader, i++);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid file: " + path);
                    }
                }
            }
        }

        public void results()
        {
            packetLossTS.results();
            booleanResult("Battery   ", brownout, "Brownout");
            leftDriveTS.results();
            rightDriveTS.results();
            elevatorTS.results();
            armTS.results();
            intakeLeftTS.results();
            intakeRightTS.results();
            climbFrontTS.results();
            climbRearTS.results();
            climbForwardTS.results();
        }

        public void readEntry(BinaryReader2 reader, int counter) 
        {
            double trip = TripTimeToDouble(reader.ReadByte());
            double packetLoss = PacketLossToDouble(reader.ReadSByte());
            double voltage = VoltageToDouble(reader.ReadUInt16());
            double cpu = RoboRioCPUToDouble(reader.ReadByte());
            bool[] statusFlags = StatusFlagsToBooleanArray(reader.ReadByte());
            brownout = brownout || statusFlags[0];
            bool watchdog = statusFlags[1];
            bool dsTele = statusFlags[2];
            bool dsAuto = statusFlags[3];
            bool dsDisabled = statusFlags[4];
            bool robotTele = statusFlags[5];
            bool robotAuto = statusFlags[6];
            bool robotDisabled = statusFlags[7];
            double can = CANUtilToDouble(reader.ReadByte());
            double wifi = WifidBToDouble(reader.ReadByte());
            double bandwidth = BandwidthToDouble(reader.ReadUInt16());
            int pdpId = reader.ReadByte();
            double[] pdpV = PDPValuesToArrayList(reader.ReadBytes(21));
            double pdpRes = reader.ReadByte();
            double pdpVol = reader.ReadByte();
            double pdpTemp = reader.ReadByte();
            DateTime time = StartTime.AddMilliseconds(20 * counter);

            packetLossTS.add(packetLoss);
            leftDriveTS.add(pdpV[2], pdpV[3]);
            rightDriveTS.add(pdpV[0], pdpV[1]);
            intakeLeftTS.add(pdpV[4]);
            intakeRightTS.add(pdpV[5]);
            armTS.add(pdpV[6]);
            elevatorTS.add(pdpV[8], pdpV[9], pdpV[10], pdpV[11]);
            climbForwardTS.add(pdpV[7]);
            climbRearTS.add(pdpV[12], pdpV[13]);
            climbFrontTS.add(pdpV[14], pdpV[15]);
        }

        protected DateTime FromLVTime(long unixTime, UInt64 ummm)
        {
            var epoch = new DateTime(1904, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            epoch = epoch.AddSeconds(unixTime);
            epoch = TimeZoneInfo.ConvertTimeFromUtc(epoch, TimeZoneInfo.Local);

            return epoch.AddSeconds(((double)ummm / UInt64.MaxValue));
        }

        protected void booleanResult(string name, bool result, string error)
        {
            Console.Write(name + "\t:");
            if (result)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(" OK");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(" NOT OK | " + error);
            }

            Console.WriteLine();
            Console.ResetColor();
        }

        //Import methods
        #region
        protected double TripTimeToDouble(byte b)
        {
            return (double)b * 0.5d;
        }

        protected double PacketLossToDouble(sbyte b)
        {
            return (double)(b * 4) * .01;
        }

        protected double VoltageToDouble(UInt16 i)
        {
            return (double)i * .00390625d;
        }

        protected double RoboRioCPUToDouble(byte b)
        {
            return ((double)b * 0.5d) * .01d;
        }

        protected bool[] StatusFlagsToBooleanArray(byte b)
        {
            byte[] bytes = { b };
            return bytes.SelectMany(GetBits).ToArray();
        }

        protected double CANUtilToDouble(byte b)
        {
            return ((double)b * 0.5d) * .01d;
        }

        protected double WifidBToDouble(byte b)
        {
            return ((double)b * 0.5d) * .01d;
        }

        protected double BandwidthToDouble(UInt16 i)
        {
            return (double)i * .00390625d;
        }
        protected double[] PDPValuesToArrayList(byte[] ba)
        {
            double[] d = new double[16];
            for (int s = 0; s < 5; s++)
            {
                if (s % 2 == 0)
                {
                    byte[] b5 = new byte[5];
                    Array.Copy(ba, s * 4, b5, 0, 5);
                    for (int n = 0; n < 4; ++n)
                    {
                        if (n == 0)
                        {
                            d[(s * 3) + n] = (double)(Convert.ToUInt16(b5[0] << 2) + Convert.ToUInt16(b5[1] >> 6)) * .125d;
                        }
                        else
                        {
                            d[(s * 3) + n] = (double)(Convert.ToUInt16(((UInt16)((byte)(b5[n] << (n * 2)))) << 2) + Convert.ToUInt16(b5[n + 1] >> (6 - (n * 2)))) * .125d;
                        }
                    }
                }
                else
                {
                    byte[] b3 = new byte[3];
                    Array.Copy(ba, (s * 4) + 1, b3, 0, 3);
                    for (int n = 0; n < 2; ++n)
                    {
                        if (n == 0)
                        {
                            d[((s * 3) + 1) + n] = (double)(Convert.ToUInt16(b3[0] << 2) + Convert.ToUInt16(b3[1] >> 6)) * .125d;
                        }
                        else
                        {
                            d[((s * 3) + 1) + n] = (double)(Convert.ToUInt16(((UInt16)((byte)(b3[1] << 2))) << 2) + Convert.ToUInt16(b3[2] >> 4)) * .125d;
                        }
                    }
                }
            }



            return d;
        }

        protected IEnumerable<bool> GetBits(byte b)
        {
            for (int i = 0; i < 8; i++)
            {
                yield return !((b & 0x80) != 0);
                b *= 2;
            }
        }

        #endregion
    }
}
