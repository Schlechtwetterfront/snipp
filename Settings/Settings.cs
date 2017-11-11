using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace clipman.Settings
{
    public class Settings : INotifyPropertyChanged
    {
        protected KeyGesture focusWindowHotkey = new KeyGesture(Key.OemTilde, ModifierKeys.Control);
        public KeyGesture FocusWindowHotkey
        {
            get { return focusWindowHotkey; }
            set
            {
                focusWindowHotkey = value;
                RaisePropertyChanged("FocusWindowHotkey");
            }
        }

        protected KeyGesture focusSearchBox = new KeyGesture(Key.F, ModifierKeys.Control);
        public KeyGesture FocusSearchBox
        {
            get { return focusSearchBox; }
            set
            {
                focusSearchBox = value;
                RaisePropertyChanged("FocusSearchBox");
            }
        }

        protected KeyGesture clearAndFocusSearchBox = new KeyGesture(Key.Escape);
        public KeyGesture ClearAndFocusSearchBox
        {
            get { return clearAndFocusSearchBox; }
            set
            {
                clearAndFocusSearchBox = value;
                RaisePropertyChanged("ClearAndFocusSearchBox");
            }
        }

        protected KeyGesture copySelectedClip = new KeyGesture(Key.C, ModifierKeys.Control | ModifierKeys.Shift);
        public KeyGesture CopySelectedClip
        {
            get { return copySelectedClip; }
            set
            {
                copySelectedClip = value;
                RaisePropertyChanged("CopySelectedClip");
            }
        }

        protected KeyGesture deleteSelectedClip = new KeyGesture(Key.D, ModifierKeys.Control);
        public KeyGesture DeleteSelectedClip
        {
            get { return deleteSelectedClip; }
            set
            {
                deleteSelectedClip = value;
                RaisePropertyChanged("DeleteSelectedClip");
            }
        }

        protected KeyGesture pinSelectedClip = new KeyGesture(Key.P, ModifierKeys.Control);
        public KeyGesture PinSelectedClip
        {
            get { return pinSelectedClip; }
            set
            {
                pinSelectedClip = value;
                RaisePropertyChanged("PinSelectedClip");
            }
        }

        protected KeyGesture quit = new KeyGesture(Key.Q, ModifierKeys.Control);
        public KeyGesture Quit
        {
            get { return quit; }
            set
            {
                quit = value;
                RaisePropertyChanged("Quit");
            }
        }

        protected Theme currentTheme;
        public Theme CurrentTheme
        {
            get { return currentTheme; }
            set { currentTheme = value;  RaisePropertyChanged("CurrentTheme"); }
        }

        public Settings()
        {
            foreach (SettingsProperty prop in Properties.Settings.Default.Properties)
            {
                UpdateSetting(prop.Name);
            }

            Properties.Settings.Default.PropertyChanged += (sender, args) => UpdateSetting(args.PropertyName);
        }

        private void UpdateSetting(String name)
        {
            var converter = new KeyGestureConverter();
            switch (name)
            {
                case "QuitShortcut":
                    Quit = converter.ConvertFrom(Properties.Settings.Default[name]) as KeyGesture;
                    break;
                case "PinShortcut":
                    PinSelectedClip = converter.ConvertFrom(Properties.Settings.Default[name]) as KeyGesture;
                    break;
                case "DeleteShortcut":
                    DeleteSelectedClip = converter.ConvertFrom(Properties.Settings.Default[name]) as KeyGesture;
                    break;
                case "CopyShortcut":
                    CopySelectedClip = converter.ConvertFrom(Properties.Settings.Default[name]) as KeyGesture;
                    break;
                case "ClearAndFocusShortcut":
                    ClearAndFocusSearchBox = converter.ConvertFrom(Properties.Settings.Default[name]) as KeyGesture;
                    break;
                case "FocusShortcut":
                    FocusSearchBox = converter.ConvertFrom(Properties.Settings.Default[name]) as KeyGesture;
                    break;
                case "FocusWindowShortcut":
                    FocusWindowHotkey = converter.ConvertFrom(Properties.Settings.Default[name]) as KeyGesture;
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
