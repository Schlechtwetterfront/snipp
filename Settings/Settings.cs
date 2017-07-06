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

        private KeyGesture focusWindowHotkey = new KeyGesture(Key.OemTilde, ModifierKeys.Control);
        public KeyGesture FocusWindowHotkey
        {
            get { return focusWindowHotkey; }
        }

        private KeyGesture focusSearchBox = new KeyGesture(Key.F, ModifierKeys.Control);
        public KeyGesture FocusSearchBox
        {
            get { return focusSearchBox; }
        }

        private KeyGesture clearAndFocusSearchBox = new KeyGesture(Key.Escape);
        public KeyGesture ClearAndFocusSearchBox
        {
            get { return clearAndFocusSearchBox; }
        }

        private KeyGesture copySelectedClip = new KeyGesture(Key.C, ModifierKeys.Control);
        public KeyGesture CopySelectedClip
        {
            get { return copySelectedClip; }
        }
    }
}
