using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SimpleLogsConsumer
{
    public class LogsRepo
    {
        private readonly LiteCollection<Event> _events;

        public LogsRepo(LiteCollection<Event> events)
        {
            _events = events;
        }

        public void Clear() => _events.Delete(l => true);       

        public void Insert(LoggedEvent log)
        {
            var e = _events.FindById(log.Id);

            if (e == null)
            {
                try
                {
                    _events.Insert(new Event
                    {
                        Id = log.Id,
                        Host = log.Host,
                        Type = log.Type,
                        StartTime = log.TimeStamp
                    });
                    return;
                } catch (Exception ex)
                {
                    if (!ex.Message.Contains("duplicate"))
                        throw ex;
                }
            }
                
            if (e == null)
                e = _events.FindById(log.Id);

            e.Duration = Math.Abs(e.StartTime - log.TimeStamp);
            e.Alert = e.Duration > Event.AlertTreshold;

            _events.Update(e);
        }

        public IEnumerable<Event> GetAll() => _events.FindAll();


        public int GetCount(Expression<Func<Event, bool>> predicate)
            => _events.Count(predicate);
    }
}
