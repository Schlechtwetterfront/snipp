using clipman.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace clipman.ViewModels
{
    class SettingsPanelViewModel : INotifyPropertyChanged
    {
        private Settings.Settings settings;
        public Settings.Settings Settings
        {
            get { return settings; }
            private set { settings = value; RaisePropertyChanged("Settings"); }
        }

        public List<Theme> Themes
        {
            get { return Theme.Themes; }
        }

        public SettingsPanelViewModel(Settings.Settings settings)
        {
            this.Settings = settings;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
