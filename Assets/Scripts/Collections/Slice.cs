using System;
using System.Collections;
using System.Collections.Generic;

namespace Collections
{
    public class Slice<T> : ICollection, IReadOnlyCollection<T>
    {
        private readonly T[] m_array;
        private readonly int m_startIndex;
        private readonly int m_count;

        public Slice(T[] array, int startIndex, int length)
        {
            if (array.Length < startIndex + length)
            {
                throw new ArgumentOutOfRangeException("startIndex + length");
            }
            
            m_array = array;
            m_startIndex = startIndex;
            m_count = length;
        }

        public T this[int index]
        {
            get
            {
                if (index >= m_count)
                {
                    throw new ArgumentOutOfRangeException("index");
                }
                
                return m_array[m_startIndex + index];
            }
        }

        public void CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("arrayIndex");
            }

            if (array.Rank > 1)
            {
                throw new ArgumentException("array is multidimensional");
            }

            if (array.Length < m_count + index)
            {
                throw new ArgumentException("Not enough elements after index in the destination array");
            }

            for (int i = 0; i < m_count; i++)
            {
                array.SetValue(this[i], i + index);
            }
        }

        public int Count => m_count;
        
        public bool IsSynchronized { get; }
        public object SyncRoot { get; }
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Contains(Predicate<T> predicate)
        {
            for (int i = 0; i < m_count; i++)
            {
                if (predicate(this[i]))
                {
                    return true;
                }
            }

            return false;
        }

        public class Enumerator : IEnumerator<T>
        {
            private readonly Slice<T> m_slice;
            private int m_index;
            private IEnumerator<T> _enumeratorImplementation;

            internal Enumerator(Slice<T> slice)
            {
                m_slice = slice;
                m_index = -1;
            }
            
            public bool MoveNext()
            {
                m_index++;
                return (m_index < m_slice.Count);
            }

            public void Reset()
            {
                m_index = -1;
            }

            object IEnumerator.Current => Current;

            public T Current
            {
                get
                {
                    if (m_index < 0 || m_index >= m_slice.Count)
                    {
                        throw new IndexOutOfRangeException("m_index");
                    }

                    return m_slice[m_index];
                }
            }

            public void Dispose()
            {
            }
        }
    }
}