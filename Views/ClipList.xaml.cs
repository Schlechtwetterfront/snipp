using System.Windows.Controls;
using System.Windows.Input;

namespace clipman.Views
{
    /// <summary>
    /// Interaction logic for ClipList.xaml
    /// </summary>
    public partial class ClipList : UserControl
    {
        public ClipList()
        {
            InitializeComponent();
        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Utility.Logging.Log("Double-click copy");

            var viewModel = (ViewModels.ClipViewModel)(sender as ListBoxItem).DataContext;
            viewModel.Clip.Copy();
        }
    }
}
