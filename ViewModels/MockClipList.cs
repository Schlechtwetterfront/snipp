
namespace clipman.ViewModels
{
    class MockClipList : ClipListViewModel
    {
        public MockClipList()
        {
            AddClip(new Clipboard.Clip("AlternationCount=\"{Binding Items.Count, RelativeSource={RelativeSource Self}}\""));
            var pinnedClip = new Clipboard.Clip("Clip 2");
            var clipVM = new ClipViewModel(pinnedClip);
            clipVM.Pinned = true;
            Clips.Add(clipVM);
            AddClip(new Clipboard.Clip("Clip 3"));
        }
    }
}
