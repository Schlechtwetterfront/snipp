using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace clipman.Clipboard
{
    class ClipboardManager
    {
        public class ClipEventArgs : EventArgs
        {
            public Clip Clip
            {
                get;
                set;
            }
        }

        ClipboardMonitor clipboardMonitor;

        DispatcherTimer clipboardEventTimer;
        int clipboardEventDelay = 500;

        public bool JustCopied
        {
            get;
            set;
        }

        public ClipboardManager()
        {
            clipboardMonitor = new ClipboardMonitor();
            clipboardMonitor.ClipboardChanged += OnClipboardChanged;

            clipboardEventTimer = new DispatcherTimer();
            clipboardEventTimer.Interval = TimeSpan.FromMilliseconds(clipboardEventDelay);
            clipboardEventTimer.Tick += new EventHandler(OnClipboardDelayFinished);
        }

        void OnClipboardChanged(object sender, EventArgs e)
        {
            clipboardEventTimer.Stop();
            clipboardEventTimer.Start();
        }

        void OnClipboardDelayFinished(object sender, EventArgs e)
        {
            var clip = CaptureClipboard();
            var args = new ClipEventArgs();
            args.Clip = clip;
            ClipCaptured?.Invoke(this, args);
        }

        public Clip CaptureClipboard()
        {
            return Clip.Capture();
        }

        public void CopyToClipboard(Clip clip)
        {
            // TODO Set JustCopied?
            clip.Copy();
        }

        public event EventHandler ClipCaptured;
    }
}
