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

namespace clipman.Views
{
    /// <summary>
    /// Interaction logic for ClipList.xaml
    /// </summary>
    public partial class ClipList : UserControl
    {
        private ICommand copyClipCommand;
        public ICommand CopyClipCommand
        {
            get
            {
                return copyClipCommand ?? (copyClipCommand = new Commands.Command(param =>
                {
                    Utility.Logging.Log("Enter copy command");
                    CopyClip(param as Models.Clip);
                }));
            }
        }

        public ClipList()
        {
            InitializeComponent();
        }

        private void Copy(object sender, MouseButtonEventArgs e)
        {
            Utility.Logging.Log("Copy callback (double-click)");
            var clip = (Models.Clip)(sender as ListBoxItem).DataContext;

            CopyClip(clip);
        }

        private void CopyClip(Models.Clip clip)
        {
            var parentWindow = Window.GetWindow(this) as MainWindow;
            if (parentWindow != null)
            {
                parentWindow.CopyClip(clip);
            }
        }
    }
}
