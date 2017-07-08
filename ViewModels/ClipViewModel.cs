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
            }
        }

        /// <summary>
        /// String displaying the creation date of the wrapped `Clip`.
        /// </summary>
        public String CreatedString
        {
            get
            {
                if (clip.Created.Date == DateTime.Today)
                {
                    return clip.Created.ToShortTimeString();
                }
                else
                {
                    return clip.Created.ToString("d");
                }
            }
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
            set { pinned = value; RaisePropertyChanged("Pinned"); }
        }

        public ClipViewModel(Clipboard.Clip clip)
        {
            this.clip = clip;
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
