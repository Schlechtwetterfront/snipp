﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Controls;
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

        public ClipListViewModel()
        {
            Clips = new ObservableCollection<ClipViewModel>();
            ClipView = CollectionViewSource.GetDefaultView(Clips);
            ClipView.Filter = Filter;
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
    }
}
