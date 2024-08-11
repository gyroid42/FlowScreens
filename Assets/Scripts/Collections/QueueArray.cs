using System.Runtime.CompilerServices;

namespace Collections
{
    public readonly unsafe struct QueueArray<T> 
        where T : unmanaged
    {
        public readonly int Length;
        private readonly short m_queueCapacity;
        private readonly byte[] m_data;

        public QueueArray(in int arrayLength, in short capacity)
        {
            // each item is (count + startIndex + endIndex + padding + itemArray)
            Length = arrayLength;
            m_queueCapacity = capacity;
            int queueSize = 4 * sizeof(short) + sizeof(T) * capacity;
            m_data = new byte[arrayLength * queueSize];
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte* GetQueuePointer(in int queueIndex)
        {
            fixed (byte* p = &m_data[queueIndex])
            {
                return p;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short Count(in int queueIndex)
        {
            return *(short*)GetQueuePointer(queueIndex);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private short* Count(in byte* queuePointer)
        {
            return (short*)queuePointer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private short* StartIndex(in byte* queuePointer)
        {
            return (short*)(queuePointer + sizeof(short));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private short* EndIndex(in byte* queuePointer)
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

            *Count(queuePointer) = 0;
            *StartIndex(queuePointer) = *EndIndex(queuePointer);
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

            ++*Count(queuePointer);
            var endIndex = EndIndex(queuePointer);

            *GetItem(queuePointer, *endIndex) = item;
            ++*endIndex;
            *endIndex %= m_queueCapacity;
        }

        public T Dequeue(in int queueIndex)
        {
            var queuePointer = GetQueuePointer(queueIndex);

            --*Count(queuePointer);
            var startIndex = StartIndex(queuePointer);
            var item = GetItem(queuePointer, *startIndex);

            ++*startIndex;
            *startIndex %= m_queueCapacity;
            return *item;
        }

        public ref T Peek(in int queueIndex)
        {
            var queuePointer = GetQueuePointer(queueIndex);

            var startIndex = *StartIndex(queuePointer);
            return ref *GetItem(queuePointer, startIndex);
        }
    }
}