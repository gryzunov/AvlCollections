﻿using System;
using Xunit;

namespace AvlCollections.Tests
{
    public class AvlTreeTests
    {
        [Fact]
        public void TestInsert()
        {
            var tree = new AvlTree<int>();
            for (int i = 1; i <= 100; i++)
            {
                Assert.True(tree.TryAdd(i));
            }
        }

        [Fact]
        public void TestDelete()
        {
            var tree = new AvlTree<int>();
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
            var tree = new AvlTree<int>();
            for (int i = 1; i <= 100; i++)
            {
                tree.Add(i);
                CheckBalance(tree);
            }
        }

        [Fact]
        public void DeleteBalanceTest()
        {
            var tree = new AvlTree<int>();
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
            var tree = new AvlTree<int>();
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

        [Fact]
        public void InOrderTreeWalkTest()
        {
            var tree = new AvlTree<int>();
            var src = new int[100];
            for (int i = 0; i < src.Length; i++)
            {
                src[i] = i + 1;
                tree.Add(i + 1);
            }

            var dest = new int[100];
            var index = 0;
            tree.InOrderTreeWalk(item =>
            {
                dest[index++] = item;
                return true;
            });

            Assert.Equal(src, dest);
        }

        private void CheckBalance(AvlTree<int> tree)
        {
#if DEBUG
            CheckDepth(tree.Root);
#endif
        }

        private static int CheckDepth(AvlTree<int>.Node node)
        {
            if (node != null)
            {
                int err = 0;
                int rv;
                int b = CheckDepth(node.Left);
                int f = CheckDepth(node.Right);
                if (b == f)
                {
                    if (node.Balance != 0)
                    {
                        err = 1;
                    }
                    rv = b + 1;
                }
                else if (b == f - 1)
                {
                    if (node.Balance != -1)
                    {
                        err = 1;
                    }
                    rv = f + 1;
                }
                else if (b - 1 == f)
                {
                    if (node.Balance != 1)
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
                    throw new Exception($"Error at {node.Item}: b={b}, f={f}, balance={node.Balance}");
                }
                return rv;
            }
            return 0;
        }
    }
}
