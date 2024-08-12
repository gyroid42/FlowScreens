using System;
using System.Runtime.CompilerServices;

namespace Collections
{
    public struct SparseArray<T>
    {
        public int Length => m_length;
        public int Capacity => m_array.Length;

        private readonly int[] m_keyToIndex;
        private readonly int[] m_indexToKey;
        private readonly T[] m_array;

        private int m_length;
        private int m_lastKeyIndex;

        public SparseArray(int capacity)
        {
            m_length = 0;
            m_lastKeyIndex = 0;
            m_keyToIndex = new int[capacity];
            m_indexToKey = new int[capacity];
            m_array = new T[capacity];

            for (int i = 0; i < m_keyToIndex.Length; i++)
            {
                m_keyToIndex[i] = -1;
            }
        }
        
        public ref T this[int index] => ref m_array[index];

        public ref T GetValue(int key)
        {
            int index = m_keyToIndex[key];
            return ref m_array[index];
        }

        public bool TryGetValue(int key, out T value)
        {
            if (!KeyValid(key) && !Contains(key))
            {
                value = default;
                return false;
            }

            value = GetValue(key);
            return true;
        }

        public void Insert(int key, in T value)
        {
            if (!KeyValid(key))
            {
                throw new ArgumentOutOfRangeException(nameof(key), key, "Sparse Array Key out of Range");
            }

            int index = m_keyToIndex[key];
            if (index >= 0)
            {
                m_array[index] = value;
                return;
            }

            m_keyToIndex[key] = m_length;
            m_indexToKey[m_length] = key;
            m_array[m_length] = value;
            m_length++;
        }

        public int Insert(in T value)
        {
            if (m_length == Capacity)
            {
                throw new OutOfMemoryException("array at capacity");
            }

            while (Contains(m_lastKeyIndex))
            {
                m_lastKeyIndex++;
                m_lastKeyIndex %= m_keyToIndex.Length;
            }

            m_keyToIndex[m_lastKeyIndex] = m_length;
            m_indexToKey[m_length] = m_lastKeyIndex;
            m_array[m_length] = value;
            m_length++;

            return m_lastKeyIndex;
        }

        public void Remove(int key)
        {
            if (!KeyValid(key) || !Contains(key))
            {
                throw new ArgumentOutOfRangeException("key");
            }

            int index = m_keyToIndex[key];
            
            m_array[index] = m_array[m_length - 1];
            m_array[m_length - 1] = default;

            int movedKey = m_indexToKey[m_length - 1];
            m_keyToIndex[movedKey] = index;

            m_keyToIndex[key] = -1;
            m_indexToKey[index] = movedKey;

            m_length--;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool KeyValid(int key) => 
            key >= 0 && key < m_keyToIndex.Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(int key) => m_keyToIndex[key] >= 0;
        
        public bool Contains(Predicate<T> predicate)
        {
            for (int index = 0; index < m_length; index++)
            {
                if (predicate(m_array[index]))
                {
                    return true;
                }
            }

            return false;
        }
    }
}