using Xunit;
using SimpleLogsConsumer.Tests.Data;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;
using LiteDB;

namespace SimpleLogsConsumer.IntegrationTests
{
    public class AppIntegrationTests : IDisposable
    {
        private const int ConsumersCount = 10;
        private const int EventsCount = 1000;

        private readonly string _dbName = Guid.NewGuid().ToString();
        private readonly string _filePath = TmpFilesManager.GetTmpFilePath();

        private readonly App _sut;

        public AppIntegrationTests()
        {
            _sut = new App(_filePath, _dbName);
        }

        [Fact]
        public async Task ProducerWithMultipleConsumersTest()
        {
            var events = await SetupData(_filePath, EventsCount);

            await _sut.Run(ConsumersCount);

            var consumed = _sut.Get<LogsRepo>().GetAll();

            foreach (var e in events)
            {
                var log = consumed.Single(c => c.Id == e.Id);
                Assert.Equal(e.Duration > Event.AlertTreshold, log.Alert);
            }
        }

        [Fact(Skip="Create large file")]
        public async Task GenerateLargeFile()
        {
            var gen = new LogsGenerator(TmpFilesManager.GetTmpFilePath());

            await gen.GenerateLargeFile(10000);
        }

        private async Task<IEnumerable<Event>> SetupData(string filePath, int size)
        {
            var gen = new LogsGenerator(filePath);
            var events = gen.GenerateRandomEvents(size).ToList();
            await gen.Append(events);
            return events;
        }

        public void Dispose()
        {
            _sut.Get<LiteDatabase>().Dispose();
            TmpFilesManager.Delete(_filePath);
            TmpFilesManager.DropDb(_dbName);
        }
    }
}
