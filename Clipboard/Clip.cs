using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace clipman.Clipboard
{


    public class Clip : INotifyPropertyChanged, IComparable
    {
        static int TitleCharCount = 256;
        static char TrimmingCharacter = '\u2026';

        String processedContent;
        public String OneLineContent
        {
            get { return processedContent; }
        }
        String content;
        /// <summary>
        /// Actual content of the clip as copied from the clipboard.
        /// </summary>
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
                    defaultTitle += Clip.TrimmingCharacter;
                }
                Title = defaultTitle;

                RaisePropertyChanged("Content");
                RaisePropertyChanged("Title");
            }
        }

        /// <summary>
        /// Time this clip was retrieved.
        /// </summary>
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
                return defaultTitle;
            }
            protected set
            {
                defaultTitle = value;
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

        /// <summary>
        /// Tries to copy this clip to the clipboard.
        /// </summary>
        /// <returns>True if clipboard was filled.</returns>
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

        /// <summary>
        /// Checks if this clip matches a search string in any way.
        /// </summary>
        /// <param name="searchString">The search string</param>
        /// <returns>True if a match is likely</returns>
        public bool Matches(String searchString)
        {
            Title = defaultTitle;
            //TitleMain = TitleSuffix = "";

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

        /// <summary>
        /// If a match in the main content was found adjust the clip title to
        /// highlight the search string.
        /// </summary>
        /// <param name="searchString">The matched search string</param>
        protected void ProcessSearchString(String searchString)
        {
            int start, end, index, searchLength, contentLength, limit;
            String prefix, foundString, suffix;

            // Start of the found string.
            index = SearchContent.IndexOf(searchString);
            searchLength = searchString.Length;
            contentLength = SearchContent.Length;

            // If the length is equal,
            if (searchLength == contentLength)
            {
                //TitleMain = defaultTitle;
                //TitlePrefix = TitleSuffix = "";
                return;
            }

            // Limit the string to ensure that it fully displays at our default
            // window width.
            limit = Math.Min(contentLength, Clip.TitleCharCount);

            int suffixStart = index + searchLength;

            // Left over space after the search string is cut out.
            int spaceLeft = limit - searchLength;
            // Calculate (even) space for prefix and suffix. Also account for
            // odd space and just give the prefix one more in that case.
            int spaceForPrefix = (int)Math.Ceiling(spaceLeft * 0.5);
            int spaceForSuffix = (int)Math.Floor(spaceLeft * 0.5);

            int suffixLength = contentLength - suffixStart;
            int prefixLength = index;
            // Suffix is smaller than what was reserved for it, so give more
            // space to prefix.
            if (suffixLength < spaceForSuffix)
            {
                // Add left over space from suffix to prefix, unless it already has enough space.
                prefixLength = Math.Min(prefixLength, spaceForPrefix + spaceForSuffix - suffixLength);
                start = Math.Max(index - prefixLength, 0);
                end = contentLength;
            }
            // Prefix is smaller than what was reserved for it, so give more
            // space to prefix.
            else if (prefixLength < spaceForPrefix)
            {
                start = 0;
                // Add left over space from prefix to suffix, unless it already has enough space.
                suffixLength = Math.Min(suffixLength, spaceForSuffix + spaceForPrefix - prefixLength);
                end = Math.Min(suffixStart + suffixLength, limit);
            }
            else
            {
                // Both parts fill their reserved space. Any excess will be cut
                // off to ensure that the whole string is displayable.
                prefixLength = spaceForPrefix;
                suffixLength = spaceForSuffix;
                start = index - spaceForPrefix;
                end = suffixStart + spaceForSuffix;
            }

            // Cut content into prefix, main and suffix.
            prefix = processedContent.Substring(start, prefixLength);
            foundString = processedContent.Substring(index, searchLength);
            suffix = processedContent.Substring(
                suffixStart, suffixLength
            );

            // Add ... if some of the head of the string is cut off.
            if (start == 0)
            {
                //TitlePrefix = prefix;
            }
            else
            {
                //TitlePrefix = Clip.TrimmingCharacter + prefix;
            }

            //TitleMain = foundString;

            // Add ... if some of the tail of the string is cut off.
            if (end == contentLength)
            {
                //TitleSuffix = suffix;
            }
            else
            {
                //TitleSuffix = suffix + Clip.TrimmingCharacter;
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
