using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Payments.Adapters {
    public class DynamicCollection : IEnumerable<IModelWrapper>, INotifyCollectionChanged {
        private readonly IEnumerable _source;
        private readonly INotifyCollectionChanged _notifyCollectionChanged;
        private readonly List<IModelWrapper> _items = new List<IModelWrapper>();

        private readonly bool _noItem;
        private readonly bool _addItem;

        public DynamicCollection(IEnumerable source, bool noItem, bool addItem) {
            _noItem = noItem;
            _addItem = addItem;
            _source = source;
            _notifyCollectionChanged = source as INotifyCollectionChanged;
            if (_notifyCollectionChanged == null) {
                throw new InvalidOperationException("Collection is not INotifyCollectionChanged");
            }

            _notifyCollectionChanged.CollectionChanged += OnSourceChanged;

            Reset();
        }

        public IEnumerator<IModelWrapper> GetEnumerator() {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        private void OnSourceChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs) {
            Reset();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void Reset() {
            _items.Clear();

            if (_noItem) _items.Add(new NoItemModelWrapper());
            if (_addItem) _items.Add(new CreateItemModelWrapper());

            foreach (var item in _source) {
                _items.Add(new ItemModelWrapper(item));
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;


        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
            CollectionChanged?.Invoke(this, e);
        }
    }
}