namespace Collections
{
    public struct FixedStackManaged<T>
        where T : class
    {
        private int m_count;
        private readonly T[] m_array;

        public FixedStackManaged(in int capacity)
        {
            m_array = new T[capacity];
            m_count = 0;
        }

        public int Count => m_count;

        public T this[in int index] => m_array[index];

        public void Clear()
        {
            m_count = 0;

            for (int i = 0; i < m_array.Length; i++)
            {
                m_array[i] = null;
            }
        }

        public T Peek()
        {
            return m_array[m_count - 1];
        }

        public T Pop()
        {
            --m_count;

            var item = m_array[m_count];
            m_array[m_count] = null;
            return item;
        }

        public void Push(in T item)
        {
            m_array[m_count] = item;
            m_count++;
        }
    }
}