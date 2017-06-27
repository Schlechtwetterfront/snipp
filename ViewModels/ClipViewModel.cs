using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace clipman.ViewModels
{
    public class ClipViewModel
    {
        protected Clipboard.Clip clip;
        public Clipboard.Clip Clip
        {
            get
            {
                return clip;
            }
            set
            {
                clip = value;
            }
        }

        public String CreatedString
        {
            get
            {
                if (clip.Created.Date == DateTime.Today)
                {
                    return clip.Created.ToShortTimeString();
                }
                else
                {
                    return clip.Created.ToString("d");
                }
            }
        }

        public ClipViewModel(Clipboard.Clip clip)
        {
            this.clip = clip;
        }
    }
}
