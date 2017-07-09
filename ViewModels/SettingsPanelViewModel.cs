using System;
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
