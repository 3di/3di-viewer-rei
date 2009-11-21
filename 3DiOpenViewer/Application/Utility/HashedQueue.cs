/*
 * Copyright (c) 2008-2009, 3Di, Inc. (http://3di.jp/) and contributors.
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of 3Di, Inc., nor the name of the 3Di Viewer
 *       "Rei" project, nor the names of its contributors may be used to
 *       endorse or promote products derived from this software without
 *       specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY 3Di, Inc. AND CONTRIBUTORS ``AS IS'' AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL 3Di, Inc. OR THE
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

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
            Pair<TKey, TValue> pair = queue.Dequeue();
            hashtable.Remove(pair.Key);
            return pair.Value;
        }
    }
}
