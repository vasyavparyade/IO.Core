using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using IO.Core.Communication;

namespace IO.Core.Sample
{
    public class MoqCommunicator : ICommunicator
    {
        private readonly AutoResetEvent _event = new AutoResetEvent(false);

        public MoqCommunicator()
        {
        }

        public bool Open()
        {
            return true;
        }

        public bool Close()
        {
            return false;
        }

        public void Write(ReadOnlyMemory<byte> buffer)
        {
            Console.WriteLine(Encoding.ASCII.GetString(buffer.ToArray()));
            _event.Set();
        }

        public int Read(Memory<byte> buffer)
        {
            _event.WaitOne();

            const string str = "Hello my friend!\n";
            var bytes = Encoding.ASCII.GetBytes(str);
            new Span<byte>(bytes, 0, bytes.Length).CopyTo(buffer.Span);

            return str.Length;
        }

        public async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken token = default)
        {
            await Task.Run(() => Write(buffer), token).ConfigureAwait(false);
        }

        public async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken token = default)
        {
            return await Task.Run(() => Read(buffer), token).ConfigureAwait(false);
        }
    }
}
