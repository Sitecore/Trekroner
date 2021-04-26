using System;
using System.Text;

namespace Sitecore.Trekroner.ContainerService
{
    public class InfiniteStreamReader : IDisposable
    {
        private StringBuilder _messageBuilder = new StringBuilder();
        private Decoder _decoder = Encoding.UTF8.GetDecoder();

        public void WriteBytes(byte[] data, int count)
        {
            var charBuffer = new char[count];
            var charCount = _decoder.GetChars(data, 0, count, charBuffer, 0); // stateful -- will retain any incomplete bytes
            _messageBuilder.Append(charBuffer, 0, charCount);
        }

        public string ReadNextLine()
        {
            var index = _messageBuilder.ToString().IndexOf('\n');
            if (index < 0)
            {
                return null;
            }
            var nextLine = _messageBuilder.ToString(0, index + 1);
            _messageBuilder = _messageBuilder.Remove(0, index + 1);
            return nextLine;
        }

        public void Dispose()
        {
            _messageBuilder.Clear();
            _decoder.Reset();
        }
    }
}
