using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DataStructures
{
    /// <summary>
    /// Avl Tree with parentless node
    /// </summary>
    public class AvlTree2<T>
    {
        private readonly IComparer<T> _comparer;
        private Node _root;

        public AvlTree2()
        {
            _comparer = Comparer<T>.Default;
        }

        public AvlTree2(IComparer<T> comparer)
        {
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        }

        public bool Add(T value)
        {
            return InternalAdd(ref _root, value);
        }

        public bool Remove(T value)
        {
            return InternalRemove(ref _root, value);
        }

        public bool Contains(T value)
        {
            var node = _root;
            while (node != null)
            {
                int compare = _comparer.Compare(value, node.Value);
                if (compare == 0)
                {
                    return true;
                }
                node = compare > 0 ? node.Right : node.Left;
            }
            return false;
        }

        private bool InternalAdd(ref Node root, T value)
        {
            var node = root;
            ref var parent = ref root;
            while (node != null)
            {
                var compare = _comparer.Compare(value, node.Value);
                if (compare == 0)
                {
                    return false;
                }
                if (!node.IsBalanced)
                {
                    parent = ref root;
                }
                root = ref compare > 0 ? ref node.Right : ref node.Left;
                node = root;
            }
            node = new Node { Value = value, Longer = Direction.None };
            root = node;
            RebalanceInsert(ref parent, value);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Direction GetDirection(int value)
        {
            return (Direction) (int) (((uint) -value >> 31) - ((uint) value >> 31));
        }

        private bool InternalRemove(ref Node root, T value)
        {
            Node target = null;
            ref var parent = ref root;
            ref var tree = ref root;
            var dir = Direction.None;
            var node = tree;
            while (node != null)
            {
                var compare = _comparer.Compare(value, node.Value);
                if (compare == 0)
                {
                    target = tree;
                }
                if (compare > 0)
                {
                    dir = Direction.Right;
                    if (node.Right == null)
                    {
                        break;
                    }
                    if (node.IsBalanced || (node.Longer == Direction.Left && node.Left.IsBalanced))
                    {
                        parent = ref tree;
                    }
                    tree = ref node.Right;
                }
                else
                {
                    dir = Direction.Left;
                    if (node.Left == null)
                    {
                        break;
                    }
                    if (node.IsBalanced || (node.Longer == Direction.Right && node.Right.IsBalanced))
                    {
                        parent = ref tree;
                    }
                    tree = ref node.Left;
                }
                node = tree;
            }
            if (target == null)
            {
                return false;
            }
            var (n, d) = RebalanceDelete(ref parent, target, value);
            if (d == Direction.Left)
            {
                SwapDelete(ref n.Left, ref root, dir);
            }
            return true;
        }

        private (Node, Direction) RebalanceDelete(ref Node root, Node target, T value)
        {
            var targetNode = target;
            var targetDir = Direction.None;
            while (true)
            {
                var node = root;
                Direction dir;
                if (_comparer.Compare(value, node.Value) > 0)
                {
                    if (node.Right == null)
                    {
                        break;
                    }
                    dir = Direction.Right;
                }
                else
                {
                    if (node.Left == null)
                    {
                        break;
                    }
                    dir = Direction.Left;
                }
                var oppositeDir = (Direction) (-(int) dir);
                if (node.IsBalanced)
                {
                    node.Longer = oppositeDir;
                }
                else if (node.Longer == dir)
                {
                    node.Longer = Direction.None;
                }
                else
                {
                    var second = dir > 0 ? node.Left.Longer : node.Right.Longer;
                    if (second == dir)
                    {
                        var third = dir > 0 ? node.Left.Right.Longer : node.Right.Left.Longer;
                        Rotate3(ref root, oppositeDir, third);
                    }
                    else if (second == Direction.None)
                    {
                        Rotate2(ref root, oppositeDir);
                        node.Longer = oppositeDir;
                        root.Longer = dir;
                    }
                    else
                    {
                        Rotate2(ref root, oppositeDir);
                    }
                    if (node == targetNode)
                    {
                        target = root;
                        targetDir = dir;
                    }
                }
                root = ref dir > 0 ? ref node.Right : ref node.Left;
            }
            return (target, targetDir);
        }

        private static void SwapDelete(ref Node target, ref Node tree, Direction dir)
        {
            var targetNode = target;
            var treeNode = tree;

            target = treeNode;
            tree = dir > 0 ? treeNode.Left : treeNode.Right;
            treeNode.Left = targetNode.Left;
            treeNode.Right = targetNode.Right;
            treeNode.Longer = targetNode.Longer;
        }

        private void RebalanceInsert(ref Node parent, T value)
        {
            var path = parent;
            Direction first, second;
            if (!path.IsBalanced)
            {
                first = GetDirection(_comparer.Compare(value, path.Value));
                if (path.Longer != first)
                {
                    path.Longer = Direction.None;
                    path = first > 0 ? path.Right : path.Left;
                }
                else
                {
                    var node = first > 0 ? path.Right : path.Left;
                    second = GetDirection(_comparer.Compare(value, node.Value));
                    if (first == second)
                    {
                        path = Rotate2(ref parent, first);
                    }
                    else
                    {
                        path = first > 0 ? path.Right : path.Left;
                        path = second > 0 ? path.Right : path.Left;
                        var third = GetDirection(_comparer.Compare(value, path.Value));
                        path = Rotate3(ref parent, first, third);
                    }
                }
            }
            RebalancePath(path, value);
        }

        private static Node Rotate2(ref Node parent, Direction dir)
        {
            var b = parent;

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

            parent = d;

            b.Longer = Direction.None;
            d.Longer = Direction.None;

            return e;
        }

        private static Node Rotate3(ref Node parent, Direction dir, Direction third)
        {
            var b = parent;

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

            parent = d;

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

        private void RebalancePath(Node path, T value)
        {
            while (path != null)
            {
                var compare = _comparer.Compare(value, path.Value);
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

            internal T Value { get; set; }

            internal Direction Longer { get; set; }

            internal bool IsBalanced => Longer == Direction.None;
        }
    }
}
