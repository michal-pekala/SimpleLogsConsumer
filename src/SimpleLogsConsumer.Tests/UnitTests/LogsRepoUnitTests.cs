using LiteDB;
using SimpleLogsConsumer.Tests.Data;
using System;
using System.Linq;
using Xunit;

namespace SimpleLogsConsumer.UnitTests
{
    public class LogsRepoUnitTests : IDisposable
    {
        private readonly Random _rand = new Random();
        private readonly string _dbName = Guid.NewGuid().ToString();

        private readonly App _app;
        private readonly LogsRepo _sut;

        public LogsRepoUnitTests()
        {
            // cannot mock LiteCollection<Event>, it is sealed

            _app = new App(null, _dbName);
            _sut = _app.Get<LogsRepo>();
        }

        public void Dispose()
        {
            _app.Get<LiteDatabase>().Dispose();
            TmpFilesManager.DropDb(_dbName);
        }

        [Fact]
        public void Event_duration_is_a_diff_of_timestamps()
        {
            // Given
            var (start, end) = PrepareLogs();

            // When
            _sut.Insert(start);
            _sut.Insert(end);

            var diff = Math.Abs(end.TimeStamp - start.TimeStamp);

            // Then
            Assert.Equal(diff, Consumed(start.Id).Duration);
        }

        [Fact]
        public void The_order_of_logs_does_not_matter()
        {
            var (start, end) = PrepareLogs();

            _sut.Insert(end);
            _sut.Insert(start);

            var diff = Math.Abs(end.TimeStamp - start.TimeStamp);

            Assert.Equal(diff, Consumed(start.Id).Duration);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void Duration_less_or_equal_4_millis_then_alert_is_false(int duration)
        {
            var startTime = _rand.Next();
            var (start, end) = PrepareLogs(
                start: s => s.TimeStamp = startTime,
                end: e => e.TimeStamp = startTime + duration);

            _sut.Insert(end);
            _sut.Insert(start);

            Assert.False(Consumed(start.Id).Alert);
        }

        [Fact]
        public void Duration_greater_than_4_millis_then_alert_is_true()
        {
            var duration = _rand.Next() + 4;
            var startTime = _rand.Next();
            var (start, end) = PrepareLogs(
                start: s => s.TimeStamp = startTime,
                end: e => e.TimeStamp = startTime + duration);

            _sut.Insert(end);
            _sut.Insert(start);

            Assert.True(Consumed(start.Id).Alert);
        }

        [Fact]
        public void Big_timestamp_values_are_supported()
        {
            var diff = 1234;

            var (start, end) = PrepareLogs(
                start: s => s.TimeStamp = long.MaxValue - diff,
                end: e => e.TimeStamp = long.MaxValue);

            _sut.Insert(end);
            _sut.Insert(start);

            Assert.Equal(diff, Consumed(start.Id).Duration);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        [InlineData("xyz")]
        [InlineData("abcd ")]
        public void Type_is_logged(string type)
        {
            var (start, end) = PrepareLogs(start: s => s.Type = type);

            _sut.Insert(start);

            var expected = string.IsNullOrWhiteSpace(type) ? null : type.Trim();

            Assert.Equal(expected, Consumed(start.Id).Type);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("   ")]
        [InlineData("abc defg ")]
        public void Host_is_logged(string host)
        {
            var (start, end) = PrepareLogs(start: s => s.Host = host);

            _sut.Insert(start);

            var expected = string.IsNullOrWhiteSpace(host) ? null : host.Trim();

            Assert.Equal(expected, Consumed(start.Id).Host);
        }

        #region FIXTURE
        private (LoggedEvent, LoggedEvent) PrepareLogs(
            Action<LoggedEvent> start = null,
            Action<LoggedEvent> end = null)
        {
            var gen = new LogsGenerator(null);
            var lines = gen.GetLogs(gen.GenerateRandomEvents(1));

            start?.Invoke(lines.First());
            end?.Invoke(lines.Last());

            return (lines.First(), lines.Last());
        }

        private Event Consumed(string id)
            => _app.Get<LiteCollection<Event>>().FindById(id);
        #endregion
    }
}
