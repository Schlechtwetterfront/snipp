using System;

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
