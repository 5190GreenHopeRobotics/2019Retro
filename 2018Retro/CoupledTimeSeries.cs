using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace frc.team5190.diagnostics
{
    class CoupledTimeSeries
    {
        int numSeries;
        TimeSeries t1, t2, t3, t4;
        String name;
        double exceededTime = 0;
        double difference;
        double time;
        bool statusFlag = true;

        public CoupledTimeSeries(String name, TimeSeries t1, TimeSeries t2, double difference, double time)
        {
            this.numSeries = 2;
            this.name = name;
            this.t1 = t1;
            this.t2 = t2;
            this.difference = difference;
            this.time = time;
        }

        public CoupledTimeSeries(String name, TimeSeries t1, TimeSeries t2, TimeSeries t3, TimeSeries t4, double difference, double time)
        {
            this.numSeries = 4;
            this.name = name;
            this.t1 = t1;
            this.t2 = t2;
            this.t3 = t3;
            this.t4 = t4;
            this.difference = difference;
            this.time = time;
        }

        public void add(double v1, double v2)
        {
            t1.add(v1);
            t2.add(v2);

            if (v1/v2 > 1 + difference || v2/v1 > 1 + difference)
            {
                exceededTime += .02;
            }
            else
            {
                exceededTime = 0;
            }

            if (exceededTime > time)
            {
                statusFlag = false;
            }
        }

        public void add(double v1, double v2, double v3, double v4)
        {
            t1.add(v1);
            t2.add(v2);
            t3.add(v3);
            t4.add(v4);

            if (v1 / v2 > 1 + difference || v2 / v1 > 1 + difference ||
                v3 / v4 > 1 + difference || v4 / v3 > 1 + difference ||
                v1 / v3 > 1 + difference || v3 / v1 > 1 + difference)
            {
                exceededTime += .02;
            }
            else
            {
                exceededTime = 0;
            }

            if (exceededTime > time)
            {
                statusFlag = false;
            }
        }

        public bool status()
        {
            return statusFlag;
        }

        public void results()
        {
            Console.Write(name + "\t: ");

            if (numSeries == 2)
            {
                if (status() && t1.status() && t2.status())
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
            else if (numSeries == 4)
            {
                if (status() && t1.status() && t2.status() && t3.status() && t4.status())
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

            if (!status())
            {
                Console.Write(" | Difference between coupled motors");
            }

            t1.results(false);
            t2.results(false);
            
            if (numSeries == 4)
            {
                t3.results(false);
                t4.results(false);
            }

            Console.WriteLine();
            Console.ResetColor();
        }
    }
}
