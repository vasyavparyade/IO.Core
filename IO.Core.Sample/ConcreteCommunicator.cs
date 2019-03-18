using System;
using System.Buffers;
using System.Text;

using IO.Core.Communication;

namespace IO.Core.Sample
{
    public class ConcreteCommunicator : Communicator<Message, MoqCommunicator>
    {
        private readonly MoqCommunicator _communicator;

        public ConcreteCommunicator(MoqCommunicator communicator) : base(communicator)
        {
            _communicator = communicator ?? throw new ArgumentNullException(nameof(communicator));
        }

        protected override Message ParseMessage(ReadOnlySequence<byte> data)
        {
            return new Message
            {
                Text = Encoding.ASCII.GetString(data.ToArray())
            };
        }

        protected override SequencePosition? GetPosition(ReadOnlySequence<byte> buffer)
        {
            return buffer.PositionOf((byte)('\n'));
        }
    }
}
