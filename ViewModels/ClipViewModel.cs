using System;
using System.ComponentModel;
using System.Windows.Input;
using static clipman.Clipboard.ClipboardManager;

namespace clipman.ViewModels
{
    public class ClipViewModel : IComparable, INotifyPropertyChanged
    {
        protected Clipboard.Clip clip;
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
