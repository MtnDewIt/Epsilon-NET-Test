using System;
using TagTool.Cache;

namespace CacheEditor.Components.TagTree
{
    public class TagTreeTagNode : TagTreeNode 
    {
        private Func<string> _textDelegate;

        public TagTreeTagNode(CachedTag tag, Func<string> textDelegate)
        {
            Tag = tag;
            _textDelegate = textDelegate;
            UpdateName();
        }

        public void UpdateName()
        {
            Text = _textDelegate();
        }
    }
}
