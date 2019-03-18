using System;
using System.Text;

using IO.Core.Communication;

namespace IO.Core.Sample
{
    public class Message : ICommunicable
    {
        public string Text { get; set; }

        public byte[] GetBytes()
        {
            if (string.IsNullOrWhiteSpace(Text))
                throw new InvalidOperationException($"{nameof(Text)} is null or white space");

            return Encoding.ASCII.GetBytes(Text);
        }
    }
}
