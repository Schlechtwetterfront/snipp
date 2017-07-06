using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace clipman.Settings
{
    public class Settings
    {
        public int ClipLimit;

        public KeyGesture FocusWindowHotkey = new KeyGesture(Key.OemTilde, ModifierKeys.Control);
    }
}
