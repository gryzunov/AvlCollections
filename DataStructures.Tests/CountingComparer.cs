using System.Collections.Generic;

namespace DataStructures.Tests
{
    public class CountingComparer: IComparer<int>
    {
        public int Count { get; private set; }

        public int Compare(int x, int y)
        {
            Count++;
            return x.CompareTo(y);
        }
    }
}
