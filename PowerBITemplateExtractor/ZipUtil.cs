using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerBITemplateExtractor
{
    public class ZipUtil
    {
        public static void CreateArchive(string fileName, DirectoryInfo destinationPath)
        {
            string oldCurrentDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(destinationPath.FullName);
            lauch7zip(string.Format(@"a -tZip ..\{0} * -mx9", fileName));
            Directory.SetCurrentDirectory(oldCurrentDirectory);
        }

        public static void ExtractArchive(string destinationPath, string fileName)
        {
            if (Directory.Exists(destinationPath))
                Directory.Delete(destinationPath, recursive: true);
            Directory.CreateDirectory(destinationPath);
            lauch7zip(string.Format("x {0} -o{1}", fileName, destinationPath));
        }


        /// <summary>
        /// Launch the legacy application with some options set.
        /// </summary>
        private static void lauch7zip(string arguments)
        {
            // For the example

            // Use ProcessStartInfo class
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "7za.exe";
            startInfo.Arguments = arguments;
            Console.WriteLine("7za.exe " + arguments);

            // Start the process with the info we specified.
            // Call WaitForExit and then the using statement will close.
            using (Process exeProcess = Process.Start(startInfo))
            {
                exeProcess.WaitForExit();
            }
        }
    }
}
