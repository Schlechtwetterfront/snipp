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

        private int clipLimit;
        public int ClipLimit
        {
            get { return clipLimit; }
            set
            {
                clipLimit = value;
                while (Clips.Count > clipLimit)
                {
                    Clips.RemoveAt(0);
                }
                RaisePropertyChanged("ClipLimit");
                RaisePropertyChanged("Clips");
            }
        }

        private object selectedItem = new object();
        public object SelectedItem
        {
            get { return selectedItem; }
            set { selectedItem = value; RaisePropertyChanged("SelectedItem"); }
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
            var viewModel = new ClipViewModel(clip);
            viewModel.PropertyChanged += OnClipViewModelPropChanged;
            if (Clips.Count >= ClipLimit)
            {
                // If the limit is reached, throw out the oldest one.
                Clips.RemoveAt(0);
            }
            Clips.Add(viewModel);
        }

        private void OnClipViewModelPropChanged(object sender, PropertyChangedEventArgs e)
        {
            var cvm = sender as ClipViewModel;
            if (e.PropertyName == "Clip" && cvm.Clip == null)
            {
                Clips.Remove(cvm);
            }
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
