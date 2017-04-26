using System;
using System.Threading.Tasks;

// using alias because of collision between
// Havarnov.AzureServiceBus and Havarnov.AzureServiceBus.CSharpWrapper
using QueueClient = Havarnov.AzureServiceBus.CSharpWrapper.QueueClient<string>;
using AzureServiceBusException = 
    Havarnov.AzureServiceBus.CSharpWrapper.AzureServiceBusException;
using Parser = Havarnov.AzureServiceBus.CSharpWrapper.Parser;

namespace Havarnov.AzureServiceBus.Sample.Client.CSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            Task t = MainAsync(args);
            t.Wait();
        }

        static async Task MainAsync(string[] args) {
            var connectionString =
                Parser.ParseConnectionString(args[0]);
            var name = args[1];

            var queue = new QueueClient(connectionString, name);

            try {
                await queue.PostAsync("foobar --- test1");
                var msg = await queue.ReceiveAsync();
                if (msg != null) {
                    Console.WriteLine("msg received: {0}", msg.Data);
                    await queue.DeleteMsgAsync(
                        msg.Properties.LockToken,
                        msg.Properties.SequenceNumber);
                } else {
                    Console.WriteLine("no msg available in queue");
                }
            } catch (AzureServiceBusException e) {
                Console.WriteLine("exception: {0}", e.Message);
            }

            Console.WriteLine("Hello World!");

            return;
        }
    }
}
