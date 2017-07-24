using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace clipman.Views
{
    public class RichTextBlock : TextBlock
    {
        public ObservableCollection<Inline> InlineList
        {
            get { return (ObservableCollection<Inline>)GetValue(InlineListProperty); }
            set { SetValue(InlineListProperty, value); }
        }

        public static readonly DependencyProperty InlineListProperty =
            DependencyProperty.Register("InlineList", typeof(ObservableCollection<Inline>), typeof(RichTextBlock), new UIPropertyMetadata(null, OnPropertyChanged));

        private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            RichTextBlock textBlock = (RichTextBlock)sender;
            if (e.NewValue != null)
            {
                textBlock.Inlines.Clear();
                textBlock.Inlines.AddRange((ObservableCollection<Inline>)e.NewValue);
            }
        }
    }
}
