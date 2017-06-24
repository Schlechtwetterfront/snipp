using System;
using System.Collections.Generic;
using System.Linq;
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

namespace clipman
{ 
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ViewModels.ClipListViewModel clipViewModel;

        public MainWindow()
        {
            InitializeComponent();

            clipViewModel = new ViewModels.ClipListViewModel();
            clipList.DataContext = clipViewModel;
        }

        private void btnPaste_Click(object sender, RoutedEventArgs e)
        {
            var clip = Models.Clip.Capture();
            clipViewModel.AddClip(clip);
        }

        private void Search(object sender, TextChangedEventArgs e)
        {
            clipViewModel.FilterString = searchBox.Text;
            clipViewModel.ClipView.Refresh();
        }
    }
}
