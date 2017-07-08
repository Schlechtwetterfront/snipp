using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace clipman.Clipboard
{


    public class Clip : INotifyPropertyChanged, IComparable
    {
        static int TitleCharCount = 45;

        String processedContent;
        String content;
        public String Content
        {
            get { return content; }
            set
            {
                content = value;

                processedContent = Regex.Replace(value, @"[\s]+", " ").Trim();

                SearchContent = processedContent.ToLower();

                defaultTitle = processedContent.Substring(
                    0,
                    Math.Min(Clip.TitleCharCount, processedContent.Length)
                );

                if (defaultTitle.Length < processedContent.Length)
                {
                    defaultTitle += "...";
                }
                Title = defaultTitle;

                RaisePropertyChanged("Content");
                RaisePropertyChanged("Title");
            }
        }

        public DateTime Created
        {
            get;
            set;
        }

        protected String defaultTitle;
        /// <summary>
        /// Shortened content without line breaks.
        /// </summary>
        public String Title
        {
            get
            {
                return TitlePrefix;
            }
            protected set
            {
                TitlePrefix = value;
            }
        }

        public String TitlePrefix
        {
            get;
            set;
        }

        public String TitleMain
        {
            get;
            set;
        }

        public String TitleSuffix
        {
            get;
            set;
        }

        /// <summary>
        /// Lower-case content where all whitespace is replaced with a single space.
        /// </summary>
        public String SearchContent
        {
            get;
            protected set;
        }

        public Clip()
        {
        }

        public Clip(String content)
        {
            Content = content;
            Created = DateTime.Now;
        }

        public static Clip Capture()
        {
            try
            {
                var data = System.Windows.Clipboard.GetDataObject();
                var formats = data.GetFormats();
                var textFormats = new string[]
                {
                    DataFormats.Html,
                    DataFormats.OemText,
                    DataFormats.Rtf,
                    DataFormats.StringFormat,
                    DataFormats.Text,
                    DataFormats.UnicodeText,
                    DataFormats.Xaml,
                    DataFormats.CommaSeparatedValue,
                };
                // Only make clip if it contains text data.
                if (textFormats.Any(s => formats.Contains(s))) {
                    return new Clip(System.Windows.Clipboard.GetText());
                }
            }
            catch (System.Runtime.InteropServices.ExternalException e)
            {
                Utility.Logging.Log("Failed to read clipboard: " + e.Message);
            }
            return null;
        }

        public bool Copy()
        {
            try
            {
                var args = new ClipboardManager.ClipEventArgs();
                args.Clip = this;

                Copied?.Invoke(this, args);

                System.Windows.Clipboard.SetText(Content);
                return true;
            }
            catch (System.Runtime.InteropServices.ExternalException e)
            {
                Utility.Logging.Log("Failed to set clipboard: " + e.Message);
                return false;
            }
        }

        public bool Matches(String searchString)
        {
            Title = defaultTitle;
            TitleMain = TitleSuffix = "";

            if (string.IsNullOrWhiteSpace(searchString))
            {
                return true;
            }

            if (SearchContent.Contains(searchString))
            {
                ProcessSearchString(searchString);
                return true;
            }

            if (Created.ToShortDateString().Contains(searchString))
            {
                return true;
            }

            return false;
        }

        protected void ProcessSearchString(String searchString)
        {
            int start, end, index, searchLength, contentLength, limit;
            String prefix, foundString, suffix;

            index = SearchContent.IndexOf(searchString);
            searchLength = searchString.Length;
            contentLength = SearchContent.Length;

            limit = Math.Min(contentLength, Clip.TitleCharCount);

            int suffixStart = index + searchLength;

            int spaceLeft = limit - searchLength;
            int spaceForPrefix = (int)Math.Ceiling(spaceLeft * 0.5);
            int spaceForSuffix = (int)Math.Floor(spaceLeft * 0.5);

            int suffixLength = contentLength - suffixStart;
            int prefixLength = index;
            // Suffix is smaller than half of the leftover space, so give more
            // space to prefix.
            if (suffixLength < spaceForSuffix)
            {
                prefixLength = Math.Min(prefixLength, spaceForPrefix + spaceForSuffix - suffixLength);
                start = Math.Max(index - prefixLength, 0);
                end = contentLength;
            }
            // Prefix is smaller than half of the leftover space.
            else if (prefixLength < spaceForPrefix)
            {
                start = 0;
                suffixLength = Math.Min(suffixLength, spaceForSuffix + spaceForPrefix - prefixLength);
                end = Math.Min(suffixStart + suffixLength, limit);
            }
            else
            {
                prefixLength = spaceForPrefix;
                suffixLength = spaceForSuffix;
                start = index - spaceForPrefix;
                end = suffixStart + spaceForSuffix;
            }

            prefix = processedContent.Substring(start, prefixLength);
            foundString = processedContent.Substring(index, searchLength);
            suffix = processedContent.Substring(
                suffixStart, suffixLength
            );

            if (start == 0)
            {
                TitlePrefix = prefix;
            }
            else
            {
                TitlePrefix = String.Format("...{0}", prefix);
            }

            TitleMain = foundString;

            if (end == contentLength)
            {
                TitleSuffix = suffix;
            }
            else
            {
                TitleSuffix = String.Format("{0}...", suffix);
            }
        }

        public int CompareTo(object other)
        {
            Clip otherClip = other as Clip;
            if (otherClip == null)
            {
                return 1;
            }

            return Created.CompareTo(otherClip.Created);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public event EventHandler<ClipboardManager.ClipEventArgs> Copied;
    }
}
