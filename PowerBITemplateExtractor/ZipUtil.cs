using System;
using System.Diagnostics;
using System.IO;

namespace PowerBITemplateExtractor
{
    public class ZipUtil
    {
        public static void CreateArchive(string sourcePath, string fileName)
        {
            string oldCurrentDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(sourcePath);
            lauch7zip(string.Format(@"a -tZip ..\{0} * -mx9", fileName));
            Directory.SetCurrentDirectory(oldCurrentDirectory);
        }

        public static void ExtractArchive(string destinationPath, string fileName)
        {
            Directory.CreateDirectory(destinationPath);
            lauch7zip(string.Format("x {0} -o{1} -y", fileName, destinationPath));
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
