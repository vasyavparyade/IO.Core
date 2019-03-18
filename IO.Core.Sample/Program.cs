using System;
using System.Threading.Tasks;

using IO.Core.Extensions;

namespace IO.Core.Sample
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            var communicator = new ConcreteCommunicator(new MoqCommunicator());
            communicator.Open();
            communicator.Start();

            var result = await communicator.TransmitReceiveAsync(new Message
            {
                Text = "Hello world!"
            }).ConfigureAwait(false);
            Console.WriteLine(result.Text);

            var str = Console.ReadLine();
        }
    }
}
