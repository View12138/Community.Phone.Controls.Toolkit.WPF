// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// A rating control that displays a configurable number of rating items.
    /// </summary>
    [TemplatePart(Name = FilledClipElement, Type = typeof(UIElement))]
    [TemplatePart(Name = FilledGridElement, Type = typeof(Grid))]
    [TemplatePart(Name = UnfilledGridElement, Type = typeof(Grid))]
    [TemplatePart(Name = DragBorderElement, Type = typeof(Border))]
    [TemplatePart(Name = DragTextBlockElement, Type = typeof(TextBlock))]
    [TemplateVisualState(Name = DragHelperCollapsed, GroupName = DragHelperStates)]
    [TemplateVisualState(Name = DragHelperVisible, GroupName = DragHelperStates)]
    public class Rating : Control
    {
        private const string FilledClipElement = "FilledClipElement";
        private const string FilledGridElement = "FilledGridElement";
        private const string UnfilledGridElement = "UnfilledGridElement";
        private const string DragBorderElement = "DragBorderElement";
        private const string DragTextBlockElement = "DragTextBlockElement";
        private const string DragHelperStates = "DragHelperStates";
        private const string DragHelperCollapsed = "Collapsed";
        private const string DragHelperVisible = "Visible";

        private UIElement _filledClipElement;
        private Grid _filledGridElement;
        private Grid _unfilledGridElement;
        private Border _dragBorderElement;
        private TextBlock _dragTextBlockElement;
        private Geometry _clippingMask;
        private readonly List<RatingItem> _filledItemCollection = new List<RatingItem>();
        private readonly List<RatingItem> _unfilledItemCollection = new List<RatingItem>();
        private bool _isMouseTracking;

        /// <summary>
        /// Occurs when <see cref="Value"/> changes.
        /// </summary>
        public event EventHandler ValueChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="Rating"/> class.
        /// </summary>
        public Rating()
        {
            SizeChanged += OnSizeChanged;
            IsManipulationEnabled = true;
            ManipulationStarted += OnManipulationStarted;
            ManipulationDelta += OnManipulationDelta;
            ManipulationCompleted += OnManipulationCompleted;

            AdjustNumberOfRatingItems();
            SynchronizeGrids();
            UpdateClippingMask();
        }

        static Rating()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(Rating),
                new FrameworkPropertyMetadata(typeof(Rating)));
        }

        /// <summary>
        /// Identifies the <see cref="FilledItemStyle"/> property.
        /// </summary>
        public static readonly DependencyProperty FilledItemStyleProperty =
            DependencyProperty.Register("FilledItemStyle", typeof(Style), typeof(Rating), new PropertyMetadata(null, OnFilledItemStyleChanged));

        /// <summary>
        /// Identifies the <see cref="UnfilledItemStyle"/> property.
        /// </summary>
        public static readonly DependencyProperty UnfilledItemStyleProperty =
            DependencyProperty.Register("UnfilledItemStyle", typeof(Style), typeof(Rating), new PropertyMetadata(null, OnUnfilledItemStyleChanged));

        /// <summary>
        /// Identifies the <see cref="RatingItemCount"/> property.
        /// </summary>
        public static readonly DependencyProperty RatingItemCountProperty =
            DependencyProperty.Register("RatingItemCount", typeof(int), typeof(Rating), new PropertyMetadata(5, OnRatingItemCountChanged));

        /// <summary>
        /// Identifies the <see cref="Value"/> property.
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(Rating), new PropertyMetadata(0.0, OnValueChanged));

        /// <summary>
        /// Identifies the <see cref="ReadOnly"/> property.
        /// </summary>
        public static readonly DependencyProperty ReadOnlyProperty =
            DependencyProperty.Register("ReadOnly", typeof(bool), typeof(Rating), new PropertyMetadata(false));

        /// <summary>
        /// Identifies the <see cref="AllowHalfItemIncrement"/> property.
        /// </summary>
        public static readonly DependencyProperty AllowHalfItemIncrementProperty =
            DependencyProperty.Register("AllowHalfItemIncrement", typeof(bool), typeof(Rating), new PropertyMetadata(false));

        /// <summary>
        /// Identifies the <see cref="AllowSelectingZero"/> property.
        /// </summary>
        public static readonly DependencyProperty AllowSelectingZeroProperty =
            DependencyProperty.Register("AllowSelectingZero", typeof(bool), typeof(Rating), new PropertyMetadata(false));

        /// <summary>
        /// Identifies the <see cref="ShowSelectionHelper"/> property.
        /// </summary>
        public static readonly DependencyProperty ShowSelectionHelperProperty =
            DependencyProperty.Register("ShowSelectionHelper", typeof(bool), typeof(Rating), new PropertyMetadata(false));

        /// <summary>
        /// Identifies the <see cref="Orientation"/> property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(Rating), new PropertyMetadata(Orientation.Horizontal, OnOrientationChanged));

        /// <summary>
        /// Gets or sets the style applied to filled items.
        /// </summary>
        public Style FilledItemStyle
        {
            get { return (Style)GetValue(FilledItemStyleProperty); }
            set { SetValue(FilledItemStyleProperty, value); }
        }

        /// <summary>
        /// Gets or sets the style applied to unfilled items.
        /// </summary>
        public Style UnfilledItemStyle
        {
            get { return (Style)GetValue(UnfilledItemStyleProperty); }
            set { SetValue(UnfilledItemStyleProperty, value); }
        }

        /// <summary>
        /// Gets or sets the number of rating items.
        /// </summary>
        public int RatingItemCount
        {
            get { return (int)GetValue(RatingItemCountProperty); }
            set { SetValue(RatingItemCountProperty, value); }
        }

        /// <summary>
        /// Gets or sets the current rating value.
        /// </summary>
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set
            {
                SetValue(ValueProperty, value);
                EventHandler handler = ValueChanged;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the control is read-only.
        /// </summary>
        public bool ReadOnly
        {
            get { return (bool)GetValue(ReadOnlyProperty); }
            set { SetValue(ReadOnlyProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether half-item increments are allowed.
        /// </summary>
        public bool AllowHalfItemIncrement
        {
            get { return (bool)GetValue(AllowHalfItemIncrementProperty); }
            set { SetValue(AllowHalfItemIncrementProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether zero can be selected.
        /// </summary>
        public bool AllowSelectingZero
        {
            get { return (bool)GetValue(AllowSelectingZeroProperty); }
            set { SetValue(AllowSelectingZeroProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether the drag selection helper is shown.
        /// </summary>
        public bool ShowSelectionHelper
        {
            get { return (bool)GetValue(ShowSelectionHelperProperty); }
            set { SetValue(ShowSelectionHelperProperty, value); }
        }

        /// <summary>
        /// Gets or sets the orientation of the items.
        /// </summary>
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        /// <inheritdoc />
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _filledClipElement = GetTemplateChild(FilledClipElement) as UIElement;
            _filledGridElement = GetTemplateChild(FilledGridElement) as Grid;
            _unfilledGridElement = GetTemplateChild(UnfilledGridElement) as Grid;
            _dragBorderElement = GetTemplateChild(DragBorderElement) as Border;
            _dragTextBlockElement = GetTemplateChild(DragTextBlockElement) as TextBlock;

            if (_filledClipElement != null)
            {
                _filledClipElement.Clip = _clippingMask;
            }

            if (_dragBorderElement != null)
            {
                _dragBorderElement.RenderTransform = new TranslateTransform();
            }

            VisualStateManager.GoToState(this, DragHelperCollapsed, false);
            SynchronizeGrids();
        }

        /// <inheritdoc />
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            if (ReadOnly)
            {
                return;
            }

            _isMouseTracking = true;
            CaptureMouse();
            UpdateValueFromMouse(e.GetPosition(this));
            ChangeDragHelperVisibility(ShowSelectionHelper);
            e.Handled = true;
        }

        /// <inheritdoc />
        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);

            if (_isMouseTracking && e.LeftButton == MouseButtonState.Pressed)
            {
                UpdateValueFromMouse(e.GetPosition(this));
                e.Handled = true;
            }
        }

        /// <inheritdoc />
        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonUp(e);

            if (_isMouseTracking)
            {
                UpdateValueFromMouse(e.GetPosition(this));
                _isMouseTracking = false;
                ReleaseMouseCapture();
                ChangeDragHelperVisibility(false);
                e.Handled = true;
            }
        }

        /// <inheritdoc />
        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            base.OnLostMouseCapture(e);

            _isMouseTracking = false;
            ChangeDragHelperVisibility(false);
        }

        private void UpdateValueFromMouse(Point position)
        {
            PerformValueCalculation(position, this);
            UpdateDragHelper();
        }

        private void OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            if (!ReadOnly)
            {
                PerformValueCalculation(e.ManipulationOrigin, e.ManipulationContainer);
                UpdateDragHelper();
            }
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (!ReadOnly)
            {
                PerformValueCalculation(e.ManipulationOrigin, e.ManipulationContainer);
                UpdateDragHelper();
                if (ShowSelectionHelper)
                {
                    ChangeDragHelperVisibility(true);
                }
            }
        }

        private void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (!ReadOnly)
            {
                PerformValueCalculation(e.ManipulationOrigin, e.ManipulationContainer);
            }

            ChangeDragHelperVisibility(false);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateClippingMask();
        }

        private void ChangeDragHelperVisibility(bool isVisible)
        {
            if (_dragBorderElement == null)
            {
                return;
            }

            VisualStateManager.GoToState(this, isVisible ? DragHelperVisible : DragHelperCollapsed, true);
        }

        private void UpdateDragHelper()
        {
            if (RatingItemCount == 0)
            {
                return;
            }

            string format = AllowHalfItemIncrement ? "F1" : "F0";
            if (_dragTextBlockElement != null)
            {
                _dragTextBlockElement.Text = Value.ToString(format, CultureInfo.CurrentCulture);
            }

            if (_dragBorderElement == null)
            {
                return;
            }

            TranslateTransform transform = _dragBorderElement.RenderTransform as TranslateTransform;
            if (transform == null)
            {
                transform = new TranslateTransform();
                _dragBorderElement.RenderTransform = transform;
            }

            if (Orientation == Orientation.Horizontal)
            {
                double distanceToCenter = _dragBorderElement.ActualWidth / 2;
                double itemCenter = _filledItemCollection.Count > 0 ? _filledItemCollection[0].ActualWidth / 2 : 0;
                transform.X = (!AllowHalfItemIncrement && !AllowSelectingZero)
                    ? (Value / RatingItemCount) * ActualWidth - distanceToCenter - itemCenter
                    : (Value / RatingItemCount) * ActualWidth - distanceToCenter;
                transform.Y = -(ActualHeight / 2 + 15);
            }
            else
            {
                double distanceToCenter = _dragBorderElement.ActualHeight / 2;
                double itemCenter = _filledItemCollection.Count > 0 ? _filledItemCollection[0].ActualHeight / 2 : 0;
                transform.Y = (!AllowHalfItemIncrement && !AllowSelectingZero)
                    ? (Value / RatingItemCount) * ActualHeight - distanceToCenter - itemCenter
                    : (Value / RatingItemCount) * ActualHeight - distanceToCenter;
                transform.X = -(ActualWidth / 2 + 15);
            }
        }

        private void PerformValueCalculation(Point location, IInputElement locationRelativeSource)
        {
            UIElement relativeElement = locationRelativeSource as UIElement;
            if (relativeElement == null)
            {
                return;
            }

            GeneralTransform transform = relativeElement.TransformToVisual(this);
            location = transform.Transform(location);

            int numberOfPositions = _filledItemCollection.Count;
            if (AllowHalfItemIncrement)
            {
                numberOfPositions *= 2;
            }

            double size = Orientation == Orientation.Horizontal ? ActualWidth : ActualHeight;
            if (size <= 0)
            {
                return;
            }

            double position = Orientation == Orientation.Horizontal ? location.X : location.Y;
            double newValue = Math.Ceiling(position / size * numberOfPositions);
            newValue = Math.Max(0, Math.Min(numberOfPositions, newValue));

            if (!AllowSelectingZero && newValue <= 0)
            {
                newValue = 1;
            }

            Value = newValue;
        }

        private void UpdateClippingMask()
        {
            if (RatingItemCount <= 0)
            {
                return;
            }

            Rect newRect;
            if (Orientation == Orientation.Horizontal)
            {
                double width = Math.Max(0, ActualWidth - BorderThickness.Right - BorderThickness.Left);
                newRect = new Rect(0, 0, width * (Value / RatingItemCount), ActualHeight);
            }
            else
            {
                double height = Math.Max(0, ActualHeight - BorderThickness.Top - BorderThickness.Bottom);
                newRect = new Rect(0, 0, ActualWidth, height * (Value / RatingItemCount));
            }

            RectangleGeometry geometry = _clippingMask as RectangleGeometry;
            if (geometry != null)
            {
                geometry.Rect = newRect;
            }
            else
            {
                geometry = new RectangleGeometry { Rect = newRect };
                _clippingMask = geometry;
            }

            if (_filledClipElement != null)
            {
                _filledClipElement.Clip = _clippingMask;
            }
        }

        private static RatingItem BuildNewRatingItem(Style style)
        {
            RatingItem item = new RatingItem();
            if (style != null)
            {
                item.Style = style;
            }

            return item;
        }

        private void AdjustNumberOfRatingItems()
        {
            while (_filledItemCollection.Count > RatingItemCount)
            {
                _filledItemCollection.RemoveAt(0);
            }

            while (_unfilledItemCollection.Count > RatingItemCount)
            {
                _unfilledItemCollection.RemoveAt(0);
            }

            while (_filledItemCollection.Count < RatingItemCount)
            {
                _filledItemCollection.Add(BuildNewRatingItem(FilledItemStyle));
            }

            while (_unfilledItemCollection.Count < RatingItemCount)
            {
                _unfilledItemCollection.Add(BuildNewRatingItem(UnfilledItemStyle));
            }
        }

        private void SynchronizeGrid(Grid grid, IList<RatingItem> items)
        {
            if (grid == null)
            {
                return;
            }

            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();
            grid.Children.Clear();

            if (Orientation == Orientation.Horizontal)
            {
                while (grid.ColumnDefinitions.Count < items.Count)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                }

                for (int i = 0; i < items.Count; i++)
                {
                    grid.Children.Add(items[i]);
                    Grid.SetColumn(items[i], i);
                    Grid.SetRow(items[i], 0);
                }
            }
            else
            {
                while (grid.RowDefinitions.Count < items.Count)
                {
                    grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                }

                for (int i = 0; i < items.Count; i++)
                {
                    grid.Children.Add(items[i]);
                    Grid.SetRow(items[i], i);
                    Grid.SetColumn(items[i], 0);
                }
            }
        }

        private void SynchronizeGrids()
        {
            SynchronizeGrid(_unfilledGridElement, _unfilledItemCollection);
            SynchronizeGrid(_filledGridElement, _filledItemCollection);
        }

        private static void OnFilledItemStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Rating rating = (Rating)d;
            foreach (RatingItem item in rating._filledItemCollection)
            {
                if (item.Style == null)
                {
                    item.Style = rating.FilledItemStyle;
                }
            }
        }

        private static void OnUnfilledItemStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Rating rating = (Rating)d;
            foreach (RatingItem item in rating._unfilledItemCollection)
            {
                if (item.Style == null)
                {
                    item.Style = rating.UnfilledItemStyle;
                }
            }
        }

        private static void OnRatingItemCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Rating rating = (Rating)d;
            if (rating.RatingItemCount <= 0)
            {
                rating.RatingItemCount = 0;
            }

            rating.AdjustNumberOfRatingItems();
            rating.SynchronizeGrids();
            rating.UpdateClippingMask();
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Rating rating = (Rating)d;
            if (rating.Value > rating.RatingItemCount || rating.Value < 0)
            {
                rating.Value = Math.Max(0, Math.Min(rating.RatingItemCount, rating.Value));
            }

            rating.UpdateClippingMask();
        }

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Rating rating = (Rating)d;
            rating.SynchronizeGrids();
            rating.UpdateClippingMask();
        }
    }

    /// <summary>
    /// A control used by <see cref="Rating"/> so that custom item templates
    /// can be supplied.
    /// </summary>
    public class RatingItem : Control
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RatingItem"/> class.
        /// </summary>
        public RatingItem()
        {
        }

        static RatingItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(RatingItem),
                new FrameworkPropertyMetadata(typeof(RatingItem)));
        }

        /// <summary>
        /// Identifies the <see cref="StrokeThickness"/> property.
        /// </summary>
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register("StrokeThickness", typeof(double), typeof(RatingItem), null);

        /// <summary>
        /// Gets or sets the stroke thickness of the item shape.
        /// </summary>
        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }
    }
}
