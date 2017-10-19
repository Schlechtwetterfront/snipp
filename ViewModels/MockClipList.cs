
namespace clipman.ViewModels
{
    class MockClipList : ClipListViewModel
    {
        public MockClipList()
        {
            AddClip(new Clipboard.Clip("AlternationCount=\"{Binding Items.Count, RelativeSource={RelativeSource Self}}\""));
            var pinnedClip = new Clipboard.Clip("rgba(255, 2, 50, .3)");
            var clipVM = new ClipViewModel(pinnedClip);
            clipVM.Pinned = true;
            Clips.Add(clipVM);
            AddClip(new Clipboard.Clip("#0bb"));
        }
    }
}
