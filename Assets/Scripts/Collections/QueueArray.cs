using System.Runtime.CompilerServices;

namespace Collections
{
    public readonly unsafe struct QueueArray<T> 
        where T : unmanaged
    {
        public readonly int Length;
        private readonly short m_queueCapacity;
        private readonly short m_queueSize;
        private readonly byte[] m_data;

        public QueueArray(in int arrayLength, in short capacity)
        {
            // each item is (count + head + tail + padding + itemArray)
            Length = arrayLength;
            m_queueCapacity = capacity;
            m_queueSize = (short)(4 * sizeof(short) + sizeof(T) * capacity);
            m_data = new byte[arrayLength * m_queueSize];
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte* GetQueuePointer(in int queueIndex)
        {
            fixed (byte* p = &m_data[queueIndex * m_queueSize])
            {
                return p;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetQueueCount(in int queueIndex)
        {
            return *(short*)GetQueuePointer(queueIndex);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private short* GetQueueCount(in byte* queuePointer)
        {
            return (short*)queuePointer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private short* GetHead(in byte* queuePointer)
        {
            return (short*)(queuePointer + sizeof(short));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private short* GetTail(in byte* queuePointer)
        {
            return (short*)(queuePointer + 2 * sizeof(short));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T* GetItem(in byte* queuePointer, in short itemIndex)
        {
            return (T*)(queuePointer + 4 * sizeof(short) + itemIndex * sizeof(T));
        }

        public void Clear(in int queueIndex)
        {
            var queuePointer = GetQueuePointer(queueIndex);

            *GetQueueCount(queuePointer) = 0;
            *GetHead(queuePointer) = 0;
            *GetTail(queuePointer) = 0;
        }

        public void ClearAll()
        {
            for (short i = 0; i < Length; i++)
            {
                Clear(i);
            }
        }

        public void Enqueue(in int queueIndex, in T item)
        {
            var queuePointer = GetQueuePointer(queueIndex);

            ++*GetQueueCount(queuePointer);
            var tail = GetTail(queuePointer);

            *GetItem(queuePointer, *tail) = item;
            ++*tail;
            *tail %= m_queueCapacity;
        }

        public T Dequeue(in int queueIndex)
        {
            var queuePointer = GetQueuePointer(queueIndex);

            --*GetQueueCount(queuePointer);
            var head = GetHead(queuePointer);
            var item = GetItem(queuePointer, *head);

            ++*head;
            *head %= m_queueCapacity;
            return *item;
        }

        public ref T Peek(in int queueIndex)
        {
            var queuePointer = GetQueuePointer(queueIndex);

            var head = *GetHead(queuePointer);
            return ref *GetItem(queuePointer, head);
        }
    }
}