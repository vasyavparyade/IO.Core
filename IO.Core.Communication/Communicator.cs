using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace IO.Core.Communication
{
    public abstract class Communicator<TCommunicable, TCommunicator>
        where TCommunicable : class, ICommunicable
        where TCommunicator : class, ICommunicator, new()
    {
        private readonly TCommunicator _communicator;

        private readonly ConcurrentQueue<TCommunicable> _queue = new ConcurrentQueue<TCommunicable>();
        private readonly AutoResetEvent _receiveEvent = new AutoResetEvent(false);

        protected Communicator(TCommunicator communicator)
        {
            _communicator = communicator ?? throw new ArgumentNullException(nameof(communicator));
        }

        public bool Open()
        {
            return _communicator.Open();
        }

        public bool Close()
        {
            return _communicator.Close();
        }

        public Task<bool> OpenAsync()
        {
            return Task.FromResult(Open());
        }

        public Task<bool> CloseAsync()
        {
            return Task.FromResult(Close());
        }

        public async Task WriteAsync(TCommunicable data, CancellationToken token = default)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            await _communicator.WriteAsync(data.GetBytes(), token).ConfigureAwait(false);
        }

        public Task<TCommunicable> ReadAsync()
        {
            return Task.Run(Dequeue);
        }

        public void Start(CancellationToken token = default)
        {
            var pipe = new Pipe();

            var writing = FillProcessAsync(pipe.Writer, token);
            var reading = ReadProcessAsync(pipe.Reader, token);

            Task.Run(async () => await Task.WhenAll(reading, writing).ConfigureAwait(false), token);
        }

        private void Enqueue(TCommunicable data)
        {
            if (!(data is null))
            {
                _queue.Enqueue(data);
                _receiveEvent.Set();
            }
        }

        private async Task FillProcessAsync(PipeWriter writer, CancellationToken token = default)
        {
            const int minimumBufferSize = 512;

            while (true)
            {
                var memory = writer.GetMemory(minimumBufferSize);

                try
                {
                    int bytesRead = await _communicator.ReadAsync(memory, token).ConfigureAwait(false);

                    if (bytesRead == 0)
                    {
                        break;
                    }

                    writer.Advance(bytesRead);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);

                    break;
                }

                var result = await writer.FlushAsync(token).ConfigureAwait(false);

                if (result.IsCompleted)
                {
                    break;
                }
            }

            writer.Complete();
        }

        private async Task ReadProcessAsync(PipeReader reader, CancellationToken token = default)
        {
            while (true)
            {
                var result = await reader.ReadAsync(token).ConfigureAwait(false);

                var buffer = result.Buffer;

                SequencePosition? position;

                do
                {
                    position = GetPosition(buffer);

                    if (position is null)
                        continue;

                    var newPosition = buffer.GetPosition(1, position.Value);

                    Enqueue(ParseMessage(buffer.Slice(0, newPosition)));

                    buffer = buffer.Slice(newPosition);
                } while (position != null);

                reader.AdvanceTo(buffer.Start, buffer.End);

                if (result.IsCompleted)
                {
                    break;
                }
            }

            reader.Complete();
        }

        private TCommunicable Dequeue()
        {
            _receiveEvent.WaitOne();

            _queue.TryDequeue(out var result);

            return result;
        }

        protected abstract TCommunicable ParseMessage(ReadOnlySequence<byte> data);

        protected abstract SequencePosition? GetPosition(ReadOnlySequence<byte> buffer);
    }
}
