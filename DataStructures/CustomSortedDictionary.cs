using System;
using System.Collections.Generic;

namespace DataStructures
{
    public class CustomSortedDictionary<TKey, TValue>
    {
        private readonly SortedSet<KeyValuePair<TKey, TValue>> _set;

        public CustomSortedDictionary()
        {
            _set = new SortedSet<KeyValuePair<TKey, TValue>>(new KeyValuePairComparer(null));
        }

        public bool Add(TKey key, TValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            return _set.Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        internal class KeyValuePairComparer : Comparer<KeyValuePair<TKey, TValue>>
        {
            internal IComparer<TKey> _keyComparer;

            public KeyValuePairComparer(IComparer<TKey> keyComparer)
            {
                if (keyComparer == null)
                {
                    _keyComparer = Comparer<TKey>.Default;
                }
                else
                {
                    _keyComparer = keyComparer;
                }
            }

            public override int Compare(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
            {
                return _keyComparer.Compare(x.Key, y.Key);
            }
        }
    }
}
