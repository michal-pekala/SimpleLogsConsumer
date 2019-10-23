using SimpleLogsConsumer;
using System;
using System.Threading.Tasks;

namespace LogLiteDB
{
    class Program
    {
        private const int DefaultConsumerThreadsCount = 10;
        private const string DbName = "ConsumedLogs";

        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("arg0: path to logs file expected (string)");
                Console.WriteLine("[arg1: consumers count (int)]");
                Console.WriteLine("press enter to exit");
                Console.ReadLine();
                return;
            }

            var consumerThreadsCount = DefaultConsumerThreadsCount;

            if (args.Length > 1)
                consumerThreadsCount = int.Parse(args[1]);

            await new App(args[0], DbName)
                .Run(consumerThreadsCount);
        }
    }
}
