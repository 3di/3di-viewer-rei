using System;
using System.Collections.Generic;

namespace OpenViewer.Utility
{
    public class Pair<TKey, TValue>
    {
        public TKey Key;
        public TValue Value;

        public Pair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }

    public class HashedQueue<TKey, TValue>
    {
        private Dictionary<TKey, Pair<TKey, TValue>> hashtable;
        private Queue<Pair<TKey, TValue>> queue;

        public HashedQueue()
        {
            hashtable = new Dictionary<TKey, Pair<TKey, TValue>>();
            queue = new Queue<Pair<TKey, TValue>>();
        }

        public HashedQueue(int capacity)
        {
            hashtable = new Dictionary<TKey, Pair<TKey, TValue>>(capacity * 2);
            queue = new Queue<Pair<TKey, TValue>>(capacity);
        }

        public void Clear()
        {
            hashtable.Clear();
            queue.Clear();
        }

        public int Count
        {
            get
            {
                return queue.Count;
            }
        }

        public bool TryGetValue(TKey key, out Pair<TKey, TValue> pair)
        {
            return hashtable.TryGetValue(key, out pair);
        }

        public void Enqueue(TKey key, TValue value)
        {
            Pair<TKey, TValue> pair = new Pair<TKey, TValue>(key, value);
            hashtable.Add(key, pair);
            queue.Enqueue(pair);
        }

        public TValue Dequeue()
        {
            Pair<TKey, TValue> pair;

            pair = queue.Dequeue();
            hashtable.Remove(pair.Key);
            return pair.Value;
        }
    }
}
