using LiteDB;
using SimpleInjector;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SimpleLogsConsumer
{
    public class App
    {
        private const int MaxQueueSize = 100000;

        public readonly Container Container = new Container();

        private readonly string _filePath;
        private readonly string _dbName;

        public App(string filePath, string dbName)
        {
            _filePath = filePath;
            _dbName = dbName;

            Container.Register(
                () => new LiteDatabase(dbName),
                Lifestyle.Singleton);
            Container.Register(() =>
            {
                using var db = Container.GetInstance<LiteDatabase>();
                return db.GetCollection<Event>("events");
            }, Lifestyle.Singleton);
            Container.Register<LogsConsumer>(Lifestyle.Singleton);
            Container.Register<LogsRepo>(Lifestyle.Singleton);
            Container.Register(
                () => new LogsProducer(filePath),
                Lifestyle.Singleton);
        }

        public T Get<T>() where T : class => Container.GetInstance<T>();
    
        public async Task Run(int consumers)
        {
            ShowMetaInfo(consumers);
            var watch = Stopwatch.StartNew();

            Get<LogsRepo>().Clear();

            Log($"db cleaned");

            using var queue = new BlockingCollection<LoggedEvent>(MaxQueueSize);
            var producer = Get<LogsProducer>();
            var consume = GetConsumers(consumers, queue);

            await Task.WhenAll(
                producer.ReadFileAndProduceEvents(queue),
                Task.WhenAll(consume));

            watch.Stop();
            Log($"LOGS IMPORT FINISHED IN: {watch.Elapsed}");
            Log($"TOTAL EVENTS COUNT: {Get<LogsRepo>().GetCount(e => true)}");
            Log($"TOTAL ALERTS COUNT: {Get<LogsRepo>().GetCount(e => e.Alert)}");
        }

        private IEnumerable<Task> GetConsumers(int consumersCount,
            BlockingCollection<LoggedEvent> queue)
        {
            for (int i = 0; i < consumersCount; i++)
                yield return Task.Factory
                    .StartNew(() => Get<LogsConsumer>().Consume(queue));
        }

        private void ShowMetaInfo(int consumersCount)
        {
            Log($"LOGS IMPORT STARTED ############################");
            Log($"imported file path: {_filePath}");
            Log($"LiteDb file name: {_dbName}");
            Log($"consumers count: {consumersCount}");
        }

        private void Log(string data)
            => Console.WriteLine($"{nameof(SimpleLogsConsumer)} {data}");
    }
}
