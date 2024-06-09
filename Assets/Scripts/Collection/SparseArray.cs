using System;
using System.Collections.Generic;

namespace Collections
{
    public class SparseArray<T> : IEnumerable<T>
    {
        public int Length { get; private set; }

        public int Capacity => m_array.Length;

        private readonly int[] m_keyToIndex;
        private readonly int[] m_indexToKey;
        private readonly T[] m_array;

        private int m_lastKeyIndex = 0;

        public SparseArray(int capacity)
        {
            m_keyToIndex = new int[capacity];
            m_indexToKey = new int[capacity];
            m_array = new T[capacity];

            for (int i = 0; i < m_keyToIndex.Length; i++)
            {
                m_keyToIndex[i] = -1;
            }
        }
        
        public T this[int index]
        {
            get => m_array[index];
            set => m_array[index] = value;
        }

        public T GetValue(int key)
        {
            int index = m_keyToIndex[key];
            return m_array[index];
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

            m_keyToIndex[key] = Length;
            m_indexToKey[Length] = key;
            m_array[Length] = value;
            Length++;
        }

        public int Insert(in T value)
        {
            if (Length == Capacity)
            {
                throw new OutOfMemoryException("array at capacity");
            }

            while (Contains(m_lastKeyIndex))
            {
                m_lastKeyIndex++;
                m_lastKeyIndex %= m_keyToIndex.Length;
            }

            m_keyToIndex[m_lastKeyIndex] = Length;
            m_indexToKey[Length] = m_lastKeyIndex;
            m_array[Length] = value;
            Length++;

            return m_lastKeyIndex;
        }

        public void Remove(int key)
        {
            if (!KeyValid(key) || !Contains(key))
            {
                throw new ArgumentOutOfRangeException("key");
            }

            int index = m_keyToIndex[key];
            
            m_array[index] = m_array[Length - 1];
            m_array[Length - 1] = default;

            int movedKey = m_indexToKey[Length - 1];
            m_keyToIndex[movedKey] = index;

            m_keyToIndex[key] = -1;
            m_indexToKey[index] = movedKey;

            Length--;
        }

        private bool KeyValid(int key) => 
            key >= 0 && key < m_keyToIndex.Length;

        public bool Contains(int key) => m_keyToIndex[key] >= 0;
        
        public bool Contains(Predicate<T> predicate)
        {
            for (int index = 0; index < Length; index++)
            {
                if (predicate(m_array[index]))
                {
                    return true;
                }
            }

            return false;
        }
        
        public IEnumerator<T> GetEnumerator()
        {
            for(int i = 0; i < Length; ++i)
                yield return m_array[i];
        }
        
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}