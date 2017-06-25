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
            get;
            private set;
        }

        public String SearchContent
        {
            get;
            private set;
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
            try
            {
                return new Clip(Clipboard.GetText());
            } catch (System.Runtime.InteropServices.ExternalException e)
            {
                Console.WriteLine("Failed to read clipboard");
                return null;
            }
        }

        public bool Matches(String searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
            {
                return true;
            }

            if (SearchContent.Contains(searchString))
            {
                return true;
            }

            if (Created.ToShortDateString().Contains(searchString))
            {
                return true;
            }

            return false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
