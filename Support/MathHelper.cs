using System;
using System.Collections.Generic;
using System.Text;

namespace Gamefloor.Support
{
    public class MathHelper
    {
        public static int Log2(int value)
        {
            int result = 0;
            while ((value >>= 1) > 0) result++;
            return result;
        }
    }
}
