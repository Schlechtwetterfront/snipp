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
        ClipboardMonitor.ClipboardMonitor clipboardMonitor;

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
        /// This is set when this application modifies the clipboard.
        /// </summary>
        public bool HasJustCopied
        {
            get;
            set;
        }

        public DateTime LastPasteTime
        {
            get;
            set;
        }
        int pasteDelay = 333;

        DispatcherTimer searchTimer;
        int searchDelay = 333;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            clipViewModel = new ViewModels.ClipListViewModel();
            clipList.DataContext = clipViewModel;

            clipboardMonitor = new ClipboardMonitor.ClipboardMonitor();
            clipboardMonitor.ClipboardChanged += ClipboardChanged;

            InitializeKeybindings();

            this.Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.Name);
        }

        void InitializeKeybindings()
        {
        }

        public void CopyClip(Models.Clip clip)
        {
            HasJustCopied = clip != null;
            clip?.Copy();
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
            clipViewModel.ClipView.Refresh();
            timer.Stop();
        }

        private void ClipboardChanged(object sender, EventArgs e)
        {
            Utility.Logging.Log(
                String.Format(
                    "Clipboard changed, HasJustCopied = {0}, Content = {1}",
                    HasJustCopied,
                    Clipboard.GetText()
                )
            );

            // TODO Wait some time (500ms) until we capture.

            var clip = Models.Clip.Capture();

            var ts = DateTime.Now - LastPasteTime;

            if (clip != null && !HasJustCopied && ts.Milliseconds > pasteDelay)
            {
                LastPasteTime = DateTime.Now;
                clipViewModel.AddClip(clip);
            }
            // Reset to enable capturing for next time something is copied.
            HasJustCopied = false;
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

        private void Copy(int index=0)
        {
            CopyClip(clipViewModel.ClipView.NthInView<Models.Clip>(index));
        }

        #endregion
    }
}
