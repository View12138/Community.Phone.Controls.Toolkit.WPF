// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Microsoft.Phone.Controls.Primitives
{
    /// <summary>
    /// The header strip of a <see cref="Pivot"/>: the headers are laid out in a
    /// horizontal run inside a canvas, and the run is scrolled so that the
    /// selected header always sits at the left edge.
    /// </summary>
    /// <remarks>
    /// The phone version scrolls a run of headers that wraps around and lets the
    /// compositor drive the movement. The port lays the run out the same way —
    /// circular, starting at <see cref="VisualFirstIndex"/>, so the first header
    /// follows the last one — and animates the canvas offset with a
    /// <see cref="QuarticEase"/>. The scroll duration is the original
    /// <c>PivotSeconds</c> constant (0.5 s); the 200 ms lock fade is the duration
    /// the toolkit's <c>LockablePivot</c> used.
    /// </remarks>
    [TemplatePart(Name = CanvasName, Type = typeof(Canvas))]
    [TemplatePart(Name = HeaderItemsPresenterName, Type = typeof(ItemsPresenter))]
    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(PivotHeaderItem))]
    public class PivotHeadersControl : ItemsControl
    {
        private const string CanvasName = "Canvas";
        private const string HeaderItemsPresenterName = "HeaderItemsPresenter";

        /// <summary>
        /// The duration of one header scroll, from the original control.
        /// </summary>
        internal const double HeaderAnimationSeconds = 0.5;

        private static readonly Duration HeaderAnimationDuration =
            new Duration(TimeSpan.FromSeconds(HeaderAnimationSeconds));

        private static readonly Duration LockFadeDuration = new Duration(TimeSpan.FromMilliseconds(200));

        private readonly Dictionary<Control, double> _sizes = new Dictionary<Control, double>();

        private Canvas _canvas;
        private ItemsPresenter _headerItemsPresenter;
        private bool _isDesign;
        private bool _isAnimating;
        private int _animatingIndex;
        private bool _updatePending;
        private bool _ignorePropertyChange;
        private bool _activeSelectionChange;
        private bool _isLocked;

        /// <summary>
        /// Identifies the <see cref="VisualFirstIndex"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty VisualFirstIndexProperty = DependencyProperty.Register(
            "VisualFirstIndex",
            typeof(int),
            typeof(PivotHeadersControl),
            new PropertyMetadata(0, OnVisualFirstIndexPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="ItemContainerStyle"/> dependency property.
        /// </summary>
        public static readonly new DependencyProperty ItemContainerStyleProperty = DependencyProperty.Register(
            "ItemContainerStyle",
            typeof(Style),
            typeof(PivotHeadersControl),
            null);

        /// <summary>
        /// Identifies the <see cref="SelectedIndex"/> dependency property.
        /// </summary>
        internal static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(
            "SelectedIndex",
            typeof(int),
            typeof(PivotHeadersControl),
            new PropertyMetadata(0, OnSelectedIndexPropertyChanged));

        /// <summary>
        /// Initializes a new instance of the <see cref="PivotHeadersControl"/> class.
        /// </summary>
        public PivotHeadersControl()
        {
            DefaultStyleKey = typeof(PivotHeadersControl);
            _isDesign = DesignerProperties.GetIsInDesignMode(this);
        }

        /// <summary>
        /// Raised when the user, or the pivot, selects a different header.
        /// </summary>
        internal event EventHandler<SelectedIndexChangedEventArgs> SelectedIndexChanged;

        /// <summary>
        /// Gets or sets the index of the header that sits at the left edge.
        /// </summary>
        public int VisualFirstIndex
        {
            get { return (int)GetValue(VisualFirstIndexProperty); }
            set { SetValue(VisualFirstIndexProperty, value); }
        }

        /// <summary>
        /// Gets or sets the style applied to the generated header items.
        /// </summary>
        public new Style ItemContainerStyle
        {
            get { return (Style)GetValue(ItemContainerStyleProperty); }
            set { SetValue(ItemContainerStyleProperty, value); }
        }

        /// <summary>
        /// Gets or sets the selected index.
        /// </summary>
        internal int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        /// <summary>
        /// Gets or sets the direction the pivot is moving in. It only breaks the
        /// tie when the run has two headers, where both scroll directions are
        /// one step.
        /// </summary>
        internal AnimationDirection AnimationDirection { get; set; }

        /// <summary>
        /// Gets a value indicating whether a click on a header should be ignored
        /// because the pivot handled the press as a drag.
        /// </summary>
        internal bool CancelClick { get; private set; }

        /// <inheritdoc/>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _headerItemsPresenter = GetTemplateChild(HeaderItemsPresenterName) as ItemsPresenter;
            _canvas = GetTemplateChild(CanvasName) as Canvas;

            if (_canvas != null)
            {
                EnsureCanvasTransform();
            }

            Pivot pivot = FindPivot();
            if (pivot != null)
            {
                pivot.RegisterHeadersControl(this);
            }

            if (Items.Count > 0)
            {
                VisualFirstIndex = SelectedIndex;
            }

            InvalidateItemsLayout();
        }

        /// <summary>
        /// Cancels the next header click. Called when the pivot turns the press
        /// into a drag.
        /// </summary>
        internal void CancelPendingClick()
        {
            CancelClick = true;
        }

        /// <summary>
        /// Clears the click bookkeeping when a new press starts.
        /// </summary>
        internal void ResetClickState()
        {
            CancelClick = false;
        }

        /// <summary>
        /// Applies the locked appearance: the headers stop responding and fade
        /// out, or fade back in when the pivot is unlocked.
        /// </summary>
        /// <param name="isLocked">Whether the pivot is locked.</param>
        internal void SetLocked(bool isLocked)
        {
            _isLocked = isLocked;

            for (int i = 0; i < Items.Count; i++)
            {
                PivotHeaderItem item = GetHeaderContainer(i);
                if (item != null)
                {
                    ApplyLockedState(item);
                }
            }
        }

        /// <summary>
        /// Moves the strip back to the resting offset.
        /// </summary>
        /// <param name="duration">The duration of the restore animation.</param>
        internal void RestoreHeaderPosition(Duration duration)
        {
            if (_canvas == null)
            {
                return;
            }

            TranslateTransform transform = EnsureCanvasTransform();
            double current = transform.X;
            if (current == 0)
            {
                return;
            }

            transform.BeginAnimation(TranslateTransform.XProperty, null);
            transform.X = 0;

            DoubleAnimation animation = new DoubleAnimation(current, 0.0, duration)
            {
                EasingFunction = new QuarticEase()
            };
            transform.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        /// <summary>
        /// Follows the pointer while the pivot is being dragged.
        /// </summary>
        /// <param name="cumulative">The horizontal displacement of the pointer.</param>
        /// <param name="contentWidth">The width of the pivot viewport.</param>
        /// <param name="duration">The time budget for the movement.</param>
        internal void PanHeader(double cumulative, double contentWidth, Duration duration)
        {
            if (_isAnimating || _canvas == null)
            {
                return;
            }

            double limit = contentWidth > 0 ? contentWidth : GetTotalItemsWidth();
            if (limit <= 0)
            {
                limit = double.MaxValue;
            }

            TranslateTransform transform = EnsureCanvasTransform();
            transform.BeginAnimation(TranslateTransform.XProperty, null);

            // The offset is applied directly so the strip tracks the pointer;
            // the duration is only the caller's timing hint.
            transform.X = Math.Max(-limit, Math.Min(limit, cumulative));
        }

        /// <summary>
        /// Selects the header the user clicked.
        /// </summary>
        /// <param name="item">The header that was clicked.</param>
        internal void OnHeaderItemClicked(PivotHeaderItem item)
        {
            if (_isAnimating)
            {
                return;
            }

            if (CancelClick)
            {
                CancelClick = false;
                return;
            }

            item.IsSelected = true;
        }

        /// <summary>
        /// Confirms a header selection and notifies the pivot.
        /// </summary>
        /// <param name="item">The header that changed.</param>
        /// <param name="isSelected">Whether the header became the selected one.</param>
        internal void NotifyHeaderItemSelected(PivotHeaderItem item, bool isSelected)
        {
            if (!isSelected)
            {
                return;
            }

            int index = ItemContainerGenerator.IndexFromContainer(item);
            if (index < 0)
            {
                index = Items.IndexOf(item);
            }

            int selectedIndex = SelectedIndex;
            SelectOne(selectedIndex, index);
            SelectedIndex = index;
        }

        /// <inheritdoc/>
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            if (Items.Count > 0)
            {
                InvalidateItemsLayout();
            }
            else
            {
                VisualFirstIndex = 0;
                SelectedIndex = 0;
            }
        }

        /// <inheritdoc/>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            ApplyItemContainerStyle(element);
            base.PrepareContainerForItemOverride(element, item);

            PivotHeaderItem header = (PivotHeaderItem)element;
            header.ParentHeadersControl = this;
            header.Item = item;

            int index = ItemContainerGenerator.IndexFromContainer(element);
            if (index < 0)
            {
                index = Items.IndexOf(item);
            }

            if (index >= 0)
            {
                header.IsSelected = SelectedIndex == index;
            }

            AfterContainerPrepared(header);
        }

        /// <inheritdoc/>
        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            PivotHeaderItem header = element as PivotHeaderItem;
            if (header != null)
            {
                header.ParentHeadersControl = null;
                header.Item = null;
                _sizes.Remove(header);
            }

            base.ClearContainerForItemOverride(element, item);
        }

        /// <inheritdoc/>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is PivotHeaderItem;
        }

        /// <inheritdoc/>
        protected override DependencyObject GetContainerForItemOverride()
        {
            PivotHeaderItem header = new PivotHeaderItem();
            ApplyItemContainerStyle(header);
            return header;
        }

        /// <summary>
        /// Applies the <see cref="ItemContainerStyle"/> to a generated header.
        /// </summary>
        /// <param name="container">The header container.</param>
        protected virtual void ApplyItemContainerStyle(DependencyObject container)
        {
            PivotHeaderItem header = container as PivotHeaderItem;
            if (header == null || header.ReadLocalValue(StyleProperty) != DependencyProperty.UnsetValue)
            {
                return;
            }

            Style style = ItemContainerStyle;
            if (style != null)
            {
                header.Style = style;
            }
            else
            {
                header.ClearValue(StyleProperty);
            }
        }

        private static void OnSelectedIndexPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PivotHeadersControl control = (PivotHeadersControl)d;
            if (!control._activeSelectionChange)
            {
                control.SelectOne((int)e.OldValue, (int)e.NewValue);
            }
        }

        private static void OnVisualFirstIndexPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PivotHeadersControl control = (PivotHeadersControl)d;
            if (control._ignorePropertyChange)
            {
                control._ignorePropertyChange = false;
                return;
            }

            int newValue = (int)e.NewValue;
            if (control.Items.Count > 0 && (newValue < 0 || newValue >= control.Items.Count))
            {
                control._ignorePropertyChange = true;
                d.SetValue(e.Property, 0);
            }

            control.UpdateItemsLayout();
        }

        private void AfterContainerPrepared(PivotHeaderItem header)
        {
            header.SizeChanged -= OnHeaderSizeChanged;
            header.SizeChanged += OnHeaderSizeChanged;

            if (header.ActualWidth > 0)
            {
                _sizes[header] = header.ActualWidth;
            }

            if (_isLocked)
            {
                ApplyLockedState(header);
            }

            InvalidateItemsLayout();
        }

        private void ApplyLockedState(PivotHeaderItem header)
        {
            header.IsHitTestVisible = !_isLocked;

            DoubleAnimation animation = new DoubleAnimation(_isLocked ? 0.0 : 1.0, LockFadeDuration)
            {
                EasingFunction = new QuarticEase()
            };
            header.BeginAnimation(OpacityProperty, animation);
        }

        private void OnHeaderSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Control control = sender as Control;
            if (control != null)
            {
                _sizes[control] = e.NewSize.Width;
            }

            if (double.IsNaN(Height) || e.NewSize.Height > Height)
            {
                Height = e.NewSize.Height;
            }

            UpdateItemsLayout();
        }

        private void SelectOne(int previousIndex, int index)
        {
            if (_activeSelectionChange || index < 0 || index >= Items.Count)
            {
                return;
            }

            try
            {
                _activeSelectionChange = true;
                for (int i = 0; i < Items.Count; i++)
                {
                    PivotHeaderItem header = GetHeaderContainer(i);
                    if (header != null)
                    {
                        header.IsSelected = i == index;
                    }
                }
            }
            finally
            {
                _activeSelectionChange = false;
                SafeRaise.Raise(SelectedIndexChanged, this, new SelectedIndexChangedEventArgs(index));
                BeginAnimate(previousIndex, index);
            }
        }

        private void BeginAnimate(int previousIndex, int newIndex)
        {
            int count = Items.Count;

            if (_isDesign || _canvas == null || _headerItemsPresenter == null || !IsLoaded ||
                previousIndex == newIndex ||
                previousIndex < 0 || newIndex < 0 ||
                previousIndex >= count || newIndex >= count)
            {
                SnapTo(newIndex);
                return;
            }

            // The run is circular, so a move is a scroll left (the arriving header
            // already sits to the right of the current one) or a scroll right (the
            // arriving header is the circular predecessor). The pivot's animation
            // direction decides which, so the headers always travel the same way
            // as the content — including a click that jumps several items.
            int forwardSteps = (((newIndex - previousIndex) % count) + count) % count;
            bool forward = AnimationDirection != AnimationDirection.Right;
            int steps = forward ? forwardSteps : count - forwardSteps;

            if (steps <= 0)
            {
                SnapTo(newIndex);
                return;
            }

            double distance = 0.0;
            int index = forward ? previousIndex : newIndex;
            for (int i = 0; i < steps; i++)
            {
                distance += GetItemWidth(index);
                index = (index + 1) % count;
            }

            if (distance <= 0)
            {
                SnapTo(newIndex);
                return;
            }

            TranslateTransform transform = EnsureCanvasTransform();
            transform.BeginAnimation(TranslateTransform.XProperty, null);
            transform.X = 0;

            _animatingIndex = newIndex;

            if (!forward)
            {
                // Lay the run out with the arriving header at the left edge and
                // start one scroll to the right of it, so it slides in from the
                // left while the headers it passes stay where they were.
                SetVisualFirstIndex(newIndex);
            }

            _isAnimating = true;

            DoubleAnimation animation = forward
                ? new DoubleAnimation(0.0, -distance, HeaderAnimationDuration)
                : new DoubleAnimation(-distance, 0.0, HeaderAnimationDuration);
            animation.EasingFunction = new QuarticEase();
            animation.FillBehavior = FillBehavior.Stop;
            animation.Completed += OnScrollAnimationCompleted;

            transform.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private void SetVisualFirstIndex(int index)
        {
            if (VisualFirstIndex != index)
            {
                VisualFirstIndex = index;
            }
            else
            {
                UpdateItemsLayout();
            }
        }

        private void OnScrollAnimationCompleted(object sender, EventArgs e)
        {
            DoubleAnimation animation = sender as DoubleAnimation;
            if (animation != null)
            {
                animation.Completed -= OnScrollAnimationCompleted;
            }

            _isAnimating = false;
            SnapTo(_animatingIndex);
        }

        private void SnapTo(int index)
        {
            if (_canvas != null)
            {
                TranslateTransform transform = EnsureCanvasTransform();
                transform.BeginAnimation(TranslateTransform.XProperty, null);
                transform.X = 0;
            }

            if (index < 0 || index >= Items.Count)
            {
                index = 0;
            }

            SetVisualFirstIndex(index);
        }

        private void InvalidateItemsLayout()
        {
            if (_updatePending || _isDesign)
            {
                UpdateItemsLayout();
                return;
            }

            _updatePending = true;
            Dispatcher.BeginInvoke(
                DispatcherPriority.Loaded,
                new Action(
                    delegate
                    {
                        _updatePending = false;
                        UpdateItemsLayout();
                    }));
        }

        private void UpdateItemsLayout()
        {
            int count = Items.Count;
            if (count == 0)
            {
                return;
            }

            int first = VisualFirstIndex;
            if (first < 0 || first >= count)
            {
                first = 0;
            }

            // The run is circular, as on the phone: the selected header sits at
            // the left edge and the headers that follow it scroll in behind it,
            // wrapping around to the first item after the last one.
            double offset = 0;
            for (int i = 0; i < count; i++)
            {
                int index = (first + i) % count;
                SetItemPosition(index, offset);
                offset += GetItemWidth(index);
            }
        }

        private void SetItemPosition(int index, double offset)
        {
            PivotHeaderItem container = GetHeaderContainer(index);
            if (container != null)
            {
                Canvas.SetLeft(container, offset);
            }
        }

        private double GetItemWidth(int index)
        {
            Control control = GetHeaderContainer(index);
            if (control == null && index < Items.Count)
            {
                control = Items[index] as Control;
            }

            if (control == null)
            {
                return 0;
            }

            double width;
            if (_sizes.TryGetValue(control, out width) && width > 0)
            {
                return width;
            }

            width = control.ActualWidth;
            if (!double.IsNaN(width) && width > 0)
            {
                _sizes[control] = width;
                return width;
            }

            return control.DesiredSize.Width > 0 ? control.DesiredSize.Width : 0;
        }

        private double GetTotalItemsWidth()
        {
            double width = 0;
            for (int i = 0; i < Items.Count; i++)
            {
                width += GetItemWidth(i);
            }

            return width;
        }

        private PivotHeaderItem GetHeaderContainer(int index)
        {
            if (index < 0 || index >= Items.Count)
            {
                return null;
            }

            PivotHeaderItem container = ItemContainerGenerator.ContainerFromIndex(index) as PivotHeaderItem;
            if (container == null)
            {
                container = Items[index] as PivotHeaderItem;
            }

            return container;
        }

        private Pivot FindPivot()
        {
            DependencyObject parent = VisualTreeHelper.GetParent(this);
            while (parent != null)
            {
                Pivot pivot = parent as Pivot;
                if (pivot != null)
                {
                    return pivot;
                }

                parent = VisualTreeHelper.GetParent(parent);
            }

            return null;
        }

        private TranslateTransform EnsureCanvasTransform()
        {
            TranslateTransform transform = _canvas.RenderTransform as TranslateTransform;
            if (transform == null)
            {
                transform = new TranslateTransform();
                _canvas.RenderTransform = transform;
            }

            return transform;
        }
    }
}
