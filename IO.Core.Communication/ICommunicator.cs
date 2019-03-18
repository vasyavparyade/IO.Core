using System;
using System.Threading;
using System.Threading.Tasks;

namespace IO.Core.Communication
{
    public interface ICommunicator
    {
        bool Open();

        bool Close();

        void Write(ReadOnlyMemory<byte> buffer);

        int Read(Memory<byte> buffer);

        ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken token = default);

        ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken token = default);
    }
}
