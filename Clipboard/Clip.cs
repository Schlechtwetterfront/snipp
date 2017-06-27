using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using System.Text.RegularExpressions;

namespace clipman.Clipboard
{


    public class Clip : INotifyPropertyChanged
    {
        static int TitleCharCount = 128;

        String content;
        public String Content
        {
            get { return content; }
            set
            {
                content = value;
                var builder = new StringBuilder();
                foreach (var c in value.Trim().Take(Clip.TitleCharCount))
                {
                    builder.Append(c);
                }
                Title = Regex.Replace(builder.ToString(), @"[\s|\r\n?|\n]+", " ");
                SearchContent = value.Trim().ToLower();
            }
        }

        public DateTime Created
        {
            get;
            set;
        }

        /// <summary>
        /// Shortened content without line breaks.
        /// </summary>
        public String Title
        {
            get;
            protected set;
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
                return new Clip(System.Windows.Clipboard.GetText());
            }
            catch (System.Runtime.InteropServices.ExternalException e)
            {
                Utility.Logging.Log("Failed to read clipboard: " + e.Message);
                return null;
            }
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
            if (string.IsNullOrWhiteSpace(searchString))
            {
                return true;
            }

            if (SearchContent.Contains(searchString))
            {
                return true;
            }

            if (Created.ToShortDateString().Contains(searchString))
            {
                return true;
            }

            return false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public event EventHandler<ClipboardManager.ClipEventArgs> Copied;
    }
}
