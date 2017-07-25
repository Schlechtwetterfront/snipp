using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace clipman.Views
{
    /// <summary>
    /// Interaction logic for ClipList.xaml
    /// </summary>
    public partial class ClipList : UserControl
    {
        private ICommand openCtxtMenuCommand;
        public ICommand OpenCtxtMenuCommand
        {
            get
            {
                return openCtxtMenuCommand ?? (openCtxtMenuCommand = new Commands.Command(param =>
                {
                    if (param is ListBoxItem) {
                        var item = param as ListBoxItem;
                        Utility.Logging.Log(((ViewModels.ClipViewModel)item.DataContext).Clip.Title);
                        Utility.Logging.Log(((ViewModels.ClipViewModel)item.DataContext).Pinned.ToString());
                        item.IsSelected = true;
                        item.ContextMenu.PlacementTarget = item;
                        item.ContextMenu.IsOpen = true;
                    }
                }));
            }
        }

        public ClipList()
        {
            InitializeComponent();

            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);
        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Utility.Logging.Log("Double-click copy");

            var viewModel = (ViewModels.ClipViewModel)(sender as ListBoxItem).DataContext;
            viewModel.Clip.Copy();
            //Echo(sender as ListBoxItem);
        }

        private void Echo(ListBoxItem control)
        {
            var echoAnim = FindResource("Echo") as Storyboard;
            echoAnim.Begin(control, control.Template);
        }

        private void OnClearButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            var viewModel = (ViewModels.ClipListViewModel)DataContext;
            viewModel.Clips.Clear();
        }
    }
}
