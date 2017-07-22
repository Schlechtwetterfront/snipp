using System;
using System.ComponentModel;
using System.Windows.Input;
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

        public ClipViewModel(Clipboard.Clip clip)
        {
            this.Clip = clip;
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
