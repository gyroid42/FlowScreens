namespace Collections
{
    public struct FixedQueueManaged<T>
        where T : class
    {
        private readonly T[] m_array;
        private int m_count;
        private int m_head;
        private int m_tail;

        public FixedQueueManaged(in int capacity)
        {
            m_array = new T[capacity];
            m_count = 0;
            m_head = 0;
            m_tail = 0;
        }

        public int Count => m_count;

        public T this[in int index] => m_array[(m_head + index) % m_array.Length];

        public void Clear()
        {
            m_count = 0;
            m_head = 0;
            m_tail = 0;

            for (int i = 0; i < m_array.Length; i++)
            {
                m_array[i] = null;
            }
        }

        public T Peek()
        {
            return m_array[m_head];
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
            m_array[m_head] = null;
            m_head = (m_head + 1) % m_array.Length;
            return item;
        }
    }
}