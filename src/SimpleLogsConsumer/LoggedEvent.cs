using System;

namespace SimpleLogsConsumer
{
    public class LoggedEvent : Event
    {
        public string State { get; set; }

        public long TimeStamp { get; set; }

        public LoggedEvent() { }

        public LoggedEvent(Event e, string state)
        {
            Id = e.Id;
            Type = e.Type;
            Host = e.Host;
            State = state;
            TimeStamp = e.StartTime +
                (state == EventState.Finished ? e.Duration : 0);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            var o = obj as LoggedEvent;

            return
                Id == o.Id &&
                Type == o.Type &&
                Host == o.Host &&
                State == o.State &&
                TimeStamp == o.TimeStamp; 
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Type, Host, State, TimeStamp);
        }
    }
}
