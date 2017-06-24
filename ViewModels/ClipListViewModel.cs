using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Data;

namespace clipman.ViewModels
{
    public class ClipListViewModel
    {
        public ObservableCollection<Models.Clip> Clips
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
            }
        }

        public ClipListViewModel()
        {
            Clips = new ObservableCollection<Models.Clip>();
            ClipView = CollectionViewSource.GetDefaultView(Clips);
            ClipView.Filter = Filter;
        }

        public void AddClip(Models.Clip clip)
        {
            Clips.Add(clip);
        }

        public bool Filter(object item)
        {
            Models.Clip clip = item as Models.Clip;
            if (clip != null)
            {
                return clip.Matches(FilterString);
            }
            return true;
        }
    }
}
