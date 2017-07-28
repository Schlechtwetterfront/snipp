using clipman.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using static clipman.Clipboard.ClipboardManager;

namespace clipman.ViewModels
{
    public class ClipViewModel : IComparable, INotifyPropertyChanged
    {
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
                }

            }
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

        private bool pinned = false;
        /// <summary>
        /// If this `ClipViewModel`/`Clip` is pinned.
        /// </summary>
        public bool Pinned
        {
            get { return pinned; }
            set { pinned = value; RaisePropertyChanged("Pinned"); RaisePropertyChanged("PinLabel"); }
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

        public Match FuzzyMatches(String searchKey)
        {
            var s = new FuzzySearch(searchKey, Clip.SearchContent);
            var result = s.FindBestMatch();
            UpdateFromFuzzy(result);
            return result;
        }

        public void UpdateFromFuzzy(Match m)
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
            inlines.Add(new Run(Clip.Title));
            RichTitle = inlines;
        }

        void UpdateRichTitleFromFuzzy(Match match)
        {
            var inlines = new ObservableCollection<Inline>();
            int lastEnd = 0;
            foreach (var m in match.GetContinuousMatches())
            {
                inlines.Add(new Run(Clip.Title.Substring(lastEnd, m.Start - lastEnd)));
                var colored = new Run(Clip.Title.Substring(m.Start, m.Length));
                colored.Foreground = (SolidColorBrush)Properties.Settings.Default["Accent"];
                inlines.Add(colored);
                lastEnd = m.Start + m.Length;
            }
            if (lastEnd != Clip.Title.Length)
            {
                inlines.Add(new Run(Clip.Title.Substring(lastEnd)));
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
