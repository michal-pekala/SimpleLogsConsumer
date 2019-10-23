using System;

namespace SimpleLogsConsumer
{
    public class Event
    {
        public const int AlertTreshold = 4;

        public string Id { get; set; }
        public long StartTime { get; set; }
        public long Duration { get; set; }
        public string Type { get; set; }
        public string Host { get; set; }

        public bool Alert { get; set; }
    }
}
