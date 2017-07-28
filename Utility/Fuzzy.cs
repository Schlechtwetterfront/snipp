using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace clipman.Utility
{
    public class ScoreConfig
    {
        public int ConsecutiveMatchBonus { get; set; } = 16;

        public int DistancePenalty { get; set; } = 4;

        public int Score(Stack<int> positions)
        {
            return Score(positions.Reverse());
        }

        public int Score(IEnumerable<int> positions)
        {
            int score = 0, lastPos = 0, consecutiveScore = 0;
            bool isFirstPos = true;

            foreach (var pos in positions)
            {
                if (isFirstPos)
                {
                    lastPos = pos;
                    isFirstPos = false;
                    continue;
                }

                var distance = pos - lastPos;

                if (distance == 1)
                {
                    consecutiveScore += ConsecutiveMatchBonus;
                }
                else
                {
                    consecutiveScore = 0;
                }

                score -= distance * DistancePenalty;
                score += consecutiveScore;

                lastPos = pos;
            }
            return score;
        }
    }

    public class Match : IComparable<Match>
    {
        public class MatchGroup
        {
            public int Start { get; set; } = 0;

            public int Length { get; set; } = 0;

            public MatchGroup(int start = 0, int length = 0)
            {
                Start = start;
                Length = length;
            }
        }

        public int Score
        {
            get;
            set;
        }

        private List<int> matches = new List<int>();
        public List<int> Matches
        {
            get { return matches; }
            set { matches = value; }
        }

        public Match()
        {
            Score = 0;
        }

        public Match(int score, List<int> matches)
        {
            Score = score;
            Matches = matches;
        }

        public List<MatchGroup> GetContinuousMatches()
        {
            var groups = new List<MatchGroup>();

            int currentStart = 0, currentLength = 0, lastIndex = 0;
            bool isFirstIndex = true;

            foreach (var i in this.Matches)
            {
                if (!isFirstIndex && i - 1 == lastIndex)
                {
                    currentLength++;
                }
                else
                {
                    if (currentLength > 0)
                    {
                        groups.Add(new MatchGroup(currentStart, currentLength));
                    }
                    currentStart = i;
                    currentLength = 1;
                    isFirstIndex = false;
                }
                lastIndex = i;
            }

            if (currentLength > 0)
            {
                groups.Add(new MatchGroup(currentStart, currentLength));
            }

            return groups;
        }

        public static int CalculateMatchScore(Match m, ScoreConfig config)
        {
            return 0;
        }

        public int CompareTo(Match other)
        {
            return Score.CompareTo(other.Score);
        }
    }

    public class FuzzySearch
    {
        private Stack<int> indexStack = new Stack<int>();

        private Match bestMatch = new Match();

        private String pattern;

        private Dictionary<char, List<int>> charMap;

        private ScoreConfig scoreConfig = new ScoreConfig();

        public FuzzySearch(String pattern, String target)
        {
            this.pattern = Regex.Replace(pattern, @"\s+", "");
            charMap = FuzzySearch.BuildCharMap(target);
        }

        public Match FindBestMatch()
        {
            Match();
            return bestMatch;
        }

        protected void Match()
        {
            var patternChar = pattern[0];
            var occurences = GetOccurences(patternChar, charMap);

            if (occurences != null)
            {
                foreach (var o in occurences)
                {
                    MatchChar(1, o);
                }
            }
        }

        protected void MatchChar(int patternIndex, int offset)
        {
            indexStack.Push(offset);

            if (patternIndex >= pattern.Length)
            {
                ScoreCurrent();
                indexStack.Pop();
                return;
            }
            char patternChar = pattern[patternIndex];

            var occurences = GetOccurences(patternChar, charMap, offset + 1);

            if (occurences == null || occurences.Count == 0)
            {
                ScoreCurrent();
                indexStack.Pop();
                return;
            }

            foreach (var o in occurences)
            {
                MatchChar(patternIndex + 1, o);
            }

            indexStack.Pop();
        }

        protected void ScoreCurrent()
        {
            var currentScore = scoreConfig.Score(indexStack);
            if (currentScore > bestMatch.Score)
            {
                var newBestMatch = new Match(currentScore, new List<int>(indexStack.Reverse()));
                bestMatch = newBestMatch;
            }
        }

        private static List<int> GetOccurences(char c, Dictionary<char, List<int>> charMap, int offset = 0)
        {
            if (charMap.ContainsKey(c))
            {
                return charMap[c].Where(i => i >= offset).ToList();
            }
            else
            {
                return new List<int>();
            }
        }

        private static Dictionary<char, List<int>> BuildCharMap(String target)
        {
            var map = new Dictionary<char, List<int>>();

            foreach (var o in target.Select((c, i) => new { Index = i, Char = c }))
            {
                if (map.ContainsKey(o.Char))
                {
                    map[o.Char].Add(o.Index);
                }
                else
                {
                    map[o.Char] = new List<int> { o.Index };
                }
            }

            return map;
        }
    }
}
