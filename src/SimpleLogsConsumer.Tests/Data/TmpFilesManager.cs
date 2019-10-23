using System;
using System.IO;

namespace SimpleLogsConsumer.Tests.Data
{
    internal class TmpFilesManager
    {
        private const string tmpDir = "tmp";
        private static string GetTmpFile
            => $"logfile_{Guid.NewGuid().ToString()}.txt";

        public static string GetTmpFilePath()
        {
            var dir = Path.Combine(Environment.CurrentDirectory, tmpDir);
        
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        
            return Path.Combine(dir, GetTmpFile);
        }

        public static void DropDb(string name)
            => Delete(Path.Combine(Environment.CurrentDirectory, name));

        public static void Delete(string filePath) => File.Delete(filePath);
    }
}
