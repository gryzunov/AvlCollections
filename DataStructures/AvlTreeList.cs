﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DataStructures
{
    public class AvlTreeList<TKey, TValue>: IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    {
        private readonly IComparer<TKey> _comparer;
        private Node _root;
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
            value = default(TValue);
            return false;
        }

        public bool ContainsKey(TKey key)
        {
            var node = FindNode(key);
            return node != null;
        }

        public bool ContainsValue(TValue value)
        {
            var found = false;
            if (value == null)
            {
                InOrderTreeWalk(node =>
                {
                    if (node.Value == null)
                    {
                        found = true;
                        return false; // stop tree walk
                    }
                    return true;
                });
            }
            else
            {
                var comparer = EqualityComparer<TValue>.Default;
                InOrderTreeWalk(node =>
                {
                    if (comparer.Equals(node.Value, value))
                    {
                        found = true;
                        return false; // stop tree walk
                    }
                    return true;
                });
            }
            return found;
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
                        current.Left = new Node { Key = key, Parent = current };
                        node = current.Left;
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
                        current.Right = new Node { Key = key, Parent = current };
                        node = current.Right;
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
            _root = new Node { Key = key };
            node = _root;
            _count++;
            return false;
        }

        public Node GetLeftmostNode()
        {
            if (_root == null)
            {
                return null;
            }
            var current = _root;
            while (current.Left != null)
            {
                current = current.Left;
            }
            return current;
        }

        public Node GetRightmostNode()
        {
            if (_root == null)
            {
                return null;
            }
            var current = _root;
            while (current.Right != null)
            {
                current = current.Right;
            }
            return current;
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
            return new Enumerator(_root);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(_root);
        }

        private void InOrderTreeWalk(Func<Node, bool> callback)
        {
            if (_root == null)
            {
                return;
            }
            Node current = null;
            var right = _root;
            var action = Action.Right;
            while (true)
            {
                switch (action)
                {
                    case Action.Right:
                        current = right;
                        Debug.Assert(current != null, "current != null");
                        while (current.Left != null)
                        {
                            current = current.Left;
                        }
                        right = current.Right;
                        action = right != null ? Action.Right : Action.Parent;
                        if (!callback(current))
                        {
                            return;
                        }
                        break;
                    case Action.Parent:
                        Debug.Assert(current != null, "current != null");
                        if (current.Parent == null)
                        {
                            return;
                        }
                        var previous = current;
                        current = current.Parent;
                        if (current.Left == previous)
                        {
                            right = current.Right;
                            action = right != null ? Action.Right : Action.Parent;
                            if (!callback(current))
                            {
                                return;
                            }
                        }
                        break;
                    default:
                        return;
                }
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
            return new Enumerator(_root);
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
            private readonly Node _root;
            private Node _current;
            private Node _right;
            private Action _action;

            public Enumerator(Node root)
            {
                _root = root;
                _right = root;
                _current = null;
                _action = _root == null ? Action.End : Action.Right;
            }

            public bool MoveNext()
            {
                switch (_action)
                {
                    case Action.Right:
                        _current = _right;
                        while (_current.Left != null)
                        {
                            _current = _current.Left;
                        }
                        _right = _current.Right;
                        _action = _right != null ? Action.Right : Action.Parent;
                        return true;
                    case Action.Parent:
                        while (_current.Parent != null)
                        {
                            var previous = _current;
                            _current = _current.Parent;
                            if (_current.Left == previous)
                            {
                                _right = _current.Right;
                                _action = _right != null ? Action.Right : Action.Parent;
                                return true;
                            }
                        }
                        _action = Action.End;
                        return false;
                    default:
                        return false;
                }
            }

            public void Reset()
            {
                _right = _root;
                _action = _root == null ? Action.End : Action.Right;
            }

            public KeyValuePair<TKey, TValue> Current => new KeyValuePair<TKey, TValue>(_current.Key, _current.Value);

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
                return new KeyEnumerator(_tree.Root);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new KeyEnumerator(_tree.Root);
            }

            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
            {
                return new KeyEnumerator(_tree.Root);
            }

            public struct KeyEnumerator: IEnumerator<TKey>
            {
                private readonly Node _root;
                private Node _current;
                private Node _right;
                private Action _action;

                public KeyEnumerator(Node root)
                {
                    _root = root;
                    _right = root;
                    _current = null;
                    _action = _root == null ? Action.End : Action.Right;
                }

                public bool MoveNext()
                {
                    switch (_action)
                    {
                        case Action.Right:
                            _current = _right;
                            while (_current.Left != null)
                            {
                                _current = _current.Left;
                            }
                            _right = _current.Right;
                            _action = _right != null ? Action.Right : Action.Parent;
                            return true;
                        case Action.Parent:
                            while (_current.Parent != null)
                            {
                                var previous = _current;
                                _current = _current.Parent;
                                if (_current.Left == previous)
                                {
                                    _right = _current.Right;
                                    _action = _right != null ? Action.Right : Action.Parent;
                                    return true;
                                }
                            }
                            _action = Action.End;
                            return false;
                        default:
                            return false;
                    }
                }

                public void Reset()
                {
                    _right = _root;
                    _action = _root == null ? Action.End : Action.Right;
                }

                public TKey Current => _current.Key;

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
                return new ValueEnumerator(_tree.Root);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new ValueEnumerator(_tree.Root);
            }

            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
            {
                return new ValueEnumerator(_tree.Root);
            }

            public struct ValueEnumerator: IEnumerator<TValue>
            {
                private readonly Node _root;
                private Node _current;
                private Node _right;
                private Action _action;

                public ValueEnumerator(Node root)
                {
                    _root = root;
                    _right = root;
                    _current = null;
                    _action = _root == null ? Action.End : Action.Right;
                }

                public bool MoveNext()
                {
                    switch (_action)
                    {
                        case Action.Right:
                            _current = _right;
                            while (_current.Left != null)
                            {
                                _current = _current.Left;
                            }
                            _right = _current.Right;
                            _action = _right != null ? Action.Right : Action.Parent;
                            return true;
                        case Action.Parent:
                            while (_current.Parent != null)
                            {
                                var previous = _current;
                                _current = _current.Parent;
                                if (_current.Left == previous)
                                {
                                    _right = _current.Right;
                                    _action = _right != null ? Action.Right : Action.Parent;
                                    return true;
                                }
                            }
                            _action = Action.End;
                            return false;
                        default:
                            return false;
                    }
                }

                public void Reset()
                {
                    _right = _root;
                    _action = _root == null ? Action.End : Action.Right;
                }

                public TValue Current => _current.Value;

                object IEnumerator.Current => Current;

                public void Dispose()
                {
                }
            }
        }

        private enum Action
        {
            Parent,
            Right,
            End
        }
    }
}
