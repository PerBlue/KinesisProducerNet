using System;
using System.Collections.Generic;
using System.IO;

namespace KinesisProducerNet
{
    public class StreamBuffer
    {
        private readonly Stream ioStream;
        private readonly Queue<byte> pending;

        public StreamBuffer(Stream ioStream)
        {
            this.ioStream = ioStream;
            this.pending = new Queue<byte>();
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
            if (!TryReadPending(sizeof(int), out var lenBuffer))
            {
                buffer = default;
                return false;
            }

            var len = 0;
            for (var i = 0; i < lenBuffer.Length; i++)
            {
                len = len * 256 + lenBuffer[i];
            }

            return TryReadPending(len, out buffer);
        }

        private bool TryReadPending(int count, out byte[] buffer)
        {
            var pendingCount = pending.Count;
            buffer = new byte[count];
            
            for (var i = 0; i < Math.Min(count, pendingCount); i++)
            {
                buffer[i] = pending.Dequeue();
            }

            var missing = count - pendingCount;
            if (missing <= 0) return true;
            
            var bytesRead = ioStream.Read(buffer, pendingCount, missing);
            if (bytesRead != missing)
            {
                for (var i = 0; i < pendingCount + bytesRead; i++)
                {
                    pending.Enqueue(buffer[i]);
                }

                buffer = default;
                return false;
            }

            return true;
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