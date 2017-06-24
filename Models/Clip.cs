using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;

namespace clipman.Models
{
    public class Clip : INotifyPropertyChanged
    {
        public String Content
        {
            get;
            set;
        }

        public DateTime Created
        {
            get;
            set;
        }

        public String Title
        {
            get { return Content.Trim(); }
        }

        public Clip()
        {
        }

        public Clip(String content)
        {
            Content = content;
            Created = DateTime.Now;
        }

        public static Clip Capture()
        {
            return new Clip(Clipboard.GetText());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
