// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Microsoft.Phone.Controls.Primitives
{
    /// <summary>
    /// One entry of the <see cref="PivotHeadersControl"/> strip. A header item
    /// is faded while it is not selected and raises the selection when it is
    /// clicked.
    /// </summary>
    [TemplateVisualState(Name = UnselectedState, GroupName = SelectionStatesGroup)]
    [TemplateVisualState(Name = SelectedState, GroupName = SelectionStatesGroup)]
    public class PivotHeaderItem : ContentControl
    {
        private const string SelectedState = "Selected";
        private const string UnselectedState = "Unselected";
        private const string SelectionStatesGroup = "SelectionStates";

        /// <summary>
        /// Identifies the <see cref="IsSelected"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            "IsSelected",
            typeof(bool),
            typeof(PivotHeaderItem),
            new PropertyMetadata(false, OnIsSelectedPropertyChanged));

        /// <summary>
        /// Initializes a new instance of the <see cref="PivotHeaderItem"/> class.
        /// </summary>
        public PivotHeaderItem()
        {
            DefaultStyleKey = typeof(PivotHeaderItem);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the header is the selected one.
        /// </summary>
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        /// <summary>
        /// Gets or sets the header control that owns this item.
        /// </summary>
        internal PivotHeadersControl ParentHeadersControl { get; set; }

        /// <summary>
        /// Gets or sets the pivot item or data item this header represents.
        /// </summary>
        internal object Item { get; set; }

        /// <inheritdoc/>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            UpdateVisualStates(false);
        }

        /// <inheritdoc/>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (e != null && !e.Handled)
            {
                e.Handled = true;
                if (ParentHeadersControl != null)
                {
                    ParentHeadersControl.OnHeaderItemClicked(this);
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            if (ParentHeadersControl != null)
            {
                ParentHeadersControl.InvalidateMeasure();
            }
        }

        private static void OnIsSelectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PivotHeaderItem item = d as PivotHeaderItem;
            if (item != null && item.ParentHeadersControl != null)
            {
                item.ParentHeadersControl.NotifyHeaderItemSelected(item, (bool)e.NewValue);
                item.UpdateVisualStates(true);
            }
        }

        private void UpdateVisualStates(bool useTransitions)
        {
            VisualStateManager.GoToState(this, IsSelected ? SelectedState : UnselectedState, useTransitions);
        }
    }
}
