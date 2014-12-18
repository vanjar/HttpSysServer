using System.Collections.Concurrent;

namespace Microsoft.Net.Http.Server
{
    /// <summary>
    /// Very simple and kind of silly buffer pool
    /// </summary>
    public class BufferPool
    {
        private const int InitialCapacity = 1000;
        private readonly ConcurrentStack<byte[]> _items = new ConcurrentStack<byte[]>();

        public BufferPool()
        {
            // TODO: allocate bytes only once?
            for(int i = 0; i<InitialCapacity; i++)
            {
                _items.Push(new byte[NativeRequestContext.DefaultBufferSize]);
            }
        }

        public byte[] Take(int size)
        {
            byte[] buffer;
            if (size != NativeRequestContext.DefaultBufferSize || !_items.TryPop(out buffer))
            {
                buffer = new byte[size];
            }
            return buffer;
        }

        public void Return(byte[] buffer)
        {
            if (buffer.Length == NativeRequestContext.DefaultBufferSize)
            {
                _items.Push(buffer);
            }
        }
    }
}
