﻿using clipman.Utility;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace clipman.ViewModels
{
    public class ClipViewModel : IComparable, INotifyPropertyChanged
    {
        public static int MaxSearchContentLength = 1000;

        protected Clipboard.Clip clip;
        /// <summary>
        /// The `Clip` this ViewModel is wrapped around.
        /// </summary>
        public Clipboard.Clip Clip
        {
            get
            {
                return clip;
            }
            set
            {
                clip = value;

                RaisePropertyChanged("Clip");

                if (clip != null)
                {
                    if (clip.Created.Date == DateTime.Today)
                    {
                        createdDateString = clip.Created.ToShortTimeString();
                    }
                    else
                    {
                        createdDateString = clip.Created.ToString("d");
                    }

                    processedContent = System.Text.RegularExpressions.Regex.Replace(value.Content, @"[\s]+", " ").Trim();

                    SearchContent = processedContent.ToLower();

                    RaisePropertyChanged("Title");
                    RaisePropertyChanged("SearchContent");

                    if (HasColorContent)
                    {
                        SolidColorBrush brush = new SolidColorBrush();
                        brush.Color = GetColor();
                        contentColor = brush;
                        RaisePropertyChanged("ContentColor");
                    }
                }

            }
        }

        /// <summary>
        /// Lower-case content where all whitespace is replaced with a single space.
        /// </summary>
        public String SearchContent
        {
            get;
            protected set;
        }

        String processedContent;
        public String Title
        {
            get { return processedContent; }
        }

        private String createdDateString;
        /// <summary>
        /// String displaying the creation date of the wrapped `Clip`.
        /// </summary>
        public String CreatedString
        {
            get { return createdDateString; }
        }

        private ICommand pinClipCommand;
        /// <summary>
        /// Executed when this `ClipViewModel` is pinned.
        /// </summary>
        public ICommand PinClipCommand
        {
            get
            {
                return pinClipCommand ?? (pinClipCommand = new Commands.Command(param =>
                {
                    Pinned = !Pinned;
                }));
            }
        }

        /// <summary>
        /// If this `ClipViewModel`/`Clip` is pinned.
        /// </summary>
        public bool Pinned
        {
            get { return Clip != null ? Clip.Pinned : false; }
            set { Clip.Pinned = value; RaisePropertyChanged("Pinned"); RaisePropertyChanged("PinLabel"); }
        }

        public String PinLabel
        {
            get
            {
                return Pinned ? "Un_pin" : "_Pin";
            }
        }

        private int indexInClipView = -1;
        public int IndexInClipView
        {
            get { return indexInClipView; }
            set
            {
                indexInClipView = value;
                RaisePropertyChanged("IndexInClipView");
                RaisePropertyChanged("NumberShortcutText");
            }
        }

        public String NumberShortcutText
        {
            get
            {
                if (IndexInClipView >= 0 && IndexInClipView < 9)
                {
                    return String.Format("Ctrl+{0}", IndexInClipView + 1);
                }
                else if (IndexInClipView == 9)
                {
                    return String.Format("Ctrl+0");
                }
                else
                {
                    return "";
                }
            }
        }

        public bool HasColorContent
        {
            get
            {
                return Regex.IsMatch(Clip.Content, @"#([0-9A-F]{3,6})", RegexOptions.IgnoreCase) || Regex.IsMatch(Clip.Content, @"rgba\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*,\s*([\d.]+)\s*\)", RegexOptions.IgnoreCase);
            }
        }

        private SolidColorBrush contentColor = null;
        public SolidColorBrush ContentColor
        {
            get
            {
                return contentColor;
            }
        }

        private ICommand deleteCommand;
        public ICommand DeleteCommand
        {
            get
            {
                return deleteCommand ?? (deleteCommand = new Commands.Command(param =>
                {
                    Clip = null;
                    RaisePropertyChanged("Clip");
                }));
            }
        }

        private ICommand copyCommand;
        public ICommand CopyCommand
        {
            get
            {
                return copyCommand ?? (copyCommand = new Commands.Command(param =>
                {
                    Clip?.Copy();
                }));
            }
        }

        private ObservableCollection<Inline> richTitle = new ObservableCollection<Inline>();
        public ObservableCollection<Inline> RichTitle
        {
            get { return richTitle; }
            protected set { richTitle = value; RaisePropertyChanged("RichTitle"); }
        }

        public int SearchScore
        {
            get;
            set;
        }

        public ClipViewModel(Clipboard.Clip clip)
        {
            this.Clip = clip;
            ResetRichTitle();
        }

        public Color GetColor()
        {
            var hexMatch = Regex.Match(Clip.Content, @"#([0-9A-F]{3,8})", RegexOptions.IgnoreCase);

            if (hexMatch.Success)
            {
                var group = hexMatch.Groups[1];
                return (Color)ColorConverter.ConvertFromString("#" + group.Value);
            }

            var rgbaMatch = Regex.Match(Clip.Content, @"rgba\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*,\s*([\d.]+)\s*\)", RegexOptions.IgnoreCase);

            if (rgbaMatch.Success)
            {
                byte r = Byte.Parse(rgbaMatch.Groups[1].Value);
                byte g = Byte.Parse(rgbaMatch.Groups[2].Value);
                byte b = Byte.Parse(rgbaMatch.Groups[3].Value);
                byte a = (byte)(255 / Double.Parse(rgbaMatch.Groups[4].Value, NumberStyles.Any, CultureInfo.InvariantCulture));

                return Color.FromArgb(a, r, g, b);
            }

            return new Color();
        }

        public void Match(String searchKey)
        {
            if (SearchContent.Length > ClipViewModel.MaxSearchContentLength)
            {
                var index = SearchContent.IndexOf(searchKey);
                if (index >= 0)
                {
                    UpdateRichtTitleFromSingle(index, searchKey.Length);
                    SearchScore = FuzzySearch.CalculateMaxScore(searchKey.Length, new ScoreConfig());
                }
            }
            else
            {
                FuzzyMatch(searchKey);
            }
        }

        public Utility.Match FuzzyMatch(String searchKey)
        {
            var s = new FuzzySearch(searchKey, SearchContent);
            var result = s.FindBestMatch();
            UpdateFromFuzzy(result);
            return result;
        }

        public void UpdateFromFuzzy(Utility.Match m)
        {
            if (m == null)
            {
                SearchScore = 0;
                ResetRichTitle();
            }
            else
            {
                SearchScore = m.Score;
                UpdateRichTitleFromFuzzy(m);
            }
        }

        public void ResetRichTitle()
        {
            RichTitle.Clear();
            var inlines = new ObservableCollection<Inline>();
            inlines.Add(new Run(Title));
            RichTitle = inlines;
        }

        void UpdateRichtTitleFromSingle(int start, int length)
        {
            var inlines = new ObservableCollection<Inline>();

            inlines.Add(new Run(Title.Substring(0, start)));

            var colored = new Run(Title.Substring(start, length));
            colored.Foreground = (SolidColorBrush)Properties.Settings.Default["Accent"];
            inlines.Add(colored);

            inlines.Add(new Run(Title.Substring(start + length)));

            RichTitle = inlines;
        }

        void UpdateRichTitleFromFuzzy(Utility.Match match)
        {
            var inlines = new ObservableCollection<Inline>();
            int lastEnd = 0;
            foreach (var m in match.GetContinuousMatches())
            {
                inlines.Add(new Run(Title.Substring(lastEnd, m.Start - lastEnd)));
                var colored = new Run(Title.Substring(m.Start, m.Length));
                colored.Foreground = (SolidColorBrush)Properties.Settings.Default["Accent"];
                inlines.Add(colored);
                lastEnd = m.Start + m.Length;
            }
            if (lastEnd != Title.Length)
            {
                inlines.Add(new Run(Title.Substring(lastEnd)));
            }
            RichTitle = inlines;
        }

        public int CompareTo(object other)
        {
            ClipViewModel otherVM = other as ClipViewModel;
            if (otherVM == null)
                return 1;

            return Clip.CompareTo(otherVM.Clip);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
