using clipman.Settings;
using clipman.Utility;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
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

        #region Commands

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
                    if (settingsPanelOpen)
                    {
                        ToggleSettingsPanel(false);
                    }
                    else
                    {
                        searchBox.Clear();
                        searchBox.Focus();
                    }
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

        private ICommand quitCommand;
        public ICommand QuitCommand
        {
            get
            {
                return quitCommand ?? (quitCommand = new Commands.Command(param =>
                {
                    Utility.Logging.Log("Quit");
                    Close();
                }));
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
                    Utility.Logging.Log("copy");
                    (param as ViewModels.ClipViewModel)?.Clip.Copy();
                }));
            }
        }

        /// <summary>
        /// Command pinning the clip it was used on (Ctrl+P on selection).
        /// </summary>
        private ICommand pinClipCommand;
        public ICommand PinClipCommand
        {
            get
            {
                return pinClipCommand ?? (pinClipCommand = new Commands.Command(param =>
                {
                    Utility.Logging.Log("pin " + param?.GetType());
                    if (param is ViewModels.ClipViewModel)
                    {
                        var cvm = (ViewModels.ClipViewModel)param;
                        cvm.Pinned = !cvm.Pinned;
                    }
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
                    clipViewModel.Clips.Remove(param as ViewModels.ClipViewModel);
                }));
            }
        }

        #endregion

        /// <summary>
        /// Delay after last input until search is started.
        /// </summary>
        DispatcherTimer searchTimer;
        int searchDelay = 260;

        bool settingsPanelOpen = false;

        int focusHotkeyId;

        ResourceDictionary currentTheme;

        public MainWindow()
        {
            InitializeComponent();

            // Try to sharpen text.
            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);

            DataContext = this;

            // Initialize clip list view model.
            clipViewModel = new ViewModels.ClipListViewModel();

            clipboardManager = new Clipboard.ClipboardManager();
            clipboardManager.ClipCaptured += OnClipCaptured;
            clipboardManager.ClipCopied += OnClipCopied;

            // Insert saved snippets from last time.
            var clips = Utility.Storage.RetrieveClips();
            clips.Sort();
            foreach (var clip in clips)
            {
                clipViewModel.AddClip(clip);
                clipboardManager.Subscribe(clip);
            }

            clipList.DataContext = clipViewModel;

            settingsPanelViewModel = new ViewModels.SettingsPanelViewModel(settings);
            settingsPanel.DataContext = settingsPanelViewModel;

            keyboardMonitor = new Settings.KeyboardMonitor();
            keyboardMonitor.KeyPressed += OnHotkeyPressed;

            InitializeKeybindings();

            InitializeTheme();

            InitializeWindow();

            Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.Name);
        }

        /// <summary>
        /// Seet window position and dimensions.
        /// </summary>
        void InitializeWindow()
        {
            Console.WriteLine(String.Format("{0} {1} {2} {3}", Properties.Settings.Default.WindowPositionX, Properties.Settings.Default.WindowPositionY, Properties.Settings.Default.WindowWidth, Properties.Settings.Default.WindowHeight));
            Left = Properties.Settings.Default.WindowPositionX;
            Top = Properties.Settings.Default.WindowPositionY;
            Width = Properties.Settings.Default.WindowWidth;
            Height = Properties.Settings.Default.WindowHeight;
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

            settings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "FocusWindowHotkey")
                {
                    keyboardMonitor.RemoveHotkey(focusHotkeyId);
                    focusHotkeyId = keyboardMonitor.AddHotkey(
                        (int)settings.FocusWindowHotkey.Modifiers,
                        KeyInterop.VirtualKeyFromKey(settings.FocusWindowHotkey.Key)
                    );
                }
            };
        }

        void InitializeTheme()
        {
            var themes = Theme.Themes;
            String themeKey = Properties.Settings.Default["Theme"] as String;

            Theme theme = themes.Find(t => t.Name == themeKey);

            if (theme != null)
            {
                settings.CurrentTheme = theme;
                SetTheme(theme);
            }
            else
            {
                Properties.Settings.Default["Theme"] = themes[0].Name;
                settings.CurrentTheme = themes[0];
                SetTheme(themes[0]);
            }

            settings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "CurrentTheme" && settings.CurrentTheme != null)
                {
                    SetTheme(settings.CurrentTheme);
                    Properties.Settings.Default["Theme"] = settings.CurrentTheme.Name;
                }
            };
        }

        void SetTheme(Theme theme)
        {
            var dict = new ResourceDictionary();
            dict.Source = new Uri(theme.FilePath, UriKind.Relative);

            if (currentTheme != null && dict != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(currentTheme);
            }

            if (dict != null)
            {
                currentTheme = dict;
                Application.Current.Resources.MergedDictionaries.Add(dict);
            }
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
                if (clipViewModel.AddClip(clip))
                {
                    (Resources["BlinkTaskBarOverlay"] as Storyboard)?.Begin();
                }
            }
        }

        private void OnClipCopied(object sender, Clipboard.ClipboardManager.ClipEventArgs e)
        {
            (Resources["StatusBarFlash"] as Storyboard)?.Begin();
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
            Storyboard buttonAnim;

            if (settingsPanelOpen)
            {
                bubbleStoryboard = FindResource("SettingsBubbleDown") as Storyboard;
                buttonAnim = FindResource("XToBurger") as Storyboard;

                searchBox.Focus();
                searchBox.SelectAll();
            }
            else
            {
                bubbleStoryboard = FindResource("SettingsBubbleUp") as Storyboard;
                buttonAnim = FindResource("BurgerToX") as Storyboard;
            }

            settingsPanel.BeginStoryboard(bubbleStoryboard);
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

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                clipList.clipList.SelectedIndex++;
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                clipList.clipList.SelectedIndex = Math.Max(0, clipList.clipList.SelectedIndex - 1);
                e.Handled = true;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Console.WriteLine(String.Format("{0} {1} {2} {3}", Left, Top, Width, Height));
            Properties.Settings.Default.WindowPositionX = Left;
            Properties.Settings.Default.WindowPositionY = Top;
            Properties.Settings.Default.WindowWidth = Width;
            Properties.Settings.Default.WindowHeight = Height;

            Properties.Settings.Default.Save();

            Utility.Storage.SaveSnippets(clipViewModel.Clips.Select(vm => vm.Clip));
        }
    }
}
