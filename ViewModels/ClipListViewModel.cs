using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;

namespace clipman.ViewModels
{
    public class ClipListViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Base collection of `ClipViewModels` (which contain the `Clip`s).
        /// </summary>
        public ObservableCollection<ClipViewModel> Clips
        {
            get;
            set;
        }

        /// <summary>
        /// Filtered and sorted collection view of `ClipViewModels`.
        /// </summary>
        public ListCollectionView ClipView
        {
            get;
            set;
        }

        /// <summary>
        /// String used to filter clips.
        /// </summary>
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

        /// <summary>
        /// Command copying the clip it was used on (Ctrl+C/Enter on selection).
        /// </summary>
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

        /// <summary>
        /// Command deleting the clip it was used on (Delete).
        /// </summary>
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

        /// <summary>
        /// Method used to filter the clips by search box input.
        /// </summary>
        /// <param name="item">`ClipViewModel`</param>
        /// <returns>True if `Clip` matches the search string or the search string is empty.</returns>
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
