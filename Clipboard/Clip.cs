using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace clipman.Clipboard
{


    public class Clip : INotifyPropertyChanged, IComparable
    {
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

                RaisePropertyChanged("Content");
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
