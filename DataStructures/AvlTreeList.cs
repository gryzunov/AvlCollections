using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DataStructures
{
    public class AvlTreeList<TKey, TValue>: IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    {
        private readonly IComparer<TKey> _comparer;
        private Node _root;
        private Node _head;
        private KeyCollection _keys;
        private ValueCollection _values;
        private int _count;

        public AvlTreeList(IComparer<TKey> comparer)
        {
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        }

        public AvlTreeList()
            : this(Comparer<TKey>.Default)
        {
        }

        public int Count => _count;

        public Node Root => _root;

        public Node Head => _head;

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
                var node = FindNode(key);
                if (node == null)
                {
                    throw new KeyNotFoundException();
                }
                return node.Value;
            }
            set
            {
                FindOrCreateNode(key, out var node);
                node.Value = value;
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var node = FindNode(key);
            if (node != null)
            {
                value = node.Value;
                return true;
            }
            value = default;
            return false;
        }

        public bool ContainsKey(TKey key)
        {
            var node = FindNode(key);
            return node != null;
        }

        public bool ContainsValue(TValue value)
        {
            var node = _head;
            if (node != null)
            {
                if (value == null)
                {
                    do
                    {
                        if (node.Value == null)
                        {
                            return true;
                        }
                        node = node.Next;
                    } while (node != _head);
                }
                else
                {
                    var comparer = EqualityComparer<TValue>.Default;
                    do
                    {
                        if (comparer.Equals(node.Value, value))
                        {
                            return true;
                        }
                        node = node.Next;
                    } while (node != _head);
                }
            }
            return false;
        }

        public bool Add(TKey key, TValue value)
        {
            var found = FindOrCreateNode(key, out var node);
            node.Value = value;
            return !found;
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            if (valueFactory == null)
            {
                throw new ArgumentNullException(nameof(valueFactory));
            }
            var found = FindOrCreateNode(key, out var node);
            if (!found)
            {
                node.Value = valueFactory(key);
            }
            return node.Value;
        }

        public bool Remove(TKey key)
        {
            var node = FindNode(key);
            if (node != null)
            {
                InternalRemoveNode(node);
                return true;
            }
            return false;
        }

        public void Clear()
        {
            _root = null;
            _count = 0;
        }

        public Node FindNode(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            var node = _root;
            while (node != null)
            {
                var compare = _comparer.Compare(key, node.Key);
                if (compare < 0)
                {
                    node = node.Left;
                }
                else if (compare > 0)
                {
                    node = node.Right;
                }
                else
                {
                    return node;
                }
            }
            return null;
        }

        public bool FindOrCreateNode(TKey key, out Node node)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            var current = _root;
            while (current != null)
            {
                int compare = _comparer.Compare(key, current.Key);
                if (compare < 0)
                {
                    if (current.Left == null)
                    {
                        var left = new Node { Key = key, Parent = current };
                        // node should be inserted BEFORE current.
                        left.Next = current;
                        left.Prev = current.Prev;
                        current.Prev.Next = left;
                        current.Prev = left;
                        current.Left = left;
                        node = left;
                        InsertBalance(current, 1);
                        _count++;
                        return false;
                    }
                    current = current.Left;
                }
                else if (compare > 0)
                {
                    if (current.Right == null)
                    {
                        var right = new Node { Key = key, Parent = current };
                        // node should be inserted AFTER current.
                        right.Prev = current;
                        right.Next = current.Next;
                        current.Next.Prev = right;
                        current.Next = right;
                        current.Right = right;
                        node = right;
                        InsertBalance(current, -1);
                        _count++;
                        return false;
                    }
                    current = current.Right;
                }
                else
                {
                    node = current;
                    return true;
                }
            }
            var newNode = new Node { Key = key };
            newNode.Next = newNode;
            newNode.Prev = newNode;
            _root = newNode;
            _head = newNode;
            node = newNode;
            _count++;
            return false;
        }

        public Node GetLeftmostNode()
        {
            return _head;
        }

        public Node GetRightmostNode()
        {
            return _head?.Prev;
        }

        public void RemoveNode(Node node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }
            InternalRemoveNode(node);
        }

        private void InternalRemoveNode(Node node)
        {
            var left = node.Left;
            var right = node.Right;
            if (left == null)
            {
                if (right == null)
                {
                    if (node == _root)
                    {
                        _root = null;
                        _head = null;
                        _count = 0;
                        return;
                    }
                    else
                    {
                        var parent = node.Parent;
                        if (parent.Left == node)
                        {
                            parent.Left = null;
                            DeleteBalance(parent, -1);
                        }
                        else
                        {
                            parent.Right = null;
                            DeleteBalance(parent, 1);
                        }
                    }
                }
                else
                {
                    Replace(node, right);
                    DeleteBalance(node, 0);
                }
            }
            else if (right == null)
            {
                Replace(node, left);
                DeleteBalance(node, 0);
            }
            else
            {
                var successor = right;
                if (successor.Left == null)
                {
                    var parent = node.Parent;
                    successor.Parent = parent;
                    successor.Left = left;
                    successor.Balance = node.Balance;
                    left.Parent = successor;
                    if (node == _root)
                    {
                        _root = successor;
                    }
                    else
                    {
                        if (parent.Left == node)
                        {
                            parent.Left = successor;
                        }
                        else
                        {
                            parent.Right = successor;
                        }
                    }
                    DeleteBalance(successor, 1);
                }
                else
                {
                    while (successor.Left != null)
                    {
                        successor = successor.Left;
                    }
                    var parent = node.Parent;
                    var successorParent = successor.Parent;
                    var successorRight = successor.Right;
                    if (successorParent.Left == successor)
                    {
                        successorParent.Left = successorRight;
                    }
                    else
                    {
                        successorParent.Right = successorRight;
                    }
                    if (successorRight != null)
                    {
                        successorRight.Parent = successorParent;
                    }
                    successor.Parent = parent;
                    successor.Left = left;
                    successor.Balance = node.Balance;
                    successor.Right = right;
                    right.Parent = successor;
                    left.Parent = successor;
                    if (node == _root)
                    {
                        _root = successor;
                    }
                    else
                    {
                        if (parent.Left == node)
                        {
                            parent.Left = successor;
                        }
                        else
                        {
                            parent.Right = successor;
                        }
                    }
                    DeleteBalance(successorParent, -1);
                }
            }
            if (_head != null)
            {
                node.Next.Prev = node.Prev;
                node.Prev.Next = node.Next;
            }
            _count--;
        }

        private void InsertBalance(Node node, int balance)
        {
            while (node != null)
            {
                balance = node.Balance += balance;
                if (balance == 0)
                {
                    return;
                }
                if (balance == 2)
                {
                    if (node.Left.Balance == 1)
                    {
                        RotateRight(node);
                    }
                    else
                    {
                        RotateLeftRight(node);
                    }
                    return;
                }
                if (balance == -2)
                {
                    if (node.Right.Balance == -1)
                    {
                        RotateLeft(node);
                    }
                    else
                    {
                        RotateRightLeft(node);
                    }
                    return;
                }
                var parent = node.Parent;
                if (parent != null)
                {
                    balance = parent.Left == node ? 1 : -1;
                }
                node = parent;
            }
        }

        private Node RotateLeft(Node node)
        {
            var right = node.Right;
            var rightLeft = right.Left;
            var parent = node.Parent;

            right.Parent = parent;
            right.Left = node;
            node.Right = rightLeft;
            node.Parent = right;

            if (rightLeft != null)
            {
                rightLeft.Parent = node;
            }
            if (node == _root)
            {
                _root = right;
            }
            else if (parent.Right == node)
            {
                parent.Right = right;
            }
            else
            {
                parent.Left = right;
            }
            right.Balance++;
            node.Balance = -right.Balance;
            return right;
        }

        private Node RotateRight(Node node)
        {
            var left = node.Left;
            var leftRight = left.Right;
            var parent = node.Parent;

            left.Parent = parent;
            left.Right = node;
            node.Left = leftRight;
            node.Parent = left;

            if (leftRight != null)
            {
                leftRight.Parent = node;
            }
            if (node == _root)
            {
                _root = left;
            }
            else if (parent.Left == node)
            {
                parent.Left = left;
            }
            else
            {
                parent.Right = left;
            }
            left.Balance--;
            node.Balance = -left.Balance;
            return left;
        }

        private Node RotateLeftRight(Node node)
        {
            var left = node.Left;
            var leftRight = left.Right;
            var parent = node.Parent;
            var leftRightRight = leftRight.Right;
            var leftRightLeft = leftRight.Left;

            leftRight.Parent = parent;
            node.Left = leftRightRight;
            left.Right = leftRightLeft;
            leftRight.Left = left;
            leftRight.Right = node;
            left.Parent = leftRight;
            node.Parent = leftRight;

            if (leftRightRight != null)
            {
                leftRightRight.Parent = node;
            }
            if (leftRightLeft != null)
            {
                leftRightLeft.Parent = left;
            }
            if (node == _root)
            {
                _root = leftRight;
            }
            else if (parent.Left == node)
            {
                parent.Left = leftRight;
            }
            else
            {
                parent.Right = leftRight;
            }
            if (leftRight.Balance == -1)
            {
                node.Balance = 0;
                left.Balance = 1;
            }
            else if (leftRight.Balance == 0)
            {
                node.Balance = 0;
                left.Balance = 0;
            }
            else
            {
                node.Balance = -1;
                left.Balance = 0;
            }
            leftRight.Balance = 0;
            return leftRight;
        }

        private Node RotateRightLeft(Node node)
        {
            var right = node.Right;
            var rightLeft = right.Left;
            var parent = node.Parent;
            var rightLeftLeft = rightLeft.Left;
            var rightLeftRight = rightLeft.Right;

            rightLeft.Parent = parent;
            node.Right = rightLeftLeft;
            right.Left = rightLeftRight;
            rightLeft.Right = right;
            rightLeft.Left = node;
            right.Parent = rightLeft;
            node.Parent = rightLeft;

            if (rightLeftLeft != null)
            {
                rightLeftLeft.Parent = node;
            }
            if (rightLeftRight != null)
            {
                rightLeftRight.Parent = right;
            }
            if (node == _root)
            {
                _root = rightLeft;
            }
            else if (parent.Right == node)
            {
                parent.Right = rightLeft;
            }
            else
            {
                parent.Left = rightLeft;
            }
            if (rightLeft.Balance == 1)
            {
                node.Balance = 0;
                right.Balance = -1;
            }
            else if (rightLeft.Balance == 0)
            {
                node.Balance = 0;
                right.Balance = 0;
            }
            else
            {
                node.Balance = 1;
                right.Balance = 0;
            }
            rightLeft.Balance = 0;
            return rightLeft;
        }

        private void DeleteBalance(Node node, int balance)
        {
            while (node != null)
            {
                balance = node.Balance += balance;
                if (balance == 2)
                {
                    if (node.Left.Balance >= 0)
                    {
                        node = RotateRight(node);
                        if (node.Balance == -1)
                        {
                            return;
                        }
                    }
                    else
                    {
                        node = RotateLeftRight(node);
                    }
                }
                else if (balance == -2)
                {
                    if (node.Right.Balance <= 0)
                    {
                        node = RotateLeft(node);
                        if (node.Balance == 1)
                        {
                            return;
                        }
                    }
                    else
                    {
                        node = RotateRightLeft(node);
                    }
                }
                else if (balance != 0)
                {
                    return;
                }
                var parent = node.Parent;
                if (parent != null)
                {
                    balance = parent.Left == node ? -1 : 1;
                }
                node = parent;
            }
        }

        private static void Replace(Node target, Node source)
        {
            var left = source.Left;
            var right = source.Right;

            target.Balance = source.Balance;
            target.Key = source.Key;
            target.Value = source.Value;
            target.Left = left;
            target.Right = right;

            if (left != null)
            {
                left.Parent = target;
            }
            if (right != null)
            {
                right.Parent = target;
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_head);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(_head);
        }

        private void InOrderTreeWalk(Func<Node, bool> callback)
        {
            var node = _head;
            if (node != null)
            {
                do
                {
                    if (!callback(node))
                    {
                        return;
                    }
                    node = node.Next;
                } while (node != _head);
            }
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
            var node = FindNode(item.Key);
            if (node != null && EqualityComparer<TValue>.Default.Equals(item.Value, node.Value))
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
            if (array.Length - index < _count)
            {
                throw new ArgumentException(nameof(index));
            }
            InOrderTreeWalk(node =>
            {
                array[index++] = new KeyValuePair<TKey, TValue>(node.Key, node.Value);
                return true;
            });
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            var node = FindNode(item.Key);
            if (node != null && EqualityComparer<TValue>.Default.Equals(item.Value, node.Value))
            {
                InternalRemoveNode(node);
                return true;
            }
            return false;
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return new Enumerator(_head);
        }

        public class Node
        {
            public Node Parent { get; internal set; }
            public Node Left { get; internal set; }
            public Node Right { get; internal set; }
            public Node Prev { get; internal set; }
            public Node Next { get; internal set; }
            public TKey Key { get; internal set; }
            public TValue Value { get; internal set; }
            public int Balance { get; internal set; }
        }

        public struct Enumerator: IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private readonly Node _head;
            private Node _node;
            private KeyValuePair<TKey, TValue> _current;
            private bool _hasValue;

            internal Enumerator(Node head)
            {
                _head = head;
                _node = head;
                _current = default;
                _hasValue = false;
            }

            public bool MoveNext()
            {
                if (_node == null)
                {
                    _hasValue = false;
                    return false;
                }
                _current = new KeyValuePair<TKey, TValue>(_node.Key, _node.Value);
                _node = _node.Next;
                _hasValue = true;
                if (ReferenceEquals(_node, _head))
                {
                    _node = null;
                }
                return true;
            }

            public void Reset()
            {
                _node = _head;
                _current = default;
                _hasValue = false;
            }

            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    if (!_hasValue)
                    {
                        throw new InvalidOperationException();
                    }
                    return _current;
                }
            }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }
        }

        public sealed class KeyCollection: ICollection<TKey>
        {
            private readonly AvlTreeList<TKey, TValue> _tree;

            public KeyCollection(AvlTreeList<TKey, TValue> tree)
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
                _tree.InOrderTreeWalk(node =>
                {
                    array[index++] = node.Key;
                    return true;
                });
            }

            public bool Remove(TKey item)
            {
                throw new NotSupportedException();
            }

            public KeyEnumerator GetEnumerator()
            {
                return new KeyEnumerator(_tree.Head);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new KeyEnumerator(_tree.Head);
            }

            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
            {
                return new KeyEnumerator(_tree.Head);
            }

            public struct KeyEnumerator: IEnumerator<TKey>
            {
                private readonly Node _head;
                private Node _node;
                private TKey _current;
                private bool _hasValue;

                internal KeyEnumerator(Node head)
                {
                    _head = head;
                    _node = head;
                    _current = default;
                    _hasValue = false;
                }

                public bool MoveNext()
                {
                    if (_node == null)
                    {
                        _hasValue = false;
                        return false;
                    }
                    _current = _node.Key;
                    _node = _node.Next;
                    _hasValue = true;
                    if (ReferenceEquals(_node, _head))
                    {
                        _node = null;
                    }
                    return true;
                }

                public void Reset()
                {
                    _node = _head;
                    _current = default;
                    _hasValue = false;
                }

                public TKey Current
                {
                    get
                    {
                        if (!_hasValue)
                        {
                            throw new InvalidOperationException();
                        }
                        return _current;
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
            private readonly AvlTreeList<TKey, TValue> _tree;

            public ValueCollection(AvlTreeList<TKey, TValue> tree)
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
                _tree.InOrderTreeWalk(node =>
                {
                    array[index++] = node.Value;
                    return true;
                });
            }

            public bool Remove(TValue item)
            {
                throw new NotSupportedException();
            }

            public ValueEnumerator GetEnumerator()
            {
                return new ValueEnumerator(_tree.Head);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new ValueEnumerator(_tree.Head);
            }

            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
            {
                return new ValueEnumerator(_tree.Head);
            }

            public struct ValueEnumerator: IEnumerator<TValue>
            {
                private readonly Node _head;
                private Node _node;
                private TValue _current;
                private bool _hasValue;

                internal ValueEnumerator(Node head)
                {
                    _head = head;
                    _node = head;
                    _current = default;
                    _hasValue = false;
                }

                public bool MoveNext()
                {
                    if (_node == null)
                    {
                        _hasValue = false;
                        return false;
                    }
                    _current = _node.Value;
                    _node = _node.Next;
                    _hasValue = true;
                    if (ReferenceEquals(_node, _head))
                    {
                        _node = null;
                    }
                    return true;
                }

                public void Reset()
                {
                    _node = _head;
                    _current = default;
                    _hasValue = false;
                }

                public TValue Current
                {
                    get
                    {
                        if (!_hasValue)
                        {
                            throw new InvalidOperationException();
                        }
                        return _current;
                    }
                }

                object IEnumerator.Current => Current;

                public void Dispose()
                {
                }
            }
        }
    }
}
