using System;
using System.Collections.Generic;

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

        private bool Insert(Node root, T value)
        {
            var node = root;
            var parent = root;
            while (node != null)
            {
                var compare = _comparer.Compare(value, node.Value);
                if (compare == 0)
                {
                    return false;
                }
                if (!node.IsBalanced)
                {

                }
            }
            return true;
        }

        private void RebalanceInsert(ref Node parent, T value)
        {
            var path = parent;
            Direction first, second;
            if (!path.IsBalanced)
            {
                first = _comparer.Compare(value, path.Value) > 0 ? Direction.Right : Direction.Left;
                if (path.Longer != first)
                {
                    path.Longer = Direction.None;
                    path = first > 0 ? path.Right : path.Left;
                }
                else
                {
                    var node = first > 0 ? path.Right : path.Left;
                    second = _comparer.Compare(value, node.Value) > 0 ? Direction.Right : Direction.Left;
                    if (first == second)
                    {
                        path = Rotate2(ref parent, first);
                    }
                    else
                    {
                        path = first > 0 ? path.Right : path.Left;
                        path = second > 0 ? path.Right : path.Left;
                        var temp = _comparer.Compare(value, path.Value);
                        var third = temp switch
                        {
                            var x when x > 0 => Direction.Right,
                            var x when x < 0 => Direction.Left,
                            _ => Direction.None
                        };
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
                b.Longer = dir > 0 ? Direction.Left : Direction.Right;
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
