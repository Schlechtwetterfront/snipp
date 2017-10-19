using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using System.Text.RegularExpressions;

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
                filterString = Regex.Replace(value.Trim().ToLower(), @"\s+", "");
                bool isEmpty = string.IsNullOrEmpty(filterString);
                RaisePropertyChanged("FilterString");
                foreach (var c in Clips)
                {
                    if (isEmpty)
                    {
                        c.SearchScore = 0;
                        c.ResetRichTitle();
                    }
                    else
                    {
                        c.Match(filterString);
                    }
                }
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
                if (clipLimit > 0)
                {
                    while (Clips.Count > clipLimit)
                    {
                        Clips.RemoveAt(0);
                    }
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

            ClipView.IsLiveSorting = true;
            ClipView.LiveSortingProperties.Add("Pinned");

            ClipView.SortDescriptions.Add(new SortDescription("SearchScore", ListSortDirection.Descending));
            ClipView.SortDescriptions.Add(new SortDescription("Pinned", ListSortDirection.Descending));
            ClipView.SortDescriptions.Add(new SortDescription("Clip", ListSortDirection.Descending));

            (ClipView as INotifyCollectionChanged).CollectionChanged += ClipViewChanged;

            Properties.Settings.Default.SettingChanging += SettingChanging;

            ClipLimit = (int)Properties.Settings.Default["ClipLimit"];
        }

        void SettingChanging(object sender, System.Configuration.SettingChangingEventArgs e)
        {
            if (e.SettingName == "ClipLimit")
            {
                ClipLimit = (int)e.NewValue;
            }
        }

        public void ClipViewChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var it in ClipView.OfType<ClipViewModel>().Select((el, i) => new { Item = el, Index = i }))
            {
                if (it.Index < 10)
                {
                    it.Item.IndexInClipView = it.Index;
                }
                else
                {
                    it.Item.IndexInClipView = -1;
                }
            }
        }

        public bool AddClip(Clipboard.Clip clip)
        {
            // Check if a clip with the same content exists in the last clips.
            var equalClips = Clips.Take(10).Where(vm => vm.Clip.Content == clip.Content);

            if (equalClips.Count() > 0)
            {
                // If a clip with the same content already exists, just update the creation time.
                // TODO: Possibly refresh list view to push clip to top.
                equalClips.First().Clip.Created = DateTime.Now;
                return false;
            }

            var viewModel = new ClipViewModel(clip);

            viewModel.PropertyChanged += OnClipViewModelPropChanged;

            if (Clips.Count >= ClipLimit && ClipLimit > 0)
            {
                // If the limit is reached, throw out the oldest one.
                // Make sure it is not pinned.
                var first = Clips.Where((c) => !c.Pinned).First();
                Clips.Remove(first);
            }

            Clips.Add(viewModel);

            return true;
        }

        private void OnClipViewModelPropChanged(object sender, PropertyChangedEventArgs e)
        {
            var cvm = sender as ClipViewModel;
            if (e.PropertyName == "Clip" && cvm.Clip == null)
            {
                Clips.Remove(cvm);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
