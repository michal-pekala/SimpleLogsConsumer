using SimpleLogsConsumer.Tests.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SimpleLogsConsumer.UnitTests
{
    public class LogsProducerUnitTests : IDisposable
    {
        private const int N = 10;
        private readonly string _filePath = TmpFilesManager.GetTmpFilePath();

        public void Dispose() => TmpFilesManager.Delete(_filePath);

        [Fact]
        public async Task Each_line_of_file_is_read_and_sent_to_the_queue()
        {
            var fileContent = await SetupFile(_filePath, N);
            var queue = new BlockingCollection<LoggedEvent>(2*N);

            var sut = new LogsProducer(_filePath);

            await sut.ReadFileAndProduceEvents(queue);

            Assert.True(queue.SequenceEqual(fileContent));
        }

        private async Task<IEnumerable<LoggedEvent>> SetupFile(string filePath, int size)
        {
            var gen = new LogsGenerator(filePath);
            var logs = gen.GetLogs(gen.GenerateRandomEvents(size));
            await gen.Append(logs);
            return logs;
        }
    }
}
