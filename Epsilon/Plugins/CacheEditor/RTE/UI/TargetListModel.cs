using Stylet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace CacheEditor.RTE.UI
{
    public class TargetListModel :
    PropertyChangedBase,
    IEnumerable<TargetListItem>,
    IEnumerable,
    INotifyCollectionChanged
    {
        private ObservableCollection<TargetListItem> _list;
        private IRteTargetCollection _targetSource;

        public TargetListModel(IRteTargetCollection source)
        {
            _targetSource = source;
            _list = new ObservableCollection<TargetListItem>();
            _list.CollectionChanged += new NotifyCollectionChangedEventHandler(_list_CollectionChanged);
            _targetSource.TargetAdded += new Action<IRteTarget>(_targetSource_TargetAdded);
            _targetSource.TargetRemoved += new Action<IRteTarget>(_targetSource_TargetRemoved);
        }

        private void _targetSource_TargetRemoved(IRteTarget target)
        {
            _list.Remove(_list.FirstOrDefault(x => x.Target == target));
        }

        private void _targetSource_TargetAdded(IRteTarget target)
        {
            _list.Add(new TargetListItem()
            {
                Target = target,
                DisplayName = target.DisplayName
            });
        }

        private void _list_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => CollectionChanged?.Invoke(sender, e);

        public void Refresh() => _targetSource.Refresh();

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public IEnumerator<TargetListItem> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
