namespace Collections
{
    public struct FixedStack<T>
        where T : unmanaged
    {
        private int m_count;
        private readonly T[] m_array;

        public FixedStack(in int capacity)
        {
            m_array = new T[capacity];
            m_count = 0;
        }

        public int Count => m_count;

        public ref T this[in int index] => ref m_array[index];

        public void Clear()
        {
            m_count = 0;
        }

        public ref T Peek()
        {
            return ref m_array[m_count - 1];
        }

        public T Pop()
        {
            --m_count;

            return m_array[m_count];
        }

        public void Push(in T item)
        {
            m_array[m_count] = item;
            m_count++;
        }
    }
}