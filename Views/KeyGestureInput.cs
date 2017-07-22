using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace clipman.Views
{
    class KeyGestureInput : TextBox
    {
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl
                || e.Key == Key.LeftShift || e.Key == Key.RightShift
                || e.Key == Key.LeftAlt || e.Key == Key.RightAlt
                || e.Key == Key.Tab)
                return;

            var converter = new KeyGestureConverter();
            try
            {
                var gesture = new KeyGesture(e.Key, e.KeyboardDevice.Modifiers);
                var text = converter.ConvertToString(gesture);
                Text = text;
            }
            catch (NotSupportedException)
            {
                MessageBox.Show(
                    "Shortcut needs to either be a special key (Escape, Function Keys) or have a modifier (Alt, Ctrl).",
                    "Invalid Shortcut"
                );
            }

            e.Handled = true;
        }
    }
}
