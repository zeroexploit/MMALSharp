﻿namespace Raspicam.Net.Native.Internal
{
    static class HelperExtensions
    {
        public static int ToFourCc(this string s)
        {
            int a1 = s[0];
            int b1 = s[1];
            int c1 = s[2];
            int d1 = s[3];
            return a1 | (b1 << 8) | (c1 << 16) | (d1 << 24);
        }
    }
}
