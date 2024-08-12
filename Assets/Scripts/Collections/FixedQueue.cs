namespace Collections
{
    public struct FixedQueue<T>
        where T : unmanaged
    {
        private readonly T[] m_array;
        private int m_count;
        private int m_head;
        private int m_tail;

        public FixedQueue(in int capacity)
        {
            m_array = new T[capacity];
            m_count = 0;
            m_head = 0;
            m_tail = 0;
        }

        public int Count => m_count;

        public ref T this[in int index] => ref m_array[(m_head + index) % m_array.Length];

        public void Clear()
        {
            m_count = 0;
            m_head = 0;
            m_tail = 0;
        }

        public ref T Peek()
        {
            return ref m_array[m_head];
        }

        public void Enqueue(in T item)
        {
            ++m_count;
            m_array[m_tail] = item;
            m_tail = (m_tail + 1) % m_array.Length;
        }

        public T Dequeue()
        {
            --m_count;
            var item = m_array[m_head];
            m_head = (m_head + 1) % m_array.Length;
            return item;
        }
    }
}