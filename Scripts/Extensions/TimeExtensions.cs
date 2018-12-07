using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UniJSON
{
    public static class TimeExtensions
    {
#if !NET_4_6 && !NET_STANDARD_2_0
        const long NSPerSecond = 100000000;
        const double TicksToSecodns = 1.0 / NSPerSecond;
        public readonly static DateTimeOffset EpocTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);
        public static long ToUnixTimeSeconds(this DateTimeOffset now)
        {
            return (now - EpocTime).Ticks / NSPerSecond;
        }
#endif
    }
}
