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
using System.Windows.Media.Imaging;
using Microsoft.Phone.Controls.Primitives;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Presents a set of <see cref="PanoramaItem"/> pages that can be panned
    /// horizontally with parallax between the title, items and background.
    /// </summary>
    /// <remarks>
    /// The phone control uses native direct manipulation for the gesture. This
    /// port drives the same snap positions, durations and parallax rates with
    /// WPF input events and storyboards.
    /// </remarks>
    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(PanoramaItem))]
    [TemplatePart(Name = BackgroundLayerElement, Type = typeof(PanningLayer))]
    [TemplatePart(Name = ItemsLayerElement, Type = typeof(PanningLayer))]
    [TemplatePart(Name = TitleLayerElement, Type = typeof(PanningLayer))]
    public class Panorama : ItemsControl
    {
        private enum DragLock
        {
            Unset,
            Free,
            Vertical,
            Horizontal
        }

        private struct DragSample
        {
            public Point Point;
            public DateTime Time;
        }

        internal const int Spacing = 48;
        internal static readonly Duration Immediately = new Duration(TimeSpan.Zero);

        private const string BackgroundLayerElement = "BackgroundLayer";
        private const string TitleLayerElement = "TitleLayer";
        private const string ItemsLayerElement = "ItemsLayer";

        private static readonly Duration DefaultDuration = new Duration(TimeSpan.FromMilliseconds(800.0));
        private static readonly Duration FlickDuration = DefaultDuration;
        private static readonly Duration SnapDuration = DefaultDuration;
        private static readonly Duration PanDuration = new Duration(TimeSpan.FromMilliseconds(150.0));

        /// <summary>
        /// Identifies the <see cref="Title"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(object), typeof(Panorama), null);

        /// <summary>
        /// Identifies the <see cref="TitleTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleTemplateProperty = DependencyProperty.Register(
            "TitleTemplate", typeof(DataTemplate), typeof(Panorama), null);

        /// <summary>
        /// Identifies the <see cref="HeaderTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register(
            "HeaderTemplate", typeof(DataTemplate), typeof(Panorama), null);

        /// <summary>
        /// Identifies the <see cref="SelectedItem"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            "SelectedItem", typeof(object), typeof(Panorama), new PropertyMetadata(null, OnSelectionChanged));

        /// <summary>
        /// Identifies the <see cref="SelectedIndex"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(
            "SelectedIndex", typeof(int), typeof(Panorama), new PropertyMetadata(-1));

        /// <summary>
        /// Identifies the <see cref="DefaultItem"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DefaultItemProperty = DependencyProperty.Register(
            "DefaultItem", typeof(object), typeof(Panorama), new PropertyMetadata(null, OnDefaultItemChanged));

        /// <summary>
        /// Identifies the <see cref="ItemContainerStyle"/> dependency property.
        /// </summary>
        public static readonly new DependencyProperty ItemContainerStyleProperty = DependencyProperty.Register(
            "ItemContainerStyle", typeof(Style), typeof(Panorama), null);

        private static readonly DependencyProperty BackgroundShadowProperty = DependencyProperty.Register(
            "BackgroundShadow", typeof(Brush), typeof(Panorama), new PropertyMetadata(null, OnBackgroundShadowChanged));

        /// <summary>
        /// Identifies the routed event carried by <see cref="SelectionChanged"/>.
        /// WPF's <see cref="SelectionChangedEventArgs"/> requires a routed event.
        /// </summary>
        internal static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent(
            "SelectionChanged", RoutingStrategy.Bubble, typeof(EventHandler<SelectionChangedEventArgs>), typeof(Panorama));

        private readonly Dictionary<object, PanoramaItem> _itemToContainer = new Dictionary<object, PanoramaItem>();
        private readonly Dictionary<PanoramaItem, object> _containerToItem = new Dictionary<PanoramaItem, object>();
        private readonly List<DragSample> _velocitySamples = new List<DragSample>();

        private PanningLayer _panningBackground;
        private PanningLayer _panningTitle;
        private PanningLayer _panningItems;

        private int _itemsWidth;
        private int _viewportWidth;
        private int _viewportHeight;
        private int _targetOffset;
        private int _initialTargetOffset;
        private int _cumulativeDragDelta;
        private int _flickDirection;
        private int _lastDragOffsetInt;

        private bool _dragging;
        private bool _dragged;
        private bool _manipulationStarted;
        private bool _updateBackgroundPending = true;
        private bool _entranceAnimationPlayed;
        private bool _adjustSelectedRequested;
        private bool _suppressSelectionChangedEvent;
        private bool _isDesignTime;

        private DragLock _dragLock;
        private Point? _dragOrigin;
        private BitmapImage _backgroundImage;

        internal PanoramaPanel Panel { get; set; }

        internal int ActualOffset
        {
            get { return _panningItems != null ? (int)_panningItems.ActualOffset : 0; }
        }

        internal int ItemsWidth
        {
            get { return _itemsWidth; }
            set { _itemsWidth = value; }
        }

        internal int ViewportWidth
        {
            get { return _viewportWidth; }
            private set { _viewportWidth = value; }
        }

        internal int ViewportHeight
        {
            get { return _viewportHeight; }
            private set { _viewportHeight = value; }
        }

        internal int AdjustedViewportWidth
        {
            get { return Math.Max(0, ViewportWidth - Spacing); }
        }

        internal bool ShouldAllowWraparound
        {
            get { return Panel != null && Panel.VisibleChildren.Count > 1; }
        }

        /// <summary>
        /// Gets or sets the title displayed above the items.
        /// </summary>
        public object Title
        {
            get { return GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        /// <summary>
        /// Gets or sets the template used to display <see cref="Title"/>.
        /// </summary>
        public DataTemplate TitleTemplate
        {
            get { return (DataTemplate)GetValue(TitleTemplateProperty); }
            set { SetValue(TitleTemplateProperty, value); }
        }

        /// <summary>
        /// Gets or sets the default header template applied to item containers.
        /// </summary>
        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        /// <summary>
        /// Gets the currently selected item.
        /// </summary>
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            private set { SetValue(SelectedItemProperty, value); }
        }

        /// <summary>
        /// Gets the index of the currently selected item.
        /// </summary>
        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            private set { SetValue(SelectedIndexProperty, value); }
        }

        /// <summary>
        /// Gets or sets the item placed first in the panorama.
        /// </summary>
        public object DefaultItem
        {
            get { return GetValue(DefaultItemProperty); }
            set { SetValue(DefaultItemProperty, value); }
        }

        /// <summary>
        /// Gets or sets the style applied to generated item containers.
        /// </summary>
        public new Style ItemContainerStyle
        {
            get { return GetValue(ItemContainerStyleProperty) as Style; }
            set { SetValue(ItemContainerStyleProperty, value); }
        }

        /// <summary>
        /// Occurs when <see cref="SelectedItem"/> changes.
        /// </summary>
        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="Panorama"/> class.
        /// </summary>
        public Panorama()
        {
            DefaultStyleKey = typeof(Panorama);
            _isDesignTime = DesignerProperties.GetIsInDesignMode(this);
            SizeChanged += OnSizeChanged;
            PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            PreviewMouseMove += OnPreviewMouseMove;
            MouseLeftButtonUp += OnMouseLeftButtonUp;
            LostMouseCapture += OnLostMouseCapture;
        }

        /// <inheritdoc/>
        public override void OnApplyTemplate()
        {
            if (_panningBackground != null)
            {
                _panningBackground.Owner = null;
            }
            if (_panningTitle != null)
            {
                _panningTitle.Owner = null;
            }
            if (_panningItems != null)
            {
                _panningItems.Owner = null;
            }

            base.OnApplyTemplate();

            _panningBackground = GetTemplateChild(BackgroundLayerElement) as PanningLayer;
            _panningTitle = GetTemplateChild(TitleLayerElement) as PanningLayer;
            _panningItems = GetTemplateChild(ItemsLayerElement) as PanningLayer;

            if (_panningBackground != null)
            {
                _panningBackground.Owner = this;
            }
            if (_panningTitle != null)
            {
                _panningTitle.Owner = this;
            }
            if (_panningItems != null)
            {
                _panningItems.Owner = this;
            }

            // The original applies an entrance PlaneProjection
            // (CenterOfRotationX = -0.1) to the page here. The port does not
            // play that entrance animation, so the flag only records that the
            // template has been applied once; the jump-to-selection logic uses
            // it to tell the first measure apart from later ones.
            if (!_isDesignTime)
            {
                _entranceAnimationPlayed = true;
            }

            Binding binding = new Binding("Background")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.Self)
            };
            SetBinding(BackgroundShadowProperty, binding);
        }

        /// <inheritdoc/>
        protected override Size MeasureOverride(Size availableSize)
        {
            double width = double.IsInfinity(availableSize.Width)
                ? (ActualWidth > 0.0 ? ActualWidth : 480.0)
                : availableSize.Width;
            double height = double.IsInfinity(availableSize.Height)
                ? (ActualHeight > 0.0 ? ActualHeight : 800.0)
                : availableSize.Height;

            ViewportWidth = (int)width;
            ViewportHeight = (int)height;

            base.MeasureOverride(new Size(double.PositiveInfinity, ViewportHeight));
            return new Size(width, height);
        }

        /// <inheritdoc/>
        protected override Size ArrangeOverride(Size finalSize)
        {
            base.ArrangeOverride(finalSize);
            return finalSize;
        }

        /// <inheritdoc/>
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            RequestAdjustSelection();
        }

        /// <summary>
        /// Applies <see cref="ItemContainerStyle"/> to a generated container.
        /// </summary>
        /// <param name="container">The container to style.</param>
        protected virtual void ApplyItemContainerStyle(DependencyObject container)
        {
            PanoramaItem panoramaItem = container as PanoramaItem;
            if (panoramaItem == null || panoramaItem.ReadLocalValue(StyleProperty) != DependencyProperty.UnsetValue)
            {
                return;
            }

            Style itemContainerStyle = ItemContainerStyle;
            if (itemContainerStyle != null)
            {
                panoramaItem.Style = itemContainerStyle;
            }
            else
            {
                panoramaItem.ClearValue(StyleProperty);
            }
        }

        /// <summary>
        /// Gets the data item represented by a container.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <returns>The data item, or <c>null</c>.</returns>
        protected object GetItem(PanoramaItem container)
        {
            object item = null;
            if (container != null)
            {
                _containerToItem.TryGetValue(container, out item);
            }
            return item;
        }

        /// <summary>
        /// Gets the container generated for a data item.
        /// </summary>
        /// <param name="item">The data item.</param>
        /// <returns>The container, or <c>null</c>.</returns>
        protected PanoramaItem GetContainer(object item)
        {
            PanoramaItem container = null;
            if (item != null)
            {
                _itemToContainer.TryGetValue(item, out container);
            }
            return container;
        }

        /// <inheritdoc/>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            ApplyItemContainerStyle(element);
            base.PrepareContainerForItemOverride(element, item);
            _itemToContainer[item] = (PanoramaItem)element;
            _containerToItem[(PanoramaItem)element] = item;

            PanoramaItem panoramaItem = element as PanoramaItem;
            if (panoramaItem == null)
            {
                return;
            }

            if (panoramaItem.Content == null && panoramaItem != item)
            {
                panoramaItem.Content = item;
            }
            if (panoramaItem.HeaderTemplate == null &&
                element.ReadLocalValue(PanoramaItem.HeaderTemplateProperty) == DependencyProperty.UnsetValue)
            {
                panoramaItem.HeaderTemplate = HeaderTemplate;
            }
            if (panoramaItem.Header == null && !(item is UIElement) &&
                panoramaItem.ReadLocalValue(PanoramaItem.HeaderProperty) == DependencyProperty.UnsetValue)
            {
                panoramaItem.Header = item;
            }
        }

        /// <inheritdoc/>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is PanoramaItem;
        }

        /// <inheritdoc/>
        protected override DependencyObject GetContainerForItemOverride()
        {
            PanoramaItem panoramaItem = new PanoramaItem();
            ApplyItemContainerStyle(panoramaItem);
            return panoramaItem;
        }

        /// <inheritdoc/>
        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            base.ClearContainerForItemOverride(element, item);
            _itemToContainer.Remove(item);
            _containerToItem.Remove((PanoramaItem)element);
        }

        /// <summary>
        /// Raises <see cref="SelectionChanged"/>.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            EventHandler<SelectionChangedEventArgs> handler = SelectionChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        internal PanoramaItem GetDefaultItemContainer()
        {
            return GetContainer(DefaultItem);
        }

        internal void RequestAdjustSelection()
        {
            if (_adjustSelectedRequested)
            {
                return;
            }
            LayoutUpdated += OnLayoutUpdatedAdjustSelection;
            _adjustSelectedRequested = true;
        }

        internal void GoTo(int offset)
        {
            GoTo(offset, Immediately);
        }

        private void OnLayoutUpdatedAdjustSelection(object sender, EventArgs e)
        {
            _adjustSelectedRequested = false;
            LayoutUpdated -= OnLayoutUpdatedAdjustSelection;
            AdjustSelection();
        }

        private void AdjustSelection()
        {
            if (_isDesignTime)
            {
                if (IsLoaded)
                {
                    _targetOffset = 0;
                    GoTo(_targetOffset, Immediately);
                }
                return;
            }

            object selectedItem = SelectedItem;
            object target = null;
            bool jumpToSelection = false;
            if (Panel != null && Panel.VisibleChildren.Count > 0)
            {
                if (selectedItem == null)
                {
                    target = GetItem(Panel.VisibleChildren[0]);
                }
                else
                {
                    PanoramaItem container = GetContainer(selectedItem);
                    jumpToSelection = _entranceAnimationPlayed;
                    target = container != null && Panel.VisibleChildren.Contains(container)
                        ? selectedItem
                        : GetItem(Panel.VisibleChildren[0]);
                }
            }
            else
            {
                _targetOffset = 0;
                GoTo(_targetOffset, Immediately);
            }

            SetSelectionInternal(target);

            if (jumpToSelection)
            {
                PanoramaItem container = GetContainer(target);
                if (container != null)
                {
                    _targetOffset = -container.StartPosition;
                    GoTo(_targetOffset, Immediately);
                }
            }
        }

        private void SetSelectionInternal(object selectedItem)
        {
            _suppressSelectionChangedEvent = true;
            SelectedItem = selectedItem;
            _suppressSelectionChangedEvent = false;
        }

        private void GoTo(int offset, Duration duration, int direction)
        {
            if (_panningBackground != null)
            {
                _panningBackground.GoTo(offset, duration);
            }
            if (_panningTitle != null)
            {
                _panningTitle.GoTo(offset, duration);
            }
            if (_panningItems != null)
            {
                _panningItems.GoTo(offset, duration);
            }
        }

        private void GoTo(int offset, Duration duration)
        {
            GoTo(offset, duration, 0);
        }

        private int CoerceTargetOffset(int targetOffset)
        {
            return !ShouldAllowWraparound && Panel != null ? Panel.CoerceTargetOffset(targetOffset) : targetOffset;
        }

        private void WrapAround(int direction)
        {
            if (_panningBackground != null)
            {
                _panningBackground.Wraparound(direction);
            }
            if (_panningTitle != null)
            {
                _panningTitle.Wraparound(direction);
            }
            if (_panningItems != null)
            {
                _panningItems.Wraparound(direction);
            }
        }

        private void ProcessFlick()
        {
            if (_flickDirection == 0 || Panel == null || ItemsWidth == 0)
            {
                return;
            }

            int offset = ActualOffset;
            Panel.GetStops(offset, ItemsWidth, out PanoramaPanel.ItemStop previous, out PanoramaPanel.ItemStop current, out PanoramaPanel.ItemStop next);
            if (previous == current && current == next && next == null)
            {
                return;
            }

            _targetOffset = _flickDirection < 0 ? -next.Position : -previous.Position;
            bool wrap = Math.Sign(_targetOffset - ActualOffset) != Math.Sign(_flickDirection);
            if (wrap && !ShouldAllowWraparound)
            {
                return;
            }
            if (wrap)
            {
                WrapAround(Math.Sign(_flickDirection));
            }

            SelectedItem = GetItem(_flickDirection < 0 ? next.Item : previous.Item);
            GoTo(_targetOffset, FlickDuration, wrap ? _flickDirection : 0);
        }

        private static double AngleFromVector(double x, double y)
        {
            double angle = Math.Atan2(y, x);
            if (angle < 0.0)
            {
                angle += Math.PI * 2.0;
            }
            return angle * 360.0 / (Math.PI * 2.0);
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ResetDrag();
            if (Items.Count <= 1)
            {
                return;
            }

            Point position = e.GetPosition(this);
            _dragOrigin = position;
            _manipulationStarted = true;
            _initialTargetOffset = _targetOffset = ActualOffset;
            _velocitySamples.Clear();
            _velocitySamples.Add(new DragSample { Point = position, Time = DateTime.UtcNow });
        }

        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!_manipulationStarted || !_dragOrigin.HasValue || e.LeftButton != MouseButtonState.Pressed)
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
                    _dragging = true;
                    CaptureMouse();
                }
                else if (angle > 45.0 && angle < 135.0)
                {
                    _dragLock = DragLock.Vertical;
                }
                else
                {
                    _dragLock = DragLock.Free;
                }
            }

            if (_dragLock != DragLock.Horizontal)
            {
                return;
            }

            e.Handled = true;
            int step = (int)deltaX - _lastDragOffsetInt;
            if (step == 0)
            {
                return;
            }

            _lastDragOffsetInt = (int)deltaX;
            _cumulativeDragDelta = (int)deltaX;
            _targetOffset += step;
            _velocitySamples.Add(new DragSample { Point = position, Time = DateTime.UtcNow });
            while (_velocitySamples.Count > 2 && (DateTime.UtcNow - _velocitySamples[0].Time).TotalMilliseconds > 100.0)
            {
                _velocitySamples.RemoveAt(0);
            }

            if (Math.Abs(_cumulativeDragDelta) <= ViewportWidth)
            {
                _dragged = true;
                GoTo(CoerceTargetOffset(_targetOffset), PanDuration);
            }
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_manipulationStarted)
            {
                return;
            }

            _dragLock = DragLock.Unset;
            if (!_dragging)
            {
                ResetDrag();
                return;
            }

            Point position = e.GetPosition(this);
            Vector velocity = EstimateVelocity();
            _manipulationStarted = false;
            _dragging = false;

            if (IsMouseCaptured)
            {
                ReleaseMouseCapture();
            }
            e.Handled = true;

            if (Math.Abs(velocity.X) >= Pivot.PixelsPerSecond && Math.Abs(velocity.X) >= Math.Abs(velocity.Y))
            {
                _flickDirection = Math.Sign(velocity.X);
                ProcessFlick();
                ResetDrag();
                return;
            }

            _cumulativeDragDelta = _dragOrigin.HasValue ? (int)(position.X - _dragOrigin.Value.X) : 0;
            _targetOffset = _initialTargetOffset + _cumulativeDragDelta;
            int direction = Math.Sign(_cumulativeDragDelta);
            ResetDrag();

            if (Panel == null)
            {
                return;
            }

            Panel.GetSnapOffset(
                _targetOffset,
                ViewportWidth,
                direction,
                out int snapTo,
                out PanoramaItem newSelection,
                out bool wraparound);

            if (wraparound)
            {
                if (!ShouldAllowWraparound)
                {
                    return;
                }
                WrapAround(direction);
            }

            object item = GetItem(newSelection);
            if (item != null)
            {
                SelectedItem = item;
            }
            GoTo(snapTo, SnapDuration, wraparound ? direction : 0);
        }

        private void OnLostMouseCapture(object sender, MouseEventArgs e)
        {
            if (!_dragging)
            {
                return;
            }
            _dragging = false;
            _manipulationStarted = false;
            ResetDrag();
        }

        private Vector EstimateVelocity()
        {
            if (_velocitySamples.Count < 2)
            {
                return new Vector(0.0, 0.0);
            }

            DragSample first = _velocitySamples[0];
            DragSample last = _velocitySamples[_velocitySamples.Count - 1];
            double seconds = (last.Time - first.Time).TotalSeconds;
            if (seconds <= 0.0)
            {
                return new Vector(0.0, 0.0);
            }

            return new Vector((last.Point.X - first.Point.X) / seconds, (last.Point.Y - first.Point.Y) / seconds);
        }

        private void ResetDrag()
        {
            _dragOrigin = null;
            _dragLock = DragLock.Unset;
            _dragging = false;
            _dragged = false;
            _flickDirection = 0;
            _lastDragOffsetInt = 0;
        }

        private void ClearFocusOnChild()
        {
            DependencyObject focused = Keyboard.FocusedElement as DependencyObject;
            PanoramaItem container = FindAncestorPanoramaItem(focused);
            if (container != null && !ReferenceEquals(container, GetContainer(SelectedItem)))
            {
                Keyboard.ClearFocus();
            }
        }

        private static PanoramaItem FindAncestorPanoramaItem(DependencyObject element)
        {
            while (element != null)
            {
                if (element is PanoramaItem panoramaItem)
                {
                    return panoramaItem;
                }
                element = VisualTreeHelper.GetParent(element);
            }
            return null;
        }

        private static void OnSelectionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            Panorama panorama = (Panorama)obj;
            panorama.ClearFocusOnChild();
            panorama.SelectedIndex = panorama.Items.IndexOf(args.NewValue);
            if (panorama._suppressSelectionChangedEvent || !panorama.Items.Contains(args.NewValue))
            {
                return;
            }

            List<object> removed = args.OldValue == null
                ? new List<object>()
                : new List<object> { args.OldValue };
            panorama.OnSelectionChanged(new SelectionChangedEventArgs(
                SelectionChangedEvent,
                removed,
                new List<object> { args.NewValue }));
        }

        private static void OnDefaultItemChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ((Panorama)obj).OnDefaultItemSet();
        }

        private void OnDefaultItemSet()
        {
            if (Panel == null)
            {
                return;
            }

            Panel.NotifyDefaultItemChanged();
            if (Panel.VisibleChildren.Count > 0)
            {
                SelectedItem = DefaultItem;
            }
            if (_panningItems != null)
            {
                _panningItems.Refresh();
            }
            GoTo(0, Immediately);
        }

        private static void OnBackgroundShadowChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            Panorama panorama = (Panorama)obj;
            if (!panorama._updateBackgroundPending)
            {
                panorama.UpdateBackground();
            }
        }

        private void UpdateBackground()
        {
            if (_panningBackground == null)
            {
                return;
            }

            _updateBackgroundPending = false;
            ContentPresenter presenter = _panningBackground.ContentPresenter;
            if (presenter == null)
            {
                return;
            }

            presenter.Height = ViewportHeight;
            if (Background is SolidColorBrush)
            {
                presenter.Width = ViewportWidth;
                _panningBackground.IsStatic = true;
            }
            else if (Background is GradientBrush)
            {
                presenter.Width = Math.Max(ItemsWidth, ViewportWidth);
                _panningBackground.IsStatic = presenter.Width == ViewportWidth;
            }
            else if (Background is ImageBrush imageBrush)
            {
                BitmapImage bitmap = imageBrush.ImageSource as BitmapImage;
                if (bitmap != null)
                {
                    if (bitmap.PixelWidth == 0)
                    {
                        UnsubscribeBackgroundImage();
                        _backgroundImage = bitmap;
                        bitmap.DownloadCompleted += OnBackgroundImageDownloaded;
                    }
                    presenter.Width = bitmap.PixelWidth;
                }
                _panningBackground.IsStatic = false;
            }

            _panningBackground.Refresh();
        }

        private void UnsubscribeBackgroundImage()
        {
            if (_backgroundImage != null)
            {
                _backgroundImage.DownloadCompleted -= OnBackgroundImageDownloaded;
                _backgroundImage = null;
            }
        }

        private void OnBackgroundImageDownloaded(object sender, EventArgs e)
        {
            UnsubscribeBackgroundImage();
            UpdateBackground();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ViewportWidth = (int)e.NewSize.Width;
            ViewportHeight = (int)e.NewSize.Height;
            if (Panel != null)
            {
                ItemsWidth = (int)Panel.ActualWidth;
            }
            UpdateBackground();
            Clip = new RectangleGeometry(new Rect(0.0, 0.0, e.NewSize.Width, e.NewSize.Height));
        }
    }
}
