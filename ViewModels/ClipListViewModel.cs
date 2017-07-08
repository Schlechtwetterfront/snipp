using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;

namespace clipman.ViewModels
{
    public class ClipListViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ClipViewModel> Clips
        {
            get;
            set;
        }

        public ListCollectionView ClipView
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
                RaisePropertyChanged("FilterString");
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
            ClipView = CollectionViewSource.GetDefaultView(Clips) as ListCollectionView;
            ClipView.IsLiveFiltering = true;
            ClipView.Filter = Filter;
            ClipView.IsLiveSorting = true;
            ClipView.LiveSortingProperties.Add("Pinned");
            ClipView.SortDescriptions.Add(new SortDescription("Pinned", ListSortDirection.Descending));
            ClipView.SortDescriptions.Add(new SortDescription("Clip", ListSortDirection.Descending));
        }

        public void AddClip(Clipboard.Clip clip)
        {
            Clips.Add(new ClipViewModel(clip));
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

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
