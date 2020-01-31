using System;
using System.Collections;
using System.Collections.Generic;

namespace DataStructures
{
    public class AvlTreeDictionary<TKey, TValue>: IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    {
        private readonly AvlTree<KeyValuePair<TKey, TValue>> _tree;
        private readonly IComparer<TKey> _comparer;
        private KeyCollection _keys;
        private ValueCollection _values;

        public AvlTreeDictionary(IComparer<TKey> comparer)
        {
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
            _tree = new AvlTree<KeyValuePair<TKey, TValue>>(new KeyValuePairComparer(comparer));
        }

        public AvlTreeDictionary()
            : this(Comparer<TKey>.Default)
        {
        }

        public int Count => _tree.Count;

        public KeyCollection Keys => _keys ?? (_keys = new KeyCollection(this));

        public ValueCollection Values => _values ?? (_values = new ValueCollection(this));

        public IComparer<TKey> Comparer => _comparer;

        public TValue this[TKey key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }
                var node = _tree.FindNode(new KeyValuePair<TKey, TValue>(key, default));
                if (node == null)
                {
                    throw new KeyNotFoundException();
                }
                return node.Item.Value;
            }
            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }
                var item = new KeyValuePair<TKey, TValue>(key, value);
                if (_tree.FindOrCreateNode(item, out var node))
                {
                    node.Item = item;
                }
            }
        }

        public void Add(TKey key, TValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (!TryAdd(key, value))
            {
                throw new ArgumentException("An item with the same key has already been added.", nameof(key));
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
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
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
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
            _tree.InOrderTreeWalk(item =>
            {
                array[index++] = item;
                return true;
            });
        }

        public bool Remove(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            return _tree.Remove(new KeyValuePair<TKey, TValue>(key, default));
        }

        public void Clear()
        {
            _tree.Clear();
        }

        public bool TryAdd(TKey key, TValue value)
        {
            return !_tree.FindOrCreateNode(new KeyValuePair<TKey, TValue>(key, value), out _);
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
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

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_tree);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;

        ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(_tree);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            var node = _tree.FindNode(item);
            if (node == null)
            {
                return false;
            }
            return EqualityComparer<TValue>.Default.Equals(item.Value, node.Item.Value);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            var node = _tree.FindNode(item);
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
            private readonly AvlTreeDictionary<TKey, TValue> _owner;

            public KeyCollection(AvlTreeDictionary<TKey, TValue> owner)
            {
                _owner = owner;
            }

            public int Count => _owner.Count;

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
                return _owner.ContainsKey(item);
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
                if (array.Length - index < _owner.Count)
                {
                    throw new ArgumentException(nameof(index));
                }
                _owner._tree.InOrderTreeWalk(item =>
                {
                    array[index++] = item.Key;
                    return true;
                });
            }

            public bool Remove(TKey item)
            {
                throw new NotSupportedException();
            }

            public KeyEnumerator GetEnumerator()
            {
                return new KeyEnumerator(_owner._tree);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new KeyEnumerator(_owner._tree);
            }

            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
            {
                return new KeyEnumerator(_owner._tree);
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
            private readonly AvlTreeDictionary<TKey, TValue> _owner;

            public ValueCollection(AvlTreeDictionary<TKey, TValue> owner)
            {
                _owner = owner;
            }

            public int Count => _owner.Count;

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
                return _owner.ContainsValue(item);
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
                if (array.Length - index < _owner.Count)
                {
                    throw new ArgumentException(nameof(index));
                }
                _owner._tree.InOrderTreeWalk(item =>
                {
                    array[index++] = item.Value;
                    return true;
                });
            }

            public bool Remove(TValue item)
            {
                throw new NotSupportedException();
            }

            public ValueEnumerator GetEnumerator()
            {
                return new ValueEnumerator(_owner._tree);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new ValueEnumerator(_owner._tree);
            }

            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
            {
                return new ValueEnumerator(_owner._tree);
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
