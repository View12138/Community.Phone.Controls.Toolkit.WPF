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
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Microsoft.Phone.Controls.Primitives;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// A control that hosts a set of <see cref="PivotItem"/> pages and moves
    /// between them with a horizontal slide.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The phone control is animated by the compositor: swiping moves the
    /// content with the finger, a flick or a header click starts a
    /// <see cref="PivotAnimationSeconds"/> second slide, and the headers scroll
    /// in lock step. WPF has no direct manipulation and no compositor, so the
    /// port drives the same motion with <see cref="DoubleAnimation"/>s:
    /// </para>
    /// <list type="bullet">
    /// <item><description>a pointer drag moves the content and the header strip (600 px/s, from the original control);</description></item>
    /// <item><description>releasing past half the viewport, flicking, clicking a header or pressing Left/Right navigates;</description></item>
    /// <item><description>the incoming item slides in from the side it travels from while the outgoing item slides out;</description></item>
    /// <item><description>the header run scrolls with a <see cref="QuarticEase"/> over 0.5 s.</description></item>
    /// </list>
    /// <para>
    /// The header run is circular, as on the phone: the selected header sits at
    /// the left edge and the headers after it follow in order, wrapping from the
    /// last item back to the first.
    /// </para>
    /// </remarks>
    [TemplatePart(Name = PivotItemPresenterElement, Type = typeof(ItemsPresenter))]
    [TemplatePart(Name = HeadersListElement, Type = typeof(PivotHeadersControl))]
    [TemplatePart(Name = TitleElement, Type = typeof(UIElement))]
    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(PivotItem))]
    public class Pivot : ItemsControl
    {
        private enum DragLock
        {
            Unset,
            Free,
            Vertical,
            Horizontal
        }

        /// <summary>
        /// The duration of one content slide, from the original control.
        /// </summary>
        internal const double PivotAnimationSeconds = 0.25;

        /// <summary>
        /// The speed the content follows the pointer at, from the original control.
        /// </summary>
        internal const double PixelsPerSecond = 600.0;

        internal const string PivotItemPresenterElement = "PivotItemPresenter";
        internal const string HeadersListElement = "HeadersListElement";
        internal const string TitleElement = "TitleElement";

        internal static readonly TimeSpan PivotAnimationTimeSpan = TimeSpan.FromSeconds(PivotAnimationSeconds);

        internal static readonly Duration PivotAnimationDuration = new Duration(PivotAnimationTimeSpan);

        internal static readonly Duration ZeroDuration = new Duration(TimeSpan.Zero);

        internal readonly IEasingFunction QuarticEase = new QuarticEase();

        /// <summary>
        /// Identifies the <see cref="HeaderTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register(
            "HeaderTemplate",
            typeof(DataTemplate),
            typeof(Pivot),
            new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="SelectedIndex"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(
            "SelectedIndex",
            typeof(int),
            typeof(Pivot),
            new PropertyMetadata(0, OnSelectedIndexPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="SelectedItem"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            "SelectedItem",
            typeof(object),
            typeof(Pivot),
            new PropertyMetadata(null, OnSelectedItemPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="Title"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title",
            typeof(object),
            typeof(Pivot),
            new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="TitleTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleTemplateProperty = DependencyProperty.Register(
            "TitleTemplate",
            typeof(DataTemplate),
            typeof(Pivot),
            new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="ItemContainerStyle"/> dependency property.
        /// </summary>
        public static readonly new DependencyProperty ItemContainerStyleProperty = DependencyProperty.Register(
            "ItemContainerStyle",
            typeof(Style),
            typeof(Pivot),
            null);

        /// <summary>
        /// Identifies the <see cref="IsLocked"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsLockedProperty = DependencyProperty.Register(
            "IsLocked",
            typeof(bool),
            typeof(Pivot),
            new PropertyMetadata(false, OnIsLockedPropertyChanged));

        /// <summary>
        /// Identifies the routed event carried by <see cref="SelectionChanged"/>.
        /// WPF's <see cref="SelectionChangedEventArgs"/> requires a routed event
        /// while the original platform's constructor did not.
        /// </summary>
        internal static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent(
            "SelectionChanged",
            RoutingStrategy.Bubble,
            typeof(SelectionChangedEventHandler),
            typeof(Pivot));

        private readonly Dictionary<object, PivotItem> _itemToContainer = new Dictionary<object, PivotItem>();
        private readonly Dictionary<PivotItem, object> _containerToItem = new Dictionary<PivotItem, object>();
        private readonly Queue<int> _queuedIndexChanges = new Queue<int>(5);

        private PivotHeadersControl _headers;
        private ItemsPresenter _itemsPresenter;
        private Panel _itemsPanel;
        private AnimationDirection? _animationHint;
        private bool _updatingHeaderItems;
        private bool _ignorePropertyChange;
        private bool _animating;
        private bool _isDesignTime;
        private bool _skippedLoadingPivotItem;
        private bool _skippedSwapVisibleContent;
        private bool _templateApplied;
        private PivotItem _exitingItem;
        private TransformAnimator _panAnimator;
        private Point? _dragOrigin;
        private double _lastDragOffset;
        private DragLock _dragLock;
        private bool _isHorizontalDragging;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pivot"/> class.
        /// </summary>
        public Pivot()
        {
            DefaultStyleKey = typeof(Pivot);
            Focusable = true;
            IsTabStop = false;

            _isDesignTime = DesignerProperties.GetIsInDesignMode(this);

            SizeChanged += OnSizeChanged;
            PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            PreviewMouseMove += OnPreviewMouseMove;
            MouseLeftButtonUp += OnMouseLeftButtonUp;
            LostMouseCapture += OnLostMouseCapture;
        }

        /// <summary>
        /// Raised before a pivot item becomes the active one.
        /// </summary>
        public event EventHandler<PivotItemEventArgs> LoadingPivotItem;

        /// <summary>
        /// Raised when a pivot item has become the active one.
        /// </summary>
        public event EventHandler<PivotItemEventArgs> LoadedPivotItem;

        /// <summary>
        /// Raised before a pivot item stops being the active one.
        /// </summary>
        public event EventHandler<PivotItemEventArgs> UnloadingPivotItem;

        /// <summary>
        /// Raised when a pivot item has stopped being the active one.
        /// </summary>
        public event EventHandler<PivotItemEventArgs> UnloadedPivotItem;

        /// <summary>
        /// Raised when the selected item changes.
        /// </summary>
        public event SelectionChangedEventHandler SelectionChanged;

        /// <summary>
        /// Gets or sets the template used to render the headers.
        /// </summary>
        public DataTemplate HeaderTemplate
        {
            get { return GetValue(HeaderTemplateProperty) as DataTemplate; }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        /// <summary>
        /// Gets or sets the index of the active pivot item.
        /// </summary>
        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set
            {
                if (!IsLocked || ReadLocalValue(SelectedIndexProperty) == DependencyProperty.UnsetValue)
                {
                    SetValue(SelectedIndexProperty, value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the active pivot item.
        /// </summary>
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        /// <summary>
        /// Gets or sets the title shown above the headers.
        /// </summary>
        public object Title
        {
            get { return GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        /// <summary>
        /// Gets or sets the template used to render <see cref="Title"/>.
        /// </summary>
        public DataTemplate TitleTemplate
        {
            get { return GetValue(TitleTemplateProperty) as DataTemplate; }
            set { SetValue(TitleTemplateProperty, value); }
        }

        /// <summary>
        /// Gets or sets the style applied to the generated pivot items.
        /// </summary>
        public new Style ItemContainerStyle
        {
            get { return GetValue(ItemContainerStyleProperty) as Style; }
            set { SetValue(ItemContainerStyleProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the pivot is locked. A locked
        /// pivot cannot be navigated, fades its headers out and refuses item
        /// changes.
        /// </summary>
        public bool IsLocked
        {
            get { return (bool)GetValue(IsLockedProperty); }
            set { SetValue(IsLockedProperty, value); }
        }

        /// <summary>
        /// Gets the title template part, once the template is applied.
        /// </summary>
        internal UIElement TitlePart
        {
            get { return GetTemplateChild(TitleElement) as UIElement; }
        }

        /// <summary>
        /// Gets a value indicating whether a slide is currently running.
        /// </summary>
        internal bool IsAnimating
        {
            get { return _animating; }
        }

        private bool EnoughItemsForManipulation
        {
            get { return Items.Count > 1; }
        }

        /// <inheritdoc/>
        public override void OnApplyTemplate()
        {
            if (_headers != null)
            {
                _headers.SelectedIndexChanged -= OnHeaderSelectionChanged;
                _headers = null;
            }

            base.OnApplyTemplate();

            _itemsPanel = null;

            _itemsPresenter = GetTemplateChild(PivotItemPresenterElement) as ItemsPresenter;
            if (_itemsPresenter != null && !(_itemsPresenter.RenderTransform is TranslateTransform))
            {
                _itemsPresenter.RenderTransform = new TranslateTransform();
            }

            RegisterHeadersControl(GetTemplateChild(HeadersListElement) as PivotHeadersControl);

            _templateApplied = true;

            if (Items.Count > 0)
            {
                if (SelectedIndex < 0)
                {
                    SelectedIndex = 0;
                }

                UpdateSelectionFromIndex();
            }

            UpdateVisibleContent(SelectedIndex);
        }

        /// <summary>
        /// Registers the header strip that belongs to this pivot. Called by
        /// <see cref="PivotHeadersControl"/> when its own template is applied, so
        /// the two controls connect regardless of template order.
        /// </summary>
        /// <param name="headers">The header strip, or <c>null</c> to detach.</param>
        internal void RegisterHeadersControl(PivotHeadersControl headers)
        {
            if (ReferenceEquals(_headers, headers))
            {
                SetSelectedHeaderIndex(SelectedIndex);
                return;
            }

            if (_headers != null)
            {
                _headers.SelectedIndexChanged -= OnHeaderSelectionChanged;
            }

            _headers = headers;

            if (_headers != null)
            {
                _headers.SelectedIndexChanged += OnHeaderSelectionChanged;
                _headers.VisualFirstIndex = Math.Max(SelectedIndex, 0);
                UpdateHeaders();
                SetSelectedHeaderIndex(SelectedIndex);
                _headers.SetLocked(IsLocked);
            }
        }

        /// <summary>
        /// Raises <see cref="LoadingPivotItem"/> for the incoming item.
        /// </summary>
        /// <param name="item">The incoming pivot item.</param>
        protected virtual void OnLoadingPivotItem(PivotItem item)
        {
            SafeRaise.Raise(LoadingPivotItem, this, () => new PivotItemEventArgs(item));
        }

        /// <summary>
        /// Raises <see cref="LoadedPivotItem"/> for the incoming item.
        /// </summary>
        /// <param name="item">The incoming pivot item.</param>
        protected virtual void OnLoadedPivotItem(PivotItem item)
        {
            SafeRaise.Raise(LoadedPivotItem, this, () => new PivotItemEventArgs(item));
        }

        /// <summary>
        /// Raises <see cref="UnloadingPivotItem"/> for the outgoing item.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnUnloadingPivotItem(PivotItemEventArgs e)
        {
            EventHandler<PivotItemEventArgs> handler = UnloadingPivotItem;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Raises <see cref="UnloadedPivotItem"/> for the outgoing item.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnUnloadedPivotItem(PivotItemEventArgs e)
        {
            EventHandler<PivotItemEventArgs> handler = UnloadedPivotItem;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <inheritdoc/>
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            int selectedIndex = SelectedIndex;
            int? newSelection = null;
            int count = Items.Count;

            if (_templateApplied && IsLocked)
            {
                throw new InvalidOperationException("Pivot Items cannot be modified when locked");
            }

            if (e != null)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        if (_templateApplied && e.NewStartingIndex == selectedIndex)
                        {
                            newSelection = selectedIndex;
                        }

                        break;

                    case NotifyCollectionChangedAction.Remove:
                        newSelection = selectedIndex;
                        if (selectedIndex == e.OldStartingIndex || selectedIndex >= count)
                        {
                            newSelection = count > 0 ? 0 : -1;
                        }

                        break;

                    case NotifyCollectionChangedAction.Reset:
                        newSelection = count > 0 ? Math.Min(Math.Max(selectedIndex, 0), count - 1) : -1;
                        break;
                }
            }

            if (newSelection.HasValue)
            {
                _animationHint = newSelection.Value < selectedIndex ? AnimationDirection.Right : AnimationDirection.Left;
                SetSelectedIndexInternal(newSelection.Value);
            }

            base.OnItemsChanged(e);

            UpdateHeaders();
            Dispatcher.BeginInvoke(
                DispatcherPriority.Loaded,
                new Action(
                    delegate
                    {
                        UpdateSelectionFromIndex();
                    }));
        }

        /// <inheritdoc/>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            ApplyItemContainerStyle(element);
            base.PrepareContainerForItemOverride(element, item);

            PivotItem container = element as PivotItem;
            if (container == null)
            {
                return;
            }

            _itemToContainer[item] = container;
            _containerToItem[container] = item;

            int selectedIndex = SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < Items.Count)
            {
                object selectedItem = Items[selectedIndex];
                if (ReferenceEquals(item, selectedItem))
                {
                    if (_skippedLoadingPivotItem)
                    {
                        OnLoadingPivotItem(container);
                        if (_skippedSwapVisibleContent)
                        {
                            OnLoadedPivotItem(container);
                        }
                    }

                    UpdateItemVisibility(container, true);
                    return;
                }
            }

            UpdateItemVisibility(container, false);
        }

        /// <inheritdoc/>
        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            base.ClearContainerForItemOverride(element, item);

            _itemToContainer.Remove(item);
            PivotItem container = element as PivotItem;
            if (container != null)
            {
                _containerToItem.Remove(container);
            }
        }

        /// <inheritdoc/>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is PivotItem;
        }

        /// <inheritdoc/>
        protected override DependencyObject GetContainerForItemOverride()
        {
            PivotItem item = new PivotItem();
            ApplyItemContainerStyle(item);
            return item;
        }

        /// <summary>
        /// Applies the <see cref="ItemContainerStyle"/> to a generated item.
        /// </summary>
        /// <param name="container">The item container.</param>
        protected virtual void ApplyItemContainerStyle(DependencyObject container)
        {
            PivotItem item = container as PivotItem;
            if (item == null || item.ReadLocalValue(StyleProperty) != DependencyProperty.UnsetValue)
            {
                return;
            }

            Style style = ItemContainerStyle;
            if (style != null)
            {
                item.Style = style;
            }
            else
            {
                item.ClearValue(StyleProperty);
            }
        }

        /// <summary>
        /// Gets the pivot item that represents the given container.
        /// </summary>
        /// <param name="container">The container to look up.</param>
        /// <returns>The item, or <c>null</c>.</returns>
        protected object GetItem(PivotItem container)
        {
            object item = null;
            if (container != null)
            {
                _containerToItem.TryGetValue(container, out item);
            }

            return item;
        }

        /// <summary>
        /// Gets the container that represents the given item.
        /// </summary>
        /// <param name="item">The item to look up.</param>
        /// <returns>The container, or <c>null</c>.</returns>
        protected PivotItem GetContainer(object item)
        {
            return GetContainerInternal(item);
        }

        /// <summary>
        /// Raises <see cref="SelectionChanged"/>.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            ClearFocusOnChild();

            SelectionChangedEventHandler handler = SelectionChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Shows or hides an item container.
        /// </summary>
        /// <param name="element">The container.</param>
        /// <param name="toVisible">Whether the container should be visible.</param>
        protected virtual void UpdateItemVisibility(UIElement element, bool toVisible)
        {
            if (element == null)
            {
                return;
            }

            element.IsHitTestVisible = toVisible;

            if (toVisible)
            {
                element.Visibility = Visibility.Visible;
                Panel.SetZIndex(element, 1);
                return;
            }

            Panel.SetZIndex(element, 0);

            if (!_animating || !ReferenceEquals(element, _exitingItem))
            {
                element.Visibility = Visibility.Collapsed;
            }
        }

        /// <inheritdoc/>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Handled || IsLocked || !EnoughItemsForManipulation)
            {
                return;
            }

            switch (e.Key)
            {
                case Key.Left:
                case Key.PageUp:
                    NavigateByIndexChange(-1);
                    e.Handled = true;
                    break;

                case Key.Right:
                case Key.PageDown:
                    NavigateByIndexChange(1);
                    e.Handled = true;
                    break;

                case Key.Home:
                    SelectedIndex = 0;
                    e.Handled = true;
                    break;

                case Key.End:
                    SelectedIndex = Items.Count - 1;
                    e.Handled = true;
                    break;
            }
        }

        internal PivotItem GetContainerInternal(object item)
        {
            PivotItem container = null;
            if (item != null)
            {
                _itemToContainer.TryGetValue(item, out container);
            }

            return container;
        }

        private static void OnSelectedIndexPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Pivot pivot = (Pivot)d;
            if (pivot._ignorePropertyChange)
            {
                pivot._ignorePropertyChange = false;
                return;
            }

            pivot.UpdateSelectedIndex((int)e.OldValue, (int)e.NewValue);
        }

        private static void OnSelectedItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Pivot pivot = (Pivot)d;
            if (pivot._ignorePropertyChange)
            {
                pivot._ignorePropertyChange = false;
                return;
            }

            pivot.UpdateSelectedItem(e.OldValue, e.NewValue);
        }

        private static void OnIsLockedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Pivot pivot = (Pivot)d;
            pivot.ResetDrag();

            if (pivot._headers != null)
            {
                pivot._headers.SetLocked((bool)e.NewValue);
            }
        }

        private static double AngleFromVector(double x, double y)
        {
            double angle = Math.Atan2(y, x);
            if (angle < 0.0)
            {
                angle = (Math.PI * 2.0) + angle;
            }

            return angle * 360.0 / (Math.PI * 2.0);
        }

        private void UpdateHeaders()
        {
            if (_headers == null)
            {
                return;
            }

            List<PivotHeaderItem> headers = new List<PivotHeaderItem>();
            int count = Items.Count;
            for (int i = 0; i < count; i++)
            {
                PivotHeaderItem header = CreateHeaderBindingControl(Items[i]);
                if (header != null)
                {
                    headers.Add(header);
                }
            }

            try
            {
                _updatingHeaderItems = true;
                _headers.ItemsSource = count == 0 ? null : headers;
            }
            finally
            {
                _updatingHeaderItems = false;
            }
        }

        private PivotHeaderItem CreateHeaderBindingControl(object item)
        {
            PivotHeaderItem header = new PivotHeaderItem();
            header.ContentTemplate = HeaderTemplate;
            header.Item = item;

            Binding binding = new Binding();
            binding.Source = item;
            if (item is PivotItem)
            {
                binding.Path = new PropertyPath("Header");
            }

            try
            {
                binding.Mode = BindingMode.OneWay;
                header.SetBinding(ContentControl.ContentProperty, binding);
                return header;
            }
            catch
            {
                if (_isDesignTime)
                {
                    return null;
                }

                throw;
            }
        }

        private void SetSelectedHeaderIndex(int selectedIndex)
        {
            try
            {
                _updatingHeaderItems = true;
                if (_headers != null && Items.Count > 0)
                {
                    _headers.SelectedIndex = selectedIndex;
                }
            }
            finally
            {
                _updatingHeaderItems = false;
            }
        }

        private void UpdateSelectedIndex(int oldIndex, int newIndex)
        {
            object selectedItem = null;
            int count = Items.Count;

            if (newIndex >= 0 && newIndex < count)
            {
                selectedItem = Items[newIndex];
            }
            else if (count > 0 && !_isDesignTime)
            {
                _ignorePropertyChange = true;
                SelectedIndex = oldIndex;
                throw new ArgumentException("SelectedIndex");
            }

            if (newIndex < 0 && Items.Count > 0 && !_isDesignTime)
            {
                _ignorePropertyChange = true;
                SelectedIndex = 0;
                throw new ArgumentException("SelectedIndex");
            }

            SelectedItem = selectedItem;
        }

        private void SetSelectedIndexInternal(int newIndex)
        {
            _ignorePropertyChange = true;
            try
            {
                SelectedIndex = newIndex - 1;
                SelectedIndex = newIndex;
            }
            finally
            {
                _ignorePropertyChange = false;
            }
        }

        private void UpdateSelectedItem(object oldValue, object newValue)
        {
            if (newValue == null && Items.Count > 0 && !_isDesignTime)
            {
                _ignorePropertyChange = true;
                SelectedItem = oldValue;
                throw new ArgumentException("SelectedItem");
            }

            int oldIndex = Items.IndexOf(oldValue);
            int newIndex = Items.IndexOf(newValue);

            // The hint is set by a navigation that knows its own direction (a
            // flick, a header click, the keyboard). Setting SelectedIndex
            // directly, or through a binding, has to infer it from the index
            // movement every time: the phone control kept the hint, which made
            // every selection after the first slide in the previous direction.
            AnimationDirection direction;
            if (_animationHint.HasValue)
            {
                direction = _animationHint.Value;
            }
            else if (oldIndex < 0)
            {
                direction = AnimationDirection.Center;
            }
            else
            {
                direction = newIndex > oldIndex ? AnimationDirection.Left : AnimationDirection.Right;
            }

            _animationHint = null;

            PivotItem newContainer = GetContainer(newValue);
            PivotItem oldContainer = GetContainer(oldValue);

            if (oldValue != null)
            {
                OnUnloadingPivotItem(new PivotItemEventArgs(oldContainer));
            }

            if (newContainer != null)
            {
                OnLoadingPivotItem(newContainer);
            }
            else
            {
                _skippedLoadingPivotItem = true;
            }

            OnSelectionChanged(new SelectionChangedEventArgs(
                SelectionChangedEvent,
                new List<object> { oldValue },
                new List<object> { newValue }));

            BeginAnimateContent(newIndex, oldContainer, direction);

            SetSelectedHeaderIndex(newIndex);

            if (SelectedIndex != newIndex)
            {
                SetSelectedIndexInternal(newIndex);
            }
        }

        private void UpdateSelectionFromIndex()
        {
            int count = Items.Count;
            int index = SelectedIndex;

            if (count == 0)
            {
                index = -1;
            }
            else if (index < 0)
            {
                index = 0;
            }
            else if (index >= count)
            {
                index = count - 1;
            }

            if (index != SelectedIndex)
            {
                _ignorePropertyChange = true;
                try
                {
                    SelectedIndex = index;
                }
                finally
                {
                    _ignorePropertyChange = false;
                }
            }

            object item = index >= 0 ? Items[index] : null;

            _ignorePropertyChange = true;
            try
            {
                SelectedItem = item;
            }
            finally
            {
                _ignorePropertyChange = false;
            }

            UpdateVisibleContent(index);
            SetSelectedHeaderIndex(index);
        }

        private void BeginAnimateContent(int newIndex, PivotItem oldItem, AnimationDirection direction)
        {
            PivotItem newItem = GetContainer(SelectedItem);

            if (newItem != null)
            {
                newItem.MoveTo(AnimationDirection.Center);
            }

            if (_headers != null)
            {
                _headers.AnimationDirection = direction;
            }

            if (_itemsPresenter == null || _isDesignTime || !IsLoaded || direction == AnimationDirection.Center || newItem == null)
            {
                _skippedSwapVisibleContent = newItem == null || _itemsPresenter == null;
                SwapVisibleContent(oldItem, newIndex);
                ProcessQueuedChanges();
                return;
            }

            SwapVisibleContent(oldItem, newIndex);

            double destination = CalculateContentDestination(direction);
            double source = -destination;

            if (source == 0)
            {
                ProcessQueuedChanges();
                return;
            }

            newItem.MoveTo(direction);

            TranslateTransform incoming = TransformAnimator.GetTranslateTransform(newItem);
            TranslateTransform outgoing = null;
            if (oldItem != null && !ReferenceEquals(oldItem, newItem))
            {
                outgoing = TransformAnimator.GetTranslateTransform(oldItem);
            }

            _animating = true;
            _exitingItem = outgoing != null ? oldItem : null;
            _skippedSwapVisibleContent = false;

            incoming.BeginAnimation(TranslateTransform.XProperty, null);
            incoming.X = 0;

            if (outgoing != null)
            {
                outgoing.BeginAnimation(TranslateTransform.XProperty, null);
                outgoing.X = 0;
            }

            DoubleAnimation incomingAnimation = new DoubleAnimation(source, 0.0, PivotAnimationDuration)
            {
                EasingFunction = QuarticEase,
                FillBehavior = FillBehavior.Stop
            };
            incomingAnimation.Completed += delegate { CompleteContentAnimation(newItem, oldItem, incoming, outgoing); };
            incoming.BeginAnimation(TranslateTransform.XProperty, incomingAnimation);

            if (outgoing != null)
            {
                DoubleAnimation outgoingAnimation = new DoubleAnimation(0.0, destination, PivotAnimationDuration)
                {
                    EasingFunction = QuarticEase,
                    FillBehavior = FillBehavior.Stop
                };
                outgoing.BeginAnimation(TranslateTransform.XProperty, outgoingAnimation);
            }
        }

        private void CompleteContentAnimation(
            PivotItem newItem,
            PivotItem oldItem,
            TranslateTransform incoming,
            TranslateTransform outgoing)
        {
            incoming.BeginAnimation(TranslateTransform.XProperty, null);
            incoming.X = 0;

            if (outgoing != null)
            {
                outgoing.BeginAnimation(TranslateTransform.XProperty, null);
                outgoing.X = 0;
            }

            _exitingItem = null;
            _animating = false;

            if (oldItem != null && !ReferenceEquals(oldItem, newItem))
            {
                UpdateItemVisibility(oldItem, false);
            }

            ProcessQueuedChanges();
        }

        private void SwapVisibleContent(PivotItem oldItem, int newIndex)
        {
            if (oldItem != null)
            {
                OnUnloadedPivotItem(new PivotItemEventArgs(oldItem));
            }

            UpdateVisibleContent(newIndex);

            PivotItem container = GetContainer(SelectedItem);
            _skippedSwapVisibleContent = container == null;

            if (container != null)
            {
                OnLoadedPivotItem(container);
            }
        }

        private void ProcessQueuedChanges()
        {
            if (_queuedIndexChanges.Count > 0 && !_animating)
            {
                NavigateByIndexChange(_queuedIndexChanges.Dequeue());
            }
        }

        private double CalculateContentDestination(AnimationDirection direction)
        {
            double destination = 0.0;
            double width = ActualWidth;

            switch (direction)
            {
                case AnimationDirection.Left:
                    destination = 0.0 - width;
                    break;

                case AnimationDirection.Right:
                    destination = width;
                    break;
            }

            return destination;
        }

        private void UpdateVisibleContent(int index)
        {
            if (!TryHasItemsHost())
            {
                return;
            }

            for (int i = 0; i < _itemsPanel.Children.Count; i++)
            {
                UpdateItemVisibility(_itemsPanel.Children[i], i == index);
            }
        }

        private bool TryHasItemsHost()
        {
            if (_itemsPanel != null)
            {
                return true;
            }

            if (ItemContainerGenerator != null)
            {
                DependencyObject container = ItemContainerGenerator.ContainerFromIndex(0);
                if (container != null)
                {
                    _itemsPanel = VisualTreeHelper.GetParent(container) as Panel;
                    return _itemsPanel != null;
                }
            }

            return false;
        }

        private void NavigateByIndexChange(int indexDelta)
        {
            if (_animating)
            {
                _queuedIndexChanges.Enqueue(indexDelta);
                return;
            }

            int selectedIndex = SelectedIndex;
            if (selectedIndex == -1 || Items.Count == 0)
            {
                return;
            }

            _animationHint = indexDelta > 0 ? AnimationDirection.Left : AnimationDirection.Right;

            selectedIndex += indexDelta;
            if (selectedIndex >= Items.Count)
            {
                selectedIndex = 0;
            }
            else if (selectedIndex < 0)
            {
                selectedIndex = Items.Count - 1;
            }

            if (_headers != null)
            {
                _headers.CancelPendingClick();
            }

            SelectedIndex = selectedIndex;
        }

        private void OnHeaderSelectionChanged(object sender, SelectedIndexChangedEventArgs e)
        {
            if (_updatingHeaderItems)
            {
                return;
            }

            _animationHint = e.SelectedIndex < SelectedIndex ? AnimationDirection.Right : AnimationDirection.Left;
            SelectedIndex = e.SelectedIndex;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Clip = new RectangleGeometry(new Rect(0.0, 0.0, ActualWidth, ActualHeight));

            if (_isDesignTime)
            {
                UpdateVisibleContent(SelectedIndex);
            }
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ResetDrag();

            if (_headers != null)
            {
                _headers.ResetClickState();
            }

            if (!EnoughItemsForManipulation || IsLocked)
            {
                return;
            }

            _dragOrigin = e.GetPosition(this);
        }

        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!_dragOrigin.HasValue || e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            Point position = e.GetPosition(this);
            double deltaX = position.X - _dragOrigin.Value.X;
            double deltaY = position.Y - _dragOrigin.Value.Y;

            if (_dragLock == DragLock.Unset)
            {
                if (Math.Abs(deltaX) < SystemParameters.MinimumHorizontalDragDistance &&
                    Math.Abs(deltaY) < SystemParameters.MinimumVerticalDragDistance)
                {
                    return;
                }

                double angle = AngleFromVector(deltaX, deltaY) % 180.0;
                if (angle <= 45.0 || angle >= 135.0)
                {
                    _dragLock = DragLock.Horizontal;
                }
                else if (angle > 45.0 && angle < 135.0)
                {
                    _dragLock = DragLock.Vertical;
                }
                else
                {
                    _dragLock = DragLock.Free;
                }

                if (_dragLock == DragLock.Horizontal)
                {
                    _isHorizontalDragging = true;

                    if (_headers != null)
                    {
                        _headers.CancelPendingClick();
                    }

                    CaptureMouse();
                }
            }

            if (_dragLock != DragLock.Horizontal)
            {
                return;
            }

            e.Handled = true;

            HorizontalDrag(deltaX, deltaX - _lastDragOffset);
            _lastDragOffset = deltaX;
        }

        private void HorizontalDrag(double cumulative, double delta)
        {
            if (_animating || !EnoughItemsForManipulation || _itemsPresenter == null || _headers == null)
            {
                return;
            }

            TransformAnimator.EnsureAnimator(_itemsPresenter, ref _panAnimator);
            if (_panAnimator == null)
            {
                return;
            }

            Duration duration = new Duration(TimeSpan.FromSeconds(Math.Abs(delta) / PixelsPerSecond));
            _panAnimator.GoTo(cumulative, duration);
            _headers.PanHeader(cumulative, ActualWidth, duration);
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isHorizontalDragging)
            {
                ResetDrag();
                return;
            }

            double cumulative = _panAnimator != null ? _panAnimator.CurrentOffset : 0.0;

            _isHorizontalDragging = false;
            ResetDrag();

            if (IsMouseCaptured)
            {
                ReleaseMouseCapture();
            }

            e.Handled = true;

            double threshold = Math.Max(ActualWidth, 1.0) / 2.0;
            bool navigated = !IsLocked && EnoughItemsForManipulation && Math.Abs(cumulative) >= threshold;
            if (navigated)
            {
                NavigateByIndexChange(cumulative < 0 ? 1 : -1);
            }

            // A navigation already started the header scroll from the dragged
            // offset; restoring would cancel it.
            GesturesComplete(!navigated);
        }

        private void OnLostMouseCapture(object sender, MouseEventArgs e)
        {
            if (!_isHorizontalDragging)
            {
                return;
            }

            _isHorizontalDragging = false;
            ResetDrag();
            GesturesComplete(true);
        }

        private void GesturesComplete(bool restoreHeaders)
        {
            if (_panAnimator != null)
            {
                _panAnimator.GoTo(0, PivotAnimationDuration, QuarticEase);
            }

            if (restoreHeaders && _headers != null)
            {
                _headers.RestoreHeaderPosition(PivotAnimationDuration);
            }
        }

        private void ResetDrag()
        {
            _dragOrigin = null;
            _dragLock = DragLock.Unset;
            _lastDragOffset = 0;
        }

        private void ClearFocusOnChild()
        {
            DependencyObject focused = Keyboard.FocusedElement as DependencyObject;
            if (focused == null)
            {
                return;
            }

            PivotItem container = FindAncestorPivotItem(focused);
            if (container != null && !ReferenceEquals(container, GetContainer(SelectedItem)))
            {
                Keyboard.ClearFocus();
            }
        }

        private PivotItem FindAncestorPivotItem(DependencyObject element)
        {
            while (element != null)
            {
                PivotItem item = element as PivotItem;
                if (item != null)
                {
                    return item;
                }

                element = GetParent(element);
            }

            return null;
        }

        private static DependencyObject GetParent(DependencyObject element)
        {
            try
            {
                return VisualTreeHelper.GetParent(element);
            }
            catch (InvalidOperationException)
            {
                return LogicalTreeHelper.GetParent(element);
            }
        }
    }
}
