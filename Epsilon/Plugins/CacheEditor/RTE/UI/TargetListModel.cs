using Stylet;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace CacheEditor.RTE.UI
{
    public class TargetListModel : PropertyChangedBase, IEnumerable<TargetListItem>, INotifyCollectionChanged
    {
        private ObservableCollection<TargetListItem> _list;
        private IRteTargetCollection _targetSource;

        public TargetListModel(IRteTargetCollection source)
        {
            _targetSource = source;
            _list = new ObservableCollection<TargetListItem>();
            _list.CollectionChanged += _list_CollectionChanged;
            _targetSource.TargetAdded += _targetSource_TargetAdded;
            _targetSource.TargetRemoved += _targetSource_TargetRemoved;
        }

        private void _targetSource_TargetRemoved(IRteTarget target)
        {
            var item = _list.FirstOrDefault(x => x.Target == target);
            _list.Remove(item);
        }

        private void _targetSource_TargetAdded(IRteTarget target)
        {
            _list.Add(new TargetListItem() { Target = target, DisplayName = target.DisplayName });
        }

        private void _list_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        public new void Refresh()
        {
            _targetSource.Refresh();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public IEnumerator<TargetListItem> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
