using System;
using System.Collections.Generic;

namespace ACE.Server.Physics.Alt
{
    /// <summary>
    /// A simple dynamic array with some SmartArray semantics from GDLE/PhatSDK.
    /// </summary>
    public class SmartArray<T>
    {
        private List<T> _data;

        public int Count => _data.Count;

        public SmartArray(int initialSize = 8)
        {
            _data = new List<T>(initialSize);
        }

        public void Add(T item)
        {
            _data.Add(item);
        }

        public bool RemoveUnordered(T item)
        {
            int idx = _data.IndexOf(item);
            if (idx >= 0)
            {
                int lastIdx = _data.Count - 1;
                _data[idx] = _data[lastIdx];
                _data.RemoveAt(lastIdx);
                return true;
            }
            return false;
        }

        public void Grow(int newSize)
        {
            if (newSize > _data.Capacity)
                _data.Capacity = newSize;
        }

        public T this[int index]
        {
            get => _data[index];
            set => _data[index] = value;
        }

        public void Clear() => _data.Clear();
        public T[] ToArray() => _data.ToArray();
    }
} 