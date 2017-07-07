using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;

namespace clipman.ViewModels
{
    public class ClipListViewModel
    {
        public ObservableCollection<ClipViewModel> Clips
        {
            get;
            set;
        }

        public ICollectionView ClipView
        {
            get;
            set;
        }

        String filterString;
        public String FilterString
        {
            get { return filterString; }
            set
            {
                filterString = value.Trim().ToLower();
                ClipView.Refresh();
            }
        }

        private ICommand copyClipCommand;
        public ICommand CopyClipCommand
        {
            get
            {
                return copyClipCommand ?? (copyClipCommand = new Commands.Command(param =>
                {
                    Utility.Logging.Log("Enter copy command");
                    (param as ClipViewModel)?.Clip.Copy();
                }));
            }
        }

        private ICommand deleteClipCommand;
        public ICommand DeleteClipCommand
        {
            get
            {
                return deleteClipCommand ?? (deleteClipCommand = new Commands.Command(param =>
                {
                    Utility.Logging.Log("Delete clip command");
                    Clips.Remove(param as ClipViewModel);
                }));
            }
        }

        public ClipListViewModel()
        {
            Clips = new ObservableCollection<ClipViewModel>();
            ClipView = CollectionViewSource.GetDefaultView(Clips);
            ClipView.Filter = Filter;
            ClipView.SortDescriptions.Add(new SortDescription("Clip", ListSortDirection.Descending));
        }

        public void AddClip(Clipboard.Clip clip)
        {
            Clips.Add(new ClipViewModel(clip));
            ClipView.Refresh();
        }

        public bool Filter(object item)
        {
            Clipboard.Clip clip = (item as ClipViewModel).Clip;
            if (clip != null)
            {
                return clip.Matches(FilterString);
            }
            return true;
        }
    }
}
