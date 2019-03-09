using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace frc.team5190.diagnostics
{
    class TimeSeries
    {
        string name;
        double maxThreshold;
        double maxThresholdTime;
        double exceededTime = 0;
        bool checkForDisconnect;
        bool[] statusFlags = { true, false };
        //List<double> values = new List<double>();

        public TimeSeries(string name, double maxThreshold, double maxThreholdTime, bool checkForDisconnect)
        {
            this.name = name;
            this.maxThreshold = maxThreshold;
            this.maxThresholdTime = maxThreholdTime;
            this.checkForDisconnect = checkForDisconnect;
        }

        public void add(double value)
        {
            //values.Add(value);
            if (value >= maxThreshold)
            {
                exceededTime += .020;
            }
            else
            {
                exceededTime = 0;
            }

            if (exceededTime > maxThresholdTime)
            {
                statusFlags[0] = false;
            }

            if (checkForDisconnect && (value > 0))
            {
                statusFlags[1] = true;
            }
        }

        public bool status()
        {
            return statusFlags[0] && (!checkForDisconnect || (checkForDisconnect && statusFlags[1]));
        }

        public void results(bool header = true)
        {
            if (header)
            {
                Console.Write(name + "\t: ");
                if (status())
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("OK");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("NOT OK");
                }
            }

            if (!statusFlags[0])
            {
                Console.Write(" | Exceeded threshold of " + maxThreshold + "amps for " + maxThresholdTime + "s");
            }

            if (checkForDisconnect && !statusFlags[1])
            {
                Console.Write(" | Disconnected");
            }

            if (header)
            {
                Console.WriteLine();
                Console.ResetColor();
            }
        }
    }
}
