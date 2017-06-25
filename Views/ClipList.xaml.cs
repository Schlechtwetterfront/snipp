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
        public ClipList()
        {
            InitializeComponent();
        }

        private void Copy(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("Trying to copy");
            var clip = (Models.Clip)(sender as ListBoxItem).DataContext;

            var parentWindow = Window.GetWindow(this) as MainWindow;
            if (parentWindow != null)
            {
                parentWindow.HasJustCopied = true;
            }

            clip.Copy();
        }
    }
}
