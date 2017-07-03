using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clipman.ViewModels
{
    class SettingsPanelViewModel
    {
        public void ClearClips()
        {
            ClearRequested?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ClearRequested;
    }
}
