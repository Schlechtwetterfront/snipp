﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
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

            clipViewModel = new ViewModels.ClipListViewModel();
            clipList.DataContext = clipViewModel;

            clipboardMonitor = new ClipboardMonitor.ClipboardMonitor();
            clipboardMonitor.ClipboardChanged += ClipboardChanged;
        }

        void Search(object sender, EventArgs e)
        {
            Console.WriteLine("Searching...");
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
            Console.WriteLine("Clipboard changed");

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

        private void btnPaste_Click(object sender, RoutedEventArgs e)
        {
            var clip = Models.Clip.Capture();
            clipViewModel.AddClip(clip);
            Console.WriteLine(clipViewModel.FilterString);
        }

        private void test_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void searchBox_Changed(object sender, TextChangedEventArgs e)
        {
            Console.WriteLine("Text changed to " + searchBox.Text);
            if (searchTimer == null)
            {
                searchTimer = new DispatcherTimer();
                searchTimer.Interval = TimeSpan.FromMilliseconds(searchDelay);
                searchTimer.Tick += new EventHandler(Search);
            }
            searchTimer.Stop();
            searchTimer.Start();
        }
    }
}
