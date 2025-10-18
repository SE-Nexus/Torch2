using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IgniteUtils.Utils
{
    public static class TimeSpanExtensions
    {
        public static string ToCompactString(this TimeSpan ts)
        {
            var parts = new[]
            {
            ts.Days    > 0 ? $"{ts.Days}d"    : null,
            ts.Hours   > 0 ? $"{ts.Hours}h"   : null,
            ts.Minutes > 0 ? $"{ts.Minutes}m" : null,
            ts.Seconds > 0 ? $"{ts.Seconds}s" : null,
        }.Where(p => p != null);

            return string.Join(" ", parts);
        }
    }
}
