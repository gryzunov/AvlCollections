using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AvlCollections
{
    /// <summary>
    /// Avl Tree with parentless node
    /// </summary>
    public class CompactAvlTree<T>: ICollection<T>, IReadOnlyCollection<T>
    {
        private static Node _dummyNode = null;
        private readonly IComparer<T> _comparer;
        private Node _root;
        private int _count;

        public CompactAvlTree()
        {
            _comparer = Comparer<T>.Default;
        }

        public CompactAvlTree(IComparer<T> comparer)
        {
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        }

#if DEBUG
        internal Node Root => _root;
#endif

        public int Count => _count;

        public bool IsReadOnly => false;

        public void Add(T item)
        {
            _ = InternalAdd(ref _root, item);
        }

        public bool TryAdd(T item)
        {
            return InternalAdd(ref _root, item);
        }

        public bool Remove(T item)
        {
            return InternalRemove(ref _root, item);
        }

        public bool Contains(T item)
        {
            var node = _root;
            while (node != null)
            {
                int compare = _comparer.Compare(item, node.Item);
                if (compare == 0)
                {
                    return true;
                }
                node = compare > 0 ? node.Right : node.Left;
            }
            return false;
        }

        public void Clear()
        {
            _count = 0;
            _root = null;
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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private bool InternalAdd(ref Node treeRef, T item)
        {
            var tree = treeRef;
            ref var pathTopRef = ref treeRef;
            while (tree != null)
            {
                var compare = _comparer.Compare(item, tree.Item);
                if (compare == 0)
                {
                    return false;
                }
                if (!tree.IsBalanced)
                {
                    pathTopRef = ref treeRef;
                }
                treeRef = ref compare > 0 ? ref tree.Right : ref tree.Left;
                tree = treeRef;
            }
            tree = new Node { Item = item, Longer = Direction.None };
            treeRef = tree;
            RebalanceInsert(ref pathTopRef, item);
            ++_count;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Direction GetDirection(int value)
        {
            // effectively does sign(value) and cast to Direction.
            return (Direction) unchecked(value >> 31 | (int) ((uint) -value >> 31));
        }

        private bool InternalRemove(ref Node treeRef, T item)
        {
            ref var targetRef = ref _dummyNode;
            ref var pathTopRef = ref treeRef;
            var dir = Direction.None;
            var tree = treeRef;
            while (tree != null)
            {
                var compare = _comparer.Compare(item, tree.Item);
                if (compare == 0)
                {
                    targetRef = ref treeRef;
                }
                if (compare > 0)
                {
                    dir = Direction.Right;
                    if (tree.Right == null)
                    {
                        break;
                    }
                    if (tree.IsBalanced || (tree.Longer == Direction.Left && tree.Left.IsBalanced))
                    {
                        pathTopRef = ref treeRef;
                    }
                    treeRef = ref tree.Right;
                }
                else
                {
                    dir = Direction.Left;
                    if (tree.Left == null)
                    {
                        break;
                    }
                    if (tree.IsBalanced || (tree.Longer == Direction.Right && tree.Right.IsBalanced))
                    {
                        pathTopRef = ref treeRef;
                    }
                    treeRef = ref tree.Left;
                }
                tree = treeRef;
            }
            if (targetRef == null)
            {
                return false;
            }
            var (n, d) = RebalanceDelete(ref pathTopRef, targetRef, item);
            if (d == Direction.Right)
            {
                SwapDelete(ref n.Right, ref treeRef, dir);
            }
            else if (d == Direction.Left)
            {
                SwapDelete(ref n.Left, ref treeRef, dir);
            }
            else
            {
                SwapDelete(ref targetRef, ref treeRef, dir);
            }
            --_count;
            return true;
        }

        private (Node, Direction) RebalanceDelete(ref Node treeRef, Node target, T item)
        {
            var targetNode = target;
            var targetDir = Direction.None;
            while (true)
            {
                var tree = treeRef;
                Direction dir;
                if (_comparer.Compare(item, tree.Item) > 0)
                {
                    if (tree.Right == null)
                    {
                        break;
                    }
                    dir = Direction.Right;
                }
                else
                {
                    if (tree.Left == null)
                    {
                        break;
                    }
                    dir = Direction.Left;
                }
                var oppositeDir = (Direction) (-(int) dir);
                if (tree.IsBalanced)
                {
                    tree.Longer = oppositeDir;
                }
                else if (tree.Longer == dir)
                {
                    tree.Longer = Direction.None;
                }
                else
                {
                    var second = dir > 0 ? tree.Left.Longer : tree.Right.Longer;
                    if (second == dir)
                    {
                        var third = dir > 0 ? tree.Left.Right.Longer : tree.Right.Left.Longer;
                        Rotate3(ref treeRef, oppositeDir, third);
                    }
                    else if (second == Direction.None)
                    {
                        Rotate2(ref treeRef, oppositeDir);
                        tree.Longer = oppositeDir;
                        treeRef.Longer = dir;
                    }
                    else
                    {
                        Rotate2(ref treeRef, oppositeDir);
                    }
                    if (tree == targetNode)
                    {
                        target = treeRef;
                        targetDir = dir;
                    }
                }
                treeRef = ref dir > 0 ? ref tree.Right : ref tree.Left;
            }
            return (target, targetDir);
        }

        private static void SwapDelete(ref Node targetRef, ref Node treeRef, Direction dir)
        {
            var targetNode = targetRef;
            var treeNode = treeRef;

            targetRef = treeNode;
            treeRef = dir > 0 ? treeNode.Left : treeNode.Right;
            treeNode.Left = targetNode.Left;
            treeNode.Right = targetNode.Right;
            treeNode.Longer = targetNode.Longer;
        }

        private void RebalanceInsert(ref Node pathTopRef, T item)
        {
            var path = pathTopRef;
            Direction first, second;
            if (!path.IsBalanced)
            {
                first = GetDirection(_comparer.Compare(item, path.Item));
                if (path.Longer != first)
                {
                    path.Longer = Direction.None;
                    path = first > 0 ? path.Right : path.Left;
                }
                else
                {
                    var node = first > 0 ? path.Right : path.Left;
                    second = GetDirection(_comparer.Compare(item, node.Item));
                    if (first == second)
                    {
                        path = Rotate2(ref pathTopRef, first);
                    }
                    else
                    {
                        path = first > 0 ? path.Right : path.Left;
                        path = second > 0 ? path.Right : path.Left;
                        var third = GetDirection(_comparer.Compare(item, path.Item));
                        path = Rotate3(ref pathTopRef, first, third);
                    }
                }
            }
            RebalancePath(path, item);
        }

        private static Node Rotate2(ref Node pathTopRef, Direction dir)
        {
            var b = pathTopRef;

            Node c, d, e;
            if (dir > 0)
            {
                d = b.Right;
                c = d.Left;
                e = d.Right;
                d.Left = b;
                b.Right = c;
            }
            else
            {
                d = b.Left;
                c = d.Right;
                e = d.Left;
                d.Right = b;
                b.Left = c;
            }

            pathTopRef = d;

            b.Longer = Direction.None;
            d.Longer = Direction.None;

            return e;
        }

        private static Node Rotate3(ref Node pathTopRef, Direction dir, Direction third)
        {
            var b = pathTopRef;

            Node c, d, e, f;
            if (dir > 0)
            {
                f = b.Right;
                d = f.Left;
                c = d.Left;
                e = d.Right;

                d.Left = b;
                d.Right = f;
                b.Right = c;
                f.Left = e;
            }
            else
            {
                f = b.Left;
                d = f.Right;
                c = d.Right;
                e = d.Left;

                d.Right = b;
                d.Left = f;
                b.Left = c;
                f.Right = e;
            }

            pathTopRef = d;

            d.Longer = Direction.None;
            b.Longer = Direction.None;
            f.Longer = Direction.None;

            if (third == Direction.None)
            {
                return null;
            }
            if (dir == third)
            {
                b.Longer = (Direction) (-(int) dir);
                return e;
            }
            f.Longer = dir;
            return c;
        }

        private void RebalancePath(Node path, T item)
        {
            while (path != null)
            {
                var compare = _comparer.Compare(item, path.Item);
                if (compare == 0)
                {
                    return;
                }
                else if (compare > 0)
                {
                    path.Longer = Direction.Right;
                    path = path.Right;
                }
                else
                {
                    path.Longer = Direction.Left;
                    path = path.Left;
                }
            }
        }

        public struct Enumerator: IEnumerator<T>
        {
            private TreeWalker _walker;

            internal Enumerator(CompactAvlTree<T> tree)
            {
                _walker = new TreeWalker(tree);
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

            public bool MoveNext()
            {
                return _walker.MoveNext();
            }

            public void Reset()
            {
                _walker.Reset();
            }
        }

        internal struct TreeWalker
        {
            private readonly Node _root;
            private readonly Stack<Node> _stack;
            private Node _current;

            public TreeWalker(CompactAvlTree<T> tree)
            {
                _current = null;
                _root = tree._root;
                if (tree._root == null)
                {
                    _stack = null;
                }
                else
                {
                    _stack = new Stack<Node>(2 * Log2(tree.Count));
                    InitStack();
                }
            }

            public Node Current => _current;

            public bool MoveNext()
            {
                if (_stack.Count == 0)
                {
                    _current = null;
                    return false;
                }
                _current = _stack.Pop();
                var node = _current.Right;
                while (node != null)
                {
                    _stack.Push(node);
                    node = node.Left;
                }
                return true;
            }

            public void Reset()
            {
                _current = null;
                _stack.Clear();
                InitStack();
            }

            private void InitStack()
            {
                var node = _root;
                while (node != null)
                {
                    _stack.Push(node);
                    node = node.Left;
                }
            }

            private static int Log2(int value)
            {
                int result = 0;
                while (value > 0)
                {
                    result++;
                    value >>= 1;
                }
                return result;
            }
        }

        internal enum Direction
        {
            Left = -1,
            None = 0,
            Right = 1
        }

        internal sealed class Node
        {
            internal Node Left;

            internal Node Right;

            internal T Item { get; set; }

            internal Direction Longer { get; set; }

            internal bool IsBalanced => Longer == Direction.None;
        }
    }
}
