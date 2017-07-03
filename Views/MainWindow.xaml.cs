using clipman.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace clipman
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ViewModels.ClipListViewModel clipViewModel;
        ViewModels.StatusBarViewModel statusBarViewModel;
        Clipboard.ClipboardManager clipboardManager;

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

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            clipViewModel = new ViewModels.ClipListViewModel();
            clipList.DataContext = clipViewModel;

            statusBarViewModel = new ViewModels.StatusBarViewModel();
            statusBar.DataContext = statusBarViewModel;

            clipboardManager = new Clipboard.ClipboardManager();
            clipboardManager.ClipCaptured += OnClipCaptured;

            InitializeKeybindings();

            Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.Name);

            Utility.Logging.Log("Width: " + mainGrid.ActualWidth + ", " + mainGrid.Width);
            settingsPanel.Margin = new Thickness { Left = mainGrid.ActualWidth };

#if DEBUG
            Left = -1400;
#endif
        }

        void InitializeKeybindings()
        {
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

        #endregion

        #region Commands

        private void Copy(int index = 0)
        {
            clipViewModel.ClipView.NthInView<ViewModels.ClipViewModel>(index)?.Clip?.Copy();
        }

        #endregion

        private void OnSettingsPanelLoad(object sender, RoutedEventArgs e)
        {
            Utility.Logging.Log("Width: " + mainGrid.ActualWidth + ", " + mainGrid.Width);
            settingsPanel.Margin = new Thickness { Left = mainGrid.ActualWidth };
        }

        private void OnSettingsToggle(object sender, RoutedEventArgs e)
        {
            var slideStoryboard = FindResource("SettingsSlide") as Storyboard;
            foreach (ThicknessAnimation anim in slideStoryboard.Children)
            {
                Thickness margin;
                if (settingsPanelOpen)
                {
                    margin = new Thickness();
                }
                else
                {
                    margin = new Thickness { Left = mainGrid.ActualWidth };
                }
                anim.To = margin;
            }
            settingsPanel.BeginStoryboard(slideStoryboard);

            var rotateStoryboard = FindResource("SettingsButtonRotate") as Storyboard;
            foreach (DoubleAnimation anim in rotateStoryboard.Children)
            {
                anim.To = settingsPanelOpen ? 0 : 180;
            }
            settingsPanelToggleButton.BeginStoryboard(rotateStoryboard);

            settingsPanelOpen = !settingsPanelOpen;
        }
    }
}
