using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GwinstekLCRTester
{
    class FileHandler
    {
        private static string TestFilesPath = Directory.GetCurrentDirectory() + "/test_files_path.txt";

        public static string CreateDefaultPath()
        {
            WriteNewPathToFile(Directory.GetCurrentDirectory());
            return Directory.GetCurrentDirectory(); // Return the default path
        }

        private static void CreateTestFilesPathFile()
        {
            File.Create(TestFilesPath);
        }

        public static string ReadTestFilesPath()
        {
            if (!File.Exists(TestFilesPath))
            {
                CreateTestFilesPathFile();
            }

            string path = File.ReadAllText(TestFilesPath);
            return path;
        }

        public static void WriteNewPathToFile(string newPath)
        {
            StreamWriter sw = File.CreateText(TestFilesPath);
            sw.WriteLine(newPath);
            sw.Close();
        }
    }
}
