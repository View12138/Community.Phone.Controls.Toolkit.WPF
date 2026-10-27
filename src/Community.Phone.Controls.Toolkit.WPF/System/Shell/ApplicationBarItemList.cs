// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Phone.Shell
{
    /// <summary>
    /// The list behind <see cref="ApplicationBar.Buttons"/> and
    /// <see cref="ApplicationBar.MenuItems"/>. It keeps the original limits and
    /// exceptions, and creates a container per item so the presenter can render
    /// it.
    /// </summary>
    /// <typeparam name="TApplicationBarItem">The kind of item in the list.</typeparam>
    internal class ApplicationBarItemList<TApplicationBarItem> : IList, IList<TApplicationBarItem>
        where TApplicationBarItem : IApplicationBarMenuItem
    {
        /// <summary>
        /// The maximum number of items the list accepts.
        /// </summary>
        internal int MaxItems;

        private readonly List<TApplicationBarItem> _list = new List<TApplicationBarItem>();
        private readonly Dictionary<TApplicationBarItem, ApplicationBarItemContainer> _containers =
            new Dictionary<TApplicationBarItem, ApplicationBarItemContainer>();

        private readonly ApplicationBarItemType _itemType;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationBarItemList{TApplicationBarItem}"/> class.
        /// </summary>
        /// <param name="itemType">The kind of element the items are rendered as.</param>
        internal ApplicationBarItemList(ApplicationBarItemType itemType)
        {
            _itemType = itemType;
        }

        /// <summary>
        /// Occurs when the list or a tracked item changed.
        /// </summary>
        internal event EventHandler Changed;

        /// <summary>
        /// Gets the number of items in the list.
        /// </summary>
        public int Count
        {
            get { return _list.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the list is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the list has a fixed size.
        /// </summary>
        bool IList.IsFixedSize
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the list is synchronized.
        /// </summary>
        public bool IsSynchronized
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the object used to synchronize access to the list.
        /// </summary>
        public object SyncRoot
        {
            get { return this; }
        }

        /// <summary>
        /// Gets or sets the item at the given index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The item.</returns>
        public TApplicationBarItem this[int index]
        {
            get
            {
                return _list[index];
            }

            set
            {
                RemoveAt(index);
                Insert(index, value);
            }
        }

        /// <summary>
        /// Gets or sets the item at the given index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The item.</returns>
        object IList.this[int index]
        {
            get
            {
                return this[index];
            }

            set
            {
                this[index] = (TApplicationBarItem)value;
            }
        }

        /// <summary>
        /// Gets the container that renders the item at the given index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The container.</returns>
        internal ApplicationBarItemContainer GetContainer(int index)
        {
            return _containers[_list[index]];
        }

        /// <summary>
        /// Returns the index of an item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The index, or -1.</returns>
        public int IndexOf(TApplicationBarItem item)
        {
            return _list.IndexOf(item);
        }

        /// <summary>
        /// Inserts an item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        public void Insert(int index, TApplicationBarItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (index < 0 || index > _list.Count)
            {
                throw new ArgumentOutOfRangeException("item");
            }

            if (_list.Count == MaxItems)
            {
                throw new InvalidOperationException("Too many items in list");
            }

            if (_containers.ContainsKey(item))
            {
                throw new InvalidOperationException("Cannot add same item to multiple lists at once or to same list twice");
            }

            ApplicationBarItemContainer container = ApplicationBarItemContainerFactory.GetApplicationBarItemContainer(item, _itemType);
            try
            {
                container.Attach();
            }
            catch
            {
                container.Detach();
                throw;
            }

            container.Changed += OnItemChanged;
            _containers[item] = container;
            _list.Insert(index, item);

            OnChanged();
        }

        /// <summary>
        /// Removes the item at the given index.
        /// </summary>
        /// <param name="index">The index.</param>
        public void RemoveAt(int index)
        {
            TApplicationBarItem item = _list[index];
            ApplicationBarItemContainer container = _containers[item];

            _list.RemoveAt(index);
            _containers.Remove(item);

            container.Changed -= OnItemChanged;
            container.Detach();

            OnChanged();
        }

        /// <summary>
        /// Adds an item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Add(TApplicationBarItem item)
        {
            Insert(_list.Count, item);
        }

        /// <summary>
        /// Removes all items.
        /// </summary>
        public void Clear()
        {
            while (_list.Count > 0)
            {
                RemoveAt(0);
            }
        }

        /// <summary>
        /// Determines whether the list contains an item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> when the item is in the list.</returns>
        public bool Contains(TApplicationBarItem item)
        {
            return _list.Contains(item);
        }

        /// <summary>
        /// Copies the items to an array.
        /// </summary>
        /// <param name="array">The destination array.</param>
        /// <param name="arrayIndex">The destination index.</param>
        public void CopyTo(TApplicationBarItem[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Copies the items to an array.
        /// </summary>
        /// <param name="array">The destination array.</param>
        /// <param name="index">The destination index.</param>
        public void CopyTo(Array array, int index)
        {
            ((ICollection)_list).CopyTo(array, index);
        }

        /// <summary>
        /// Removes an item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> when the item was removed.</returns>
        public bool Remove(TApplicationBarItem item)
        {
            int index = _list.IndexOf(item);
            if (index == -1)
            {
                return false;
            }

            RemoveAt(index);
            return true;
        }

        /// <summary>
        /// Returns an enumerator over the items.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<TApplicationBarItem> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator over the items.
        /// </summary>
        /// <returns>The enumerator.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        /// <summary>
        /// Adds an item.
        /// </summary>
        /// <param name="value">The item.</param>
        /// <returns>The new count.</returns>
        int IList.Add(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("item");
            }

            if (!(value is TApplicationBarItem item))
            {
                throw new ArgumentException("This list does not support that type");
            }

            Add(item);
            return _list.Count;
        }

        /// <summary>
        /// Determines whether the list contains an item.
        /// </summary>
        /// <param name="value">The item.</param>
        /// <returns><c>true</c> when the item is in the list.</returns>
        bool IList.Contains(object value)
        {
            return value is TApplicationBarItem item && Contains(item);
        }

        /// <summary>
        /// Returns the index of an item.
        /// </summary>
        /// <param name="value">The item.</param>
        /// <returns>The index, or -1.</returns>
        int IList.IndexOf(object value)
        {
            return value is TApplicationBarItem item ? IndexOf(item) : -1;
        }

        /// <summary>
        /// Inserts an item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The item.</param>
        void IList.Insert(int index, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("item");
            }

            if (!(value is TApplicationBarItem item))
            {
                throw new ArgumentException("This list does not support that type");
            }

            Insert(index, item);
        }

        /// <summary>
        /// Removes an item.
        /// </summary>
        /// <param name="value">The item.</param>
        void IList.Remove(object value)
        {
            if (value is TApplicationBarItem item)
            {
                Remove(item);
            }
        }

        private void OnItemChanged(object sender, EventArgs e)
        {
            OnChanged();
        }

        private void OnChanged()
        {
            EventHandler handler = Changed;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}
