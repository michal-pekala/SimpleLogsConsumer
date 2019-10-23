using System;
using System.Collections.Concurrent;

namespace SimpleLogsConsumer
{
    public class LogsConsumer
    {
        private readonly LogsRepo _db;

        public LogsConsumer(LogsRepo db)
        {
            _db = db;
        }

        public void Consume(BlockingCollection<LoggedEvent> queue)
        {
            while (!queue.IsCompleted)
            {
                LoggedEvent log = null;

                try
                {
                    log = queue.Take();
                }
                catch (InvalidOperationException) { }

                if (log != null)
                    _db.Insert(log);
            }
        }
    }
}
