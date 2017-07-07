using clipman.Utility;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace clipman
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int settingsPanelOffset = 200;

        ViewModels.ClipListViewModel clipViewModel;
        ViewModels.SettingsPanelViewModel settingsPanelViewModel;
        Clipboard.ClipboardManager clipboardManager;
        Settings.KeyboardMonitor keyboardMonitor;

        Settings.Settings settings = new Settings.Settings();
        public Settings.Settings Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// Copy nth clip by pressing Ctrl+N.
        /// </summary>
        private ICommand copyCommand;
        public ICommand CopyCommand
        {
            get
            {
                return copyCommand ?? (copyCommand = new Commands.Command(param =>
                {
                    Utility.Logging.Log(String.Format("Copy shortcut (Ctrl+{0})", param.ToString()));
                    Copy(Int32.Parse(param.ToString()));
                }));
            }
        }

        /// <summary>
        /// Clear and focus search box.
        /// </summary>
        private ICommand clearCommand;
        public ICommand ClearCommand
        {
            get
            {
                return clearCommand ?? (clearCommand = new Commands.Command(param =>
                {
                    ToggleSettingsPanel(false);
                    searchBox.Clear();
                    searchBox.Focus();
                }));
            }
        }

        /// <summary>
        /// Focus text box.
        /// </summary>
        private ICommand focusSearchCommand;
        public ICommand FocusSearchCommand
        {
            get
            {
                return focusSearchCommand ?? (focusSearchCommand = new Commands.Command(param =>
                {
                    ToggleSettingsPanel(false);
                    searchBox.Focus();
                    searchBox.SelectAll();
                }));
            }
        }

        /// <summary>
        /// Delay after last input until search is started.
        /// </summary>
        DispatcherTimer searchTimer;
        int searchDelay = 260;

        bool settingsPanelOpen = false;

        int focusHotkeyId;

        public MainWindow()
        {
            InitializeComponent();

            // Try to sharpen text.
            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);

            DataContext = this;

            clipViewModel = new ViewModels.ClipListViewModel();
            clipList.DataContext = clipViewModel;

            settingsPanelViewModel = new ViewModels.SettingsPanelViewModel();
            settingsPanel.DataContext = settingsPanelViewModel;

            settingsPanelViewModel.ClearRequested += OnClearRequested;

            clipboardManager = new Clipboard.ClipboardManager();
            clipboardManager.ClipCaptured += OnClipCaptured;

            keyboardMonitor = new Settings.KeyboardMonitor();
            keyboardMonitor.KeyPressed += OnHotkeyPressed;

            InitializeKeybindings();

            Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.Name);

#if DEBUG
            Left = -1400;
#endif
        }

        /// <summary>
        /// Initialize special keybindings like global shortcuts.
        /// </summary>
        void InitializeKeybindings()
        {
            // Global bring window to front shortcut.
            focusHotkeyId = keyboardMonitor.AddHotkey(
                (int)settings.FocusWindowHotkey.Modifiers,
                KeyInterop.VirtualKeyFromKey(settings.FocusWindowHotkey.Key)
            );
        }

        #region Callbacks

        void Search(object sender, EventArgs e)
        {
            var timer = sender as DispatcherTimer;
            if (timer == null)
            {
                return;
            }

            clipViewModel.FilterString = searchBox.Text;
            timer.Stop();
        }

        private void OnClipCaptured(object sender, Clipboard.ClipboardManager.ClipEventArgs e)
        {
            Utility.Logging.Log(
                String.Format(
                    "OnClipCaptured, Content = {0}",
                    System.Windows.Clipboard.GetText()
                )
            );

            var clip = e.Clip;

            if (clip != null)
            {
                clipViewModel.AddClip(clip);
            }
        }

        private void searchBox_Changed(object sender, TextChangedEventArgs e)
        {
            if (searchTimer == null)
            {
                searchTimer = new DispatcherTimer();
                searchTimer.Interval = TimeSpan.FromMilliseconds(searchDelay);
                searchTimer.Tick += new EventHandler(Search);
            }
            searchTimer.Stop();
            searchTimer.Start();
        }

        private void OnClearRequested(object sender, EventArgs e)
        {
            clipViewModel.Clips.Clear();
        }

        #endregion

        #region Commands

        private void Copy(int index = 0)
        {
            clipViewModel.ClipView.NthInView<ViewModels.ClipViewModel>(index)?.Clip?.Copy();
        }

        #endregion

        private void OnSettingsPanelLoad(object sender, RoutedEventArgs e)
        {
        }

        private void OnSettingsToggle(object sender, RoutedEventArgs e)
        {
            ToggleSettingsPanel(!settingsPanelOpen);
        }

        public void ToggleSettingsPanel(bool open)
        {
            if (open == settingsPanelOpen)
            {
                return;
            }

            Storyboard bubbleStoryboard;
            if (settingsPanelOpen)
            {
                bubbleStoryboard = FindResource("SettingsBubbleDown") as Storyboard;
            }
            else
            {
                bubbleStoryboard = FindResource("SettingsBubbleUp") as Storyboard;
            }
            settingsPanel.BeginStoryboard(bubbleStoryboard);

            Storyboard buttonAnim;
            if (settingsPanelOpen)
            {
                buttonAnim = FindResource("XToBurger") as Storyboard;
            }
            else
            {
                buttonAnim = FindResource("BurgerToX") as Storyboard;
            }
            buttonAnim.Begin(settingsPanelToggleButton, settingsPanelToggleButton.Template);

            settingsPanelOpen = open;
        }

        /// <summary>
        /// Global shortcut listener.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHotkeyPressed(object sender, Settings.KeyboardMonitor.HotkeyEventArgs e)
        {
            if (e.HotkeyId == focusHotkeyId)
            {
                // Bring window to front.
                FocusWindow();
            }
        }

        /// <summary>
        /// Brings window to front and focuses search box.
        /// </summary>
        public void FocusWindow()
        {
            Activate();
            searchBox.Focus();
            searchBox.SelectAll();
        }
    }
}
