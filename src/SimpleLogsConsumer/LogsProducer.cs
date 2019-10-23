using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SimpleLogsConsumer
{
    public class LogsProducer
    {
        private readonly string _filePath;

        public LogsProducer(string filePath)
        {
            _filePath = filePath;
        }

        public async Task ReadFileAndProduceEvents(BlockingCollection<LoggedEvent> queue)
        {
            var producedCount = 0;

            using StreamReader sr = new StreamReader(_filePath);
            string line;
            while ((line = await sr.ReadLineAsync()) != null)
            {
                var logged = JsonConvert.DeserializeObject<LoggedEvent>(line);
                queue.Add(logged);

                producedCount += 1;

                if (producedCount % 1000 == 0)
                    Console.WriteLine($"Imported lines: {producedCount / 1000} K");
            }
            queue.CompleteAdding();
        }
    }
}
