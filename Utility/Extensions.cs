using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clipman.Utility
{
    public static class Extensions
    {
        public static T NthInView<T>(this ICollectionView view, int index)
        {
            var enumerator = view.GetEnumerator();
            foreach (int i in Enumerable.Range(0, index + 1))
            {
                if (!enumerator.MoveNext())
                {
                    return default(T);
                }

            }
            return (T)enumerator.Current;
        }
    }
}
