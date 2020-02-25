using System;
using Xunit;

namespace AvlCollections.Tests
{
    public class CompactAvlTreeTests
    {
        [Fact]
        public void TestInsert()
        {
            var tree = new CompactAvlTree<int>();
            for (int i = 1; i <= 100; i++)
            {
                Assert.True(tree.TryAdd(i));
            }
        }

        [Fact]
        public void TestDelete()
        {
            var tree = new CompactAvlTree<int>();
            for (int i = 1; i <= 100; i++)
            {
                tree.Add(i);
            }
            Assert.False(tree.Remove(1000));
            for (int i = 1; i <= 100; i++)
            {
                Assert.True(tree.Remove(i));
            }
        }

        [Fact]
        public void InsertBalanceTest()
        {
            var tree = new CompactAvlTree<int>();
            for (int i = 1; i <= 100; i++)
            {
                tree.Add(i);
                CheckBalance(tree);
            }
        }

        [Fact]
        public void DeleteBalanceTest()
        {
            var tree = new CompactAvlTree<int>();
            for (int i = 1; i <= 100; i++)
            {
                tree.Add(i);
            }
            for (int i = 1; i <= 100; i++)
            {
                tree.Remove(i);
                CheckBalance(tree);
            }
        }

        [Fact]
        public void CopyToArrayTest()
        {
            var tree = new CompactAvlTree<int>();
            var src = new int[100];
            for (int i = 0; i < src.Length; i++)
            {
                src[i] = i + 1;
                tree.Add(i + 1);
            }

            var dest = new int[100];
            tree.CopyTo(dest, 0);
            Assert.Equal(src, dest);
        }

        private void CheckBalance(CompactAvlTree<int> tree)
        {
#if DEBUG
            CheckDepth(tree.Root);
#endif
        }

        private static int CheckDepth(CompactAvlTree<int>.Node node)
        {
            if (node != null)
            {
                int err = 0;
                int rv;
                int b = CheckDepth(node.Left);
                int f = CheckDepth(node.Right);
                if (b == f)
                {
                    if (!node.IsBalanced)
                    {
                        err = 1;
                    }
                    rv = b + 1;
                }
                else if (b == f - 1)
                {
                    if (node.Longer != CompactAvlTree<int>.Direction.Right)
                    {
                        err = 1;
                    }
                    rv = f + 1;
                }
                else if (b - 1 == f)
                {
                    if (node.Longer != CompactAvlTree<int>.Direction.Left)
                    {
                        err = 1;
                    }
                    rv = b + 1;
                }
                else
                {
                    err = 1;
                    rv = 0;
                }
                if (err != 0)
                {
                    throw new Exception($"Error at {node.Item}: b={b}, f={f}, direction={node.Longer}");
                }
                return rv;
            }
            return 0;
        }
    }
}
