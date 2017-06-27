using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace clipman.Clipboard
{
    public class ClipboardManager
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
        const int clipboardEventDelay = 333;

        DateTime lastClipboardCopy = DateTime.Now;
        const int afterCopyDelay = 333;

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
            var span = DateTime.Now - lastClipboardCopy;

            // Ignore any immediate events after we copied a clip to the clipboard.
            if (span > TimeSpan.FromMilliseconds(afterCopyDelay))
            {
                clipboardEventTimer.Stop();
                clipboardEventTimer.Start();
            }
            else
            {
                Utility.Logging.Log("Ignored ClipboardChange");
            }
        }

        void OnClipboardDelayFinished(object sender, EventArgs e)
        {
            var clip = CaptureClipboard();
            clip.Copied += OnCopyClip;

            var args = new ClipEventArgs();
            args.Clip = clip;

            ClipCaptured?.Invoke(this, args);

            clipboardEventTimer.Stop();
        }

        void OnCopyClip(object sender, ClipEventArgs e)
        {
            lastClipboardCopy = DateTime.Now;
        }

        public Clip CaptureClipboard()
        {
            return Clip.Capture();
        }

        public event EventHandler<ClipEventArgs> ClipCaptured;
    }
}
