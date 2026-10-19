// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// A container for a <see cref="ListPicker"/> item.
    /// </summary>
    [TemplateVisualState(Name = "Unselected", GroupName = "SelectionStates")]
    [TemplateVisualState(Name = "Selected", GroupName = "SelectionStates")]
    public class ListPickerItem : ContentControl
    {
        private bool _isSelected;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListPickerItem"/> class.
        /// </summary>
        public ListPickerItem()
        {
        }

        static ListPickerItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ListPickerItem),
                new FrameworkPropertyMetadata(typeof(ListPickerItem)));
        }

        internal bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                VisualStateManager.GoToState(this, value ? "Selected" : "Unselected", true);
            }
        }

        /// <inheritdoc />
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            VisualStateManager.GoToState(this, _isSelected ? "Selected" : "Unselected", false);
        }
    }
}
