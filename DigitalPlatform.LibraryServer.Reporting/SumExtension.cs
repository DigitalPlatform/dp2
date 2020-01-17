using DigitalPlatform.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DigitalPlatform.LibraryServer.Reporting
{
    public static class SumExtension
    {
        public static string SumPrice<T>(this IEnumerable<T> collection,
    Func<T, string> selector)
        {
            return collection.Aggregate("" /*start with an empty MyClass*/,
                (a, b) => PriceUtil.Add(a, selector(b)));
        }
    }
}
