using System;
using System.Collections;
using System.Collections.Generic;

namespace DataStructures
{
    public class AvlTree<T>: ICollection<T>, IReadOnlyCollection<T>
    {
        private readonly IComparer<T> _comparer;
        private Node _root;
        private int _count;

        public AvlTree()
        {
            _comparer = Comparer<T>.Default;
        }

        public AvlTree(IComparer<T> comparer)
        {
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        }

        public int Count => _count;

        public bool IsReadOnly => false;

        public IComparer<T> Comparer => _comparer;

#if DEBUG
        internal Node Root => _root;
#endif
        public void Add(T item)
        {
            _ = FindOrCreateNode(item, out var _);
        }

        public void Clear()
        {
            _root = null;
            _count = 0;
        }

        public bool Contains(T item)
        {
            return FindNode(item) != null;
        }

        public void CopyTo(T[] array, int index)
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
            var walker = new TreeWalker(this);
            while (walker.MoveNext())
            {
                var node = walker.Current;
                array[index++] = node.Item;
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Remove(T item)
        {
            var node = FindNode(item);
            if (node != null)
            {
                InternalRemoveNode(node);
                return true;
            }
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Node FindNode(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            var node = _root;
            while (node != null)
            {
                var compare = _comparer.Compare(item, node.Item);
                if (compare == 0)
                {
                    return node;
                }
                node = compare < 0 ? node.Left : node.Right;
            }
            return null;
        }

        public bool FindOrCreateNode(T item, out Node node)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            var current = _root;
            while (current != null)
            {
                int compare = _comparer.Compare(item, current.Item);
                if (compare < 0)
                {
                    if (current.Left == null)
                    {
                        current.Left = new Node { Item = item, Parent = current };
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
                        current.Right = new Node { Item = item, Parent = current };
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
            _root = new Node { Item = item };
            node = _root;
            _count++;
            return false;
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
                        _ = RotateRight(node);
                    }
                    else
                    {
                        _ = RotateLeftRight(node);
                    }
                    return;
                }
                if (balance == -2)
                {
                    if (node.Right.Balance == -1)
                    {
                        _ = RotateLeft(node);
                    }
                    else
                    {
                        _ = RotateRightLeft(node);
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
            target.Item = source.Item;
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

        internal struct TreeWalker
        {
            private readonly Node _root;
            private Node _cursor;
            private Node _current;

            public TreeWalker(AvlTree<T> tree)
            {
                _root = tree._root;
                _cursor = tree._root;
                _current = null;
                GoDeepLeft();
            }

            public Node Current => _current;

            public bool MoveNext()
            {
                if (_cursor == null)
                {
                    _current = null;
                    return false;
                }
                if (_current == null)
                {
                    _current = _cursor;
                    return true;
                }
                if (_cursor.Right != null)
                {
                    _cursor = _cursor.Right;
                    while (_cursor.Left != null)
                    {
                        _cursor = _cursor.Left;
                    }
                    _current = _cursor;
                    return true;
                }
                while (_cursor.Parent != null)
                {
                    var prev = _cursor;
                    _cursor = _cursor.Parent;
                    if (_cursor.Left == prev)
                    {
                        _current = _cursor;
                        return true;
                    }
                }
                _current = null;
                _cursor = null;
                return false;
            }

            public void Reset()
            {
                _current = null;
                _cursor = _root;
                GoDeepLeft();
            }

            private void GoDeepLeft()
            {
                var node = _cursor;
                while (node != null)
                {
                    _cursor = node;
                    node = node.Left;
                }
            }
        }

        public struct Enumerator: IEnumerator<T>
        {
            private TreeWalker _walker;

            public Enumerator(AvlTree<T> tree)
            {
                _walker = new TreeWalker(tree);
            }

            public bool MoveNext()
            {
                return _walker.MoveNext();
            }

            public void Reset()
            {
                _walker.Reset();
            }

            public T Current
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

        public class Node
        {
            public Node Parent { get; internal set; }
            public Node Left { get; internal set; }
            public Node Right { get; internal set; }
            public T Item { get; internal set; }
            public int Balance { get; internal set; }
        }
    }
}
