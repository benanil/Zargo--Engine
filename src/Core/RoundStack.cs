using System;

namespace ZargoEngine
{
    using Mathmatics;

    [Serializable]
    public class RoundStack<T>
    {
        private readonly T[] items;

        public int Capacity => items.Length;
        public int Count;

        public RoundStack(int capacity)
        {
            items = new T[capacity + 1];
        }

        public T Pop()
        {
            Count--;
            var value = items[0];
            Array.Copy(items, 1, items, 0, Mathmatic.Min(Capacity - 2, Count));
            return value;
        }

        public bool TryPop(out T value)
        {
            value = default;
            if (Count < 1) return false;

            value = Pop();
            return value != null;
        }

        public void Push(T item)
        {
            Count++;
            items[^1] = default;
            Array.Copy(items, 0, items, 1, Mathmatic.Min(Capacity - 2, Count));
            items[0] = item;
        }

        public void Clear()
        {
            Array.Clear(items, 0, items.Length);
            GC.SuppressFinalize(this);
        }
    }
}