using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace frc.team5190.diagnostics
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            string frcFolder = "C:\\users\\Public\\Documents\\FRC\\Log Files\\";
            int numFiles = args.Length >= 1 ? Convert.ToInt32(args[0]) : 1;
            int fileSize = args.Length >= 2 && args[1] == "all" ? 0 : 200000;
            DirectoryInfo dir = new DirectoryInfo(frcFolder);
            var files = dir.GetFiles("*.dslog").OrderByDescending(p => p.CreationTime).Where(p => p.Length > fileSize).Take(numFiles).AsEnumerable();
            foreach (var file in files)
            {
                Console.WriteLine("Match " + file.Name.Substring(0, file.Name.LastIndexOf('.')));
                Console.WriteLine("-----------------------------");
                new EventReader(file.FullName.Substring(0, file.FullName.LastIndexOf('.')) + ".dsevents", -1).results();
                new LogReader(file.FullName).results();
                Console.WriteLine();
            }           
        }
    }
}
