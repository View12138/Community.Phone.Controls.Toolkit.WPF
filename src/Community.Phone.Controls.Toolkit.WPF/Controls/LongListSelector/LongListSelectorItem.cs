// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Describes one element of the flattened sequence the control virtualizes:
    /// the list header, a group header, an item, a group footer or the list footer.
    /// </summary>
    internal sealed class LongListSelectorItem
    {
        internal LongListSelectorItem(object data, LongListSelectorItemKind itemKind, object group)
        {
            Data = data;
            ItemKind = itemKind;
            Group = group;
        }

        /// <summary>
        /// Gets the object the element renders: the item itself for items,
        /// the list header/footer payload for the list parts and the group
        /// object for group headers and footers.
        /// </summary>
        internal object Data { get; }

        /// <summary>
        /// Gets the kind of element.
        /// </summary>
        internal LongListSelectorItemKind ItemKind { get; }

        /// <summary>
        /// Gets the group that contains the item, if any.
        /// </summary>
        internal object Group { get; }

        public override string ToString()
        {
            return ItemKind + ": " + (Data ?? "<null>");
        }
    }

    /// <summary>
    /// Selects the DataTemplate for a flattened element. WPF asks the selector
    /// before the container has a content, so the templates stay owned by the
    /// LongListSelector instance instead of the internal items host.
    /// </summary>
    internal sealed class LongListSelectorTemplateSelector : DataTemplateSelector
    {
        private readonly LongListSelector _owner;

        internal LongListSelectorTemplateSelector(LongListSelector owner)
        {
            _owner = owner;
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return item is LongListSelectorItem entry ? _owner.GetTemplateFor(entry) : null;
        }
    }
}
