using System;
using System.Collections.Generic;
using System.Text;

namespace Gamefloor.Support
{
    public struct Priority : IComparable<Priority>
    {
        public int p0, p1, p2, p3;

        public Priority(int _p0, int _p1, int _p2, int _p3)
        {
            p0 = _p0;
            p1 = _p1;
            p2 = _p2;
            p3 = _p3;
        }

        public int CompareTo(Priority other)
        {
            int result = p0.CompareTo(other.p0);
            if (result == 0) result = p1.CompareTo(other.p1);
            if (result == 0) result = p2.CompareTo(other.p2);
            if (result == 0) result = p3.CompareTo(other.p3);
            return result;
        }
    }
}
