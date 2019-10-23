using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleLogsConsumer.Tests.Data
{
    internal class LogsGenerator
    {
        private const int EventsChunkSize = 1000;
        private const int MaxEventDurationInMillis = 100;

        private static readonly object _lock = new object();

        private readonly Random _random = new Random();        
        private readonly string _filePath;        
        private readonly Func<long> _time;

        public string Host { get; }

        public LogsGenerator(string filePath)
        {
            _filePath = filePath;
            _time = () => Millis(DateTime.Now);
            Host = Guid.NewGuid().ToString();
        }     

        public LogsGenerator(string host, string logsFilePath, Func<DateTime> now)
        {
            _filePath = logsFilePath;
            _time = () => Millis(now());
            Host = host;
        }

        public async Task GenerateLargeFile(int eventsCount)
        {
            int page = 0, total = eventsCount / EventsChunkSize;
            while (eventsCount > 0)
            {
                if (page++ % 10 == 1 || eventsCount <= EventsChunkSize)
                    Debug.WriteLine($"HOST: {Host}: write events: {page} / {total}");

                var currentChunkSize = Math.Min(eventsCount, EventsChunkSize);

                var eventsChunk = GenerateRandomEvents(currentChunkSize);

                await Append(GetLogs(eventsChunk));

                eventsCount -= EventsChunkSize;
            }
        }

        public IEnumerable<Event> GenerateRandomEvents(int size)
        {
            for (var i = 0; i < size; i++)
                yield return new Event
                {
                    Id = Guid.NewGuid().ToString(),
                    StartTime = _time() + _random.Next(),
                    Duration = _random.Next(MaxEventDurationInMillis),
                    Type = Guid.NewGuid().ToString(),
                    Host = Guid.NewGuid().ToString()
                };
        }

        public IEnumerable<LoggedEvent> GetLogs(IEnumerable<Event> events)
        {
            var lines = new HashSet<LoggedEvent>();

            foreach(var e in events)
            {
                lines.Add(new LoggedEvent(e, EventState.Started));
                lines.Add(new LoggedEvent(e, EventState.Finished));
            }
            return Shuffle(lines.ToArray());
        }

        public async Task Append(IEnumerable<Event> events)
            => await Append(GetLogs(events));

        public async Task Append(IEnumerable<LoggedEvent> lines)
        {
            var data = new StringBuilder();

            foreach (var line in lines)
                data.Append(Serialize(line) + "\n");

            await File.AppendAllTextAsync(_filePath, data.ToString());
        }

        private T[] Shuffle<T>(T[] array)
        {
            int n = array.Length;
            for (int i = 0; i < n; i++)
            {
                int r = i + _random.Next(n - i);
                T t = array[r];
                array[r] = array[i];
                array[i] = t;
            }
            return array;
        }

        private long Millis(DateTime time) => time.Ticks / TimeSpan.TicksPerMillisecond;

        private string Serialize<T>(T o)
            => JsonConvert.SerializeObject(
                o,
                Formatting.None,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
    }
}
