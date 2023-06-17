using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LLama.Enumerations
{
    /// <summary>
    ///     A queue with fixed storage size.
    ///     Currently it's only a naive implementation and needs to be further optimized in the future.
    /// </summary>
    public class FixedSizeQueue<T> : IEnumerable<T>
    {
        private readonly List<T> _storage;

        public FixedSizeQueue(int size)
        {
            Capacity = size;
            _storage = new List<T>();
        }

        /// <summary>
        ///     Fill the quene with the data. Please ensure that data.Count <= size
        /// </summary>
        /// <param name="size"></param>
        /// <param name="data"></param>
        public FixedSizeQueue(int size, IEnumerable<T> data)
        {
            Capacity = size;
            if (data.Count() > size)
            {
                throw new ArgumentException(
                    $"The max size set for the queue is {size}, but got {data.Count()} initial values.");
            }

            _storage = new List<T>(data);
        }

        public int Count => _storage.Count;
        public int Capacity { get; }

        public IEnumerator<T> GetEnumerator()
        {
            return _storage.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public FixedSizeQueue<T> FillWith(T value)
        {
            for (var i = 0; i < Count; i++)
            {
                _storage[i] = value;
            }

            return this;
        }

        /// <summary>
        ///     Enqueue an element.
        /// </summary>
        /// <returns></returns>
        public void Enqueue(T item)
        {
            _storage.Add(item);
            if (_storage.Count >= Capacity)
            {
                _storage.RemoveAt(0);
            }
        }

        public T[] ToArray()
        {
            return _storage.ToArray();
        }
    }
}
