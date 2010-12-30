using System;
using System.Collections.Generic;
using System.Text;

namespace ReportGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                Console.WriteLine("ERROR: You must pass the required parameter to generate the required data.");
                Console.WriteLine("Accepted Paramters: a - all, c - computer information, h - hardware, s - software");
                Console.WriteLine("Get All: ReportGenerator a");
                Console.WriteLine("Get Computer Info: ReportGenerator c");
                Console.WriteLine("Get Hardware Info: ReportGenerator h");
                Console.WriteLine("Get Network Info: ReportGenerator n");
                Console.WriteLine("Get Software Information: ReportGenerator s");

                Environment.Exit(0);
            }
            else
            {
                string cmd = args[0];

                if(cmd.Equals("a"))
                {
                    DBReport.ComputerInformation();
                    DBReport.HardwareInformation();
                    DBReport.SoftwareInformation();
                }
                else if(cmd.Equals("c"))
                {
                    DBReport.ComputerInformation();
                }
                else if(cmd.Equals("h"))
                {
                    DBReport.HardwareInformation();
                }
                else if(cmd.Equals("s"))
                {
                    DBReport.SoftwareInformation();
                }
                else if(cmd.Equals("n"))
                {
                    DBReport.NetworkInformation();
                }
                else
                {
                    Console.WriteLine("Parameter passed not appropriate, try again.");
                }
            }

            Console.WriteLine("Done.");
        }
    }
}
