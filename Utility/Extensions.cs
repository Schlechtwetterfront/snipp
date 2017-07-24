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

        public static int[] GetCharPositions(this string s, char c, int start = 0)
        {
            var positions = new List<int>();

            int startIndex = start;

            while (true)
            {
                int nextIndex = s.IndexOf(c, startIndex);

                if (nextIndex >= 0)
                {
                    startIndex = nextIndex + 1;
                    positions.Add(nextIndex);
                }
                else
                {
                    break;
                }
            }

            return positions.ToArray();
        }

        public static List<List<Occurence>> GetOccurences(this string s, String searchKey, int start = 0, int index = 0, List<Occurence> list = null)
        {
            if (list == null)
            {
                list = new List<Occurence>();
            }

            if (index > searchKey.Length - 1)
            {
                var r = new List<List<Occurence>>();
                r.Add(list);
                return r;
            }

            char c = searchKey[index];

            int[] charPositions = s.GetCharPositions(c, start);

            while (charPositions.Length == 0)
            {
                index++;

                if (index > searchKey.Length - 1)
                {
                    var r = new List<List<Occurence>>();
                    r.Add(list);
                    return r;
                }

                c = searchKey[index];
                charPositions = s.GetCharPositions(c, start);
            }

            var results = new List<List<Occurence>>();
            foreach (int p in charPositions)
            {
                var copy = list.Select(item => (Occurence)item.Clone()).ToList();
                copy.Add(new Occurence(p, c));

                results.AddRange(s.GetOccurences(searchKey, p + 1, index + 1, copy));
            }

            return results;
        }

        public static Fuzzy.FuzzyResult GetBestMatch(this string s, String searchKey)
        {
            var results = s.ToLower().GetOccurences(searchKey.ToLower()).Select(Fuzzy.GetChainScore).ToList();
            if (results != null && results.Count > 0)
            {
                results.Sort();
                results.Reverse();
                return results[0];
            }
            return null;
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

    public static class Fuzzy
    {
        public class FuzzyMatch
        {
            public int Length
            {
                get;
                set;
            }

            public int Start
            {
                get;
                set;
            }

            public int End
            {
                get { return Start + Length; }
            }

            public FuzzyMatch(int start)
            {
                Start = start;
                Length = 0;
            }
        }

        public class FuzzyResult : IComparable<FuzzyResult>
        {
            public const int ScoreDistance = 4;
            public const int ScoreFoundChar = 16;
            public const int ScoreConsecutive = 16;

            public int Score
            {
                get;
                set;
            }

            public List<FuzzyMatch> Matches
            {
                get;
                set;
            }

            public FuzzyResult(int score = 0, List<FuzzyMatch> matches = null)
            {
                Score = score;
                if (matches != null)
                {
                    Matches = matches;
                }
                else
                {
                    Matches = new List<FuzzyMatch>();
                }
            }

            public int CompareTo(FuzzyResult other)
            {
                return this.Score.CompareTo(other.Score);
            }
        }

        public static FuzzyResult GetChainScore(List<Extensions.Occurence> occurences)
        {
            var matches = new List<FuzzyMatch>();

            int lastIndex = -1;

            int consecutiveMatchScore = 0;

            var currentMatch = new FuzzyMatch(lastIndex);

            bool firstChar = true;

            int score = 0;

            foreach (var o in occurences)
            {
                if (o.Index == lastIndex + 1 && lastIndex != -1)
                {
                    // If this index comes directly after the previous found char,
                    // add bonus score and append to the match.
                    consecutiveMatchScore += FuzzyResult.ScoreConsecutive;
                    currentMatch.Length++;
                }
                else
                {
                    if (currentMatch.Length > 0)
                    {
                        matches.Add(currentMatch);
                    }
                    consecutiveMatchScore = 0;
                    currentMatch = new FuzzyMatch(o.Index);
                    currentMatch.Length++;
                }

                int distance = 0;
                if (!firstChar)
                {
                    // Subtract 1 here because shortest distance is 1 but we want 0 for
                    // further calculations.
                    distance = Math.Max(o.Index - Math.Max(lastIndex, 0) - 1, 0);
                }

                firstChar = false;

                score -= distance * FuzzyResult.ScoreDistance;
                score += FuzzyResult.ScoreFoundChar;
                score += consecutiveMatchScore;

                lastIndex = o.Index;
            }

            if (currentMatch.Length > 0)
            {
                matches.Add(currentMatch);
            }

            return new FuzzyResult(score, matches);
        }
    }
}
