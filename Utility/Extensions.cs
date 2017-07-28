using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

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

        public struct Occurence : ICloneable
        {
            public int Index
            {
                get;
                set;
            }

            public char Char
            {
                get;
                set;
            }

            public Occurence(int index, char c)
            {
                Index = index;
                Char = c;
            }

            public object Clone()
            {
                return new Occurence(Index, Char);
            }
        }
    }

    public static class Logging
    {
        public static void Log(String message)
        {
            Console.WriteLine(
                String.Format(
                    "{0} :: {1}",
                    DateTime.Now,
                    message
                )
            );
        }
    }
}
