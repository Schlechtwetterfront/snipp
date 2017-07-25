using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clipman.Settings
{
    public class Theme
    {
        public static List<Theme> Themes = new List<Theme> {
            new Theme("Light", @"Resources\Themes\LightTheme.xaml"),
            new Theme("Dark", @"Resources\Themes\DarkTheme.xaml"),
        };

        public String Name
        {
            get;
            set;
        }

        public String FilePath
        {
            get;
            set;
        }

        public Theme(String name, String filePath)
        {
            Name = name;
            FilePath = filePath;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
