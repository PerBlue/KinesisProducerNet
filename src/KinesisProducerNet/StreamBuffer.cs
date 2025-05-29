using System;
using System.IO;

namespace KinesisProducerNet
{
    public class StreamBuffer
    {
        private readonly Stream ioStream;

        public StreamBuffer(Stream ioStream)
        {
            this.ioStream = ioStream;
        }

        public byte[] ReadBuffer()
        {
            var len = 0;
            for (var i = 0; i < 4; i++)
            {
                len = len * 256 + ioStream.ReadByte();
            }

            var inBuffer = new byte[len];
            ioStream.Read(inBuffer, 0, len);
            return inBuffer;
        }
        
        public bool TryReadBuffer(out byte[] buffer)
        {
            var startingPosition = ioStream.Position;
            var len = 0;
            for (var i = 0; i < 4; i++)
            {
                var b = ioStream.ReadByte();
                if (b == -1)
                {
                    ioStream.Position = startingPosition;
                    buffer = default;
                    return false;
                }
                len = len * 256 + b;
            }

            buffer = new byte[len];
            var bytesRead = ioStream.Read(buffer, 0, len);
            if (bytesRead == len) return true;

            ioStream.Position = startingPosition;
            return false;
        }

        public int WriteBuffer(byte[] outBuffer)
        {
            var len = outBuffer.Length;
            var lenBytes = BitConverter.GetBytes(len);
            for (int i = 3; i >= 0; i--)
            {
                ioStream.WriteByte(lenBytes[i]);
            }

            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();

            return outBuffer.Length + 4;
        }
    }
}