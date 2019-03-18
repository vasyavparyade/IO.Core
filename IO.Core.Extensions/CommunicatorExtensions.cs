using System;
using System.Threading.Tasks;

using IO.Core.Communication;

namespace IO.Core.Extensions
{
    public static class CommunicatorExtensions
    {
        public static async Task<TCommunicable> TransmitReceiveAsync<TCommunicable, TCommunicator>(
            this Communicator<TCommunicable, TCommunicator> communicator,
            TCommunicable data
        )
            where TCommunicable : class, ICommunicable
            where TCommunicator : class, ICommunicator, new()
        {
            if (communicator is null)
                throw new ArgumentNullException(nameof(communicator));

            if (data is null)
                throw new ArgumentNullException(nameof(data));

            await communicator.WriteAsync(data).ConfigureAwait(false);

            return await communicator.ReadAsync().ConfigureAwait(false);
        }
    }
}
