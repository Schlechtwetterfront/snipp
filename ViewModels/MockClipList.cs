using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clipman.ViewModels
{
    class MockClipList : ClipListViewModel
    {
        public MockClipList()
        {
            AddClip(new Clipboard.Clip("Clip <bold>Test</bold> 1"));
            AddClip(new Clipboard.Clip("Clip 2"));
            AddClip(new Clipboard.Clip("Clip 3"));
        }
    }
}
