using System;
using System.Collections;
using System.Collections.Generic;

namespace DataStructures
{
    public class AvlTreeDictionary<TKey, TValue>: IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    {
        private readonly AvlTree<KeyValuePair<TKey, TValue>> _tree;
        private readonly KeyValuePairComparer _comparer;
        private KeyCollection _keys;
        private ValueCollection _values;

        public AvlTreeDictionary(IComparer<TKey> comparer)
        {
            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }
            _comparer = new KeyValuePairComparer(comparer);
        }

        public AvlTreeDictionary()
            : this(Comparer<TKey>.Default)
        {
        }

        public int Count => _tree.Count;

        public KeyCollection Keys => _keys ?? (_keys = new KeyCollection(this));

        public ValueCollection Values => _values ?? (_values = new ValueCollection(this));

        public bool IsReadOnly => false;

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;

        ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

        public TValue this[TKey key]
        {
            get
            {
                var node = _tree.FindNode(new KeyValuePair<TKey, TValue>(key, default));
                if (node == null)
                {
                    throw new KeyNotFoundException();
                }
                return node.Item.Value;
            }
            set
            {
                var item = new KeyValuePair<TKey, TValue>(key, value);
                _ = _tree.FindOrCreateNode(item, out var node);
                node.Item = item;
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var node = _tree.FindNode(new KeyValuePair<TKey, TValue>(key, default));
            if (node != null)
            {
                value = node.Item.Value;
                return true;
            }
            value = default;
            return false;
        }

        public bool ContainsKey(TKey key)
        {
            return _tree.Contains(new KeyValuePair<TKey, TValue>(key, default));
        }

        public bool ContainsValue(TValue value)
        {
            var comparer = EqualityComparer<TValue>.Default;
            var walker = new AvlTree<KeyValuePair<TKey, TValue>>.TreeWalker(_tree);
            while (walker.MoveNext())
            {
                var node = walker.Current;
                if (comparer.Equals(node.Item.Value, value))
                {
                    return true;
                }
            }
            return false;
        }

        public bool Add(TKey key, TValue value)
        {
            return !_tree.FindOrCreateNode(new KeyValuePair<TKey, TValue>(key, value), out _);
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            if (valueFactory == null)
            {
                throw new ArgumentNullException(nameof(valueFactory));
            }
            var found = _tree.FindOrCreateNode(new KeyValuePair<TKey, TValue>(key, default), out var node);
            if (!found)
            {
                var value = valueFactory(key);
                node.Item = new KeyValuePair<TKey, TValue>(key, value);
            }
            return node.Item.Value;
        }

        public bool Remove(TKey key)
        {
            return _tree.Remove(new KeyValuePair<TKey, TValue>(key, default));
        }

        public void Clear()
        {
            _tree.Clear();
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_tree);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(_tree);
        }

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            Add(key, value);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            var node = _tree.FindNode(item);
            if (node != null && EqualityComparer<TValue>.Default.Equals(item.Value, node.Item.Value))
            {
                return true;
            }
            return false;
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }
            if (index < 0 || index > array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (array.Length - index < Count)
            {
                throw new ArgumentException(nameof(index));
            }
            var walker = new AvlTree<KeyValuePair<TKey, TValue>>.TreeWalker(_tree);
            while (walker.MoveNext())
            {
                var node = walker.Current;
                array[index++] = node.Item;
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            var node = _tree.FindNode(new KeyValuePair<TKey, TValue>(item.Key, default));
            if (node != null && EqualityComparer<TValue>.Default.Equals(item.Value, node.Item.Value))
            {
                _tree.RemoveNode(node);
                return true;
            }
            return false;
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return new Enumerator(_tree);
        }

        public struct Enumerator: IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private AvlTree<KeyValuePair<TKey, TValue>>.TreeWalker _walker;

            public Enumerator(AvlTree<KeyValuePair<TKey, TValue>> tree)
            {
                _walker = new AvlTree<KeyValuePair<TKey, TValue>>.TreeWalker(tree);
            }

            public bool MoveNext()
            {
                return _walker.MoveNext();
            }

            public void Reset()
            {
                _walker.Reset();
            }

            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    var current = _walker.Current;
                    if (current == null)
                    {
                        throw new InvalidOperationException();
                    }
                    return current.Item;
                }
            }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }
        }

        public sealed class KeyCollection: ICollection<TKey>
        {
            private readonly AvlTreeDictionary<TKey, TValue> _tree;

            public KeyCollection(AvlTreeDictionary<TKey, TValue> tree)
            {
                _tree = tree;
            }

            public int Count => _tree.Count;

            public bool IsReadOnly => true;

            public void Add(TKey item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(TKey item)
            {
                return _tree.ContainsKey(item);
            }

            public void CopyTo(TKey[] array, int index)
            {
                if (array == null)
                {
                    throw new ArgumentNullException(nameof(array));
                }
                if (index < 0 || index > array.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                if (array.Length - index < _tree.Count)
                {
                    throw new ArgumentException(nameof(index));
                }
                var walker = new AvlTree<KeyValuePair<TKey, TValue>>.TreeWalker(_tree._tree);
                while (walker.MoveNext())
                {
                    var node = walker.Current;
                    array[index++] = node.Item.Key;
                }
            }

            public bool Remove(TKey item)
            {
                throw new NotSupportedException();
            }

            public KeyEnumerator GetEnumerator()
            {
                return new KeyEnumerator(_tree._tree);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new KeyEnumerator(_tree._tree);
            }

            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
            {
                return new KeyEnumerator(_tree._tree);
            }

            public struct KeyEnumerator: IEnumerator<TKey>
            {
                private AvlTree<KeyValuePair<TKey, TValue>>.TreeWalker _walker;

                public KeyEnumerator(AvlTree<KeyValuePair<TKey, TValue>> tree)
                {
                    _walker = new AvlTree<KeyValuePair<TKey, TValue>>.TreeWalker(tree);
                }

                public bool MoveNext()
                {
                    return _walker.MoveNext();
                }

                public void Reset()
                {
                    _walker.Reset();
                }

                public TKey Current
                {
                    get
                    {
                        var current = _walker.Current;
                        if (current == null)
                        {
                            throw new InvalidOperationException();
                        }
                        return current.Item.Key;
                    }
                }

                object IEnumerator.Current => Current;

                public void Dispose()
                {
                }
            }
        }

        public sealed class ValueCollection: ICollection<TValue>
        {
            private readonly AvlTreeDictionary<TKey, TValue> _tree;

            public ValueCollection(AvlTreeDictionary<TKey, TValue> tree)
            {
                _tree = tree;
            }

            public int Count => _tree.Count;

            public bool IsReadOnly => true;

            public void Add(TValue item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(TValue item)
            {
                return _tree.ContainsValue(item);
            }

            public void CopyTo(TValue[] array, int index)
            {
                if (array == null)
                {
                    throw new ArgumentNullException(nameof(array));
                }
                if (index < 0 || index > array.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                if (array.Length - index < _tree.Count)
                {
                    throw new ArgumentException(nameof(index));
                }
                var walker = new AvlTree<KeyValuePair<TKey, TValue>>.TreeWalker(_tree._tree);
                while (walker.MoveNext())
                {
                    var node = walker.Current;
                    array[index++] = node.Item.Value;
                }
            }

            public bool Remove(TValue item)
            {
                throw new NotSupportedException();
            }

            public ValueEnumerator GetEnumerator()
            {
                return new ValueEnumerator(_tree._tree);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new ValueEnumerator(_tree._tree);
            }

            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
            {
                return new ValueEnumerator(_tree._tree);
            }

            public struct ValueEnumerator: IEnumerator<TValue>
            {
                private AvlTree<KeyValuePair<TKey, TValue>>.TreeWalker _walker;

                public ValueEnumerator(AvlTree<KeyValuePair<TKey, TValue>> tree)
                {
                    _walker = new AvlTree<KeyValuePair<TKey, TValue>>.TreeWalker(tree);
                }

                public bool MoveNext()
                {
                    return _walker.MoveNext();
                }

                public void Reset()
                {
                    _walker.Reset();
                }

                public TValue Current
                {
                    get
                    {
                        var current = _walker.Current;
                        if (current == null)
                        {
                            throw new InvalidOperationException();
                        }
                        return current.Item.Value;
                    }
                }

                object IEnumerator.Current => Current;

                public void Dispose()
                {
                }
            }
        }

        internal class KeyValuePairComparer: IComparer<KeyValuePair<TKey, TValue>>
        {
            private readonly IComparer<TKey> _comparer;

            public KeyValuePairComparer(IComparer<TKey> comparer)
            {
                _comparer = comparer;
            }

            public int Compare(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
            {
                return _comparer.Compare(x.Key, y.Key);
            }
        }
    }
}
