// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents a collection of items that can be expanded and collapsed.
    /// </summary>
    [TemplateVisualState(Name = "Collapsed", GroupName = "ExpansionStates")]
    [TemplateVisualState(Name = "Expanded", GroupName = "ExpansionStates")]
    [TemplateVisualState(Name = "Expandable", GroupName = "ExpandabilityStates")]
    [TemplateVisualState(Name = "NonExpandable", GroupName = "ExpandabilityStates")]
    [TemplatePart(Name = "Presenter", Type = typeof(ItemsPresenter))]
    [TemplatePart(Name = "ExpanderPanel", Type = typeof(Grid))]
    public class ExpanderView : HeaderedItemsControl
    {
        private const int KeyTimeStep = 35;
        private const int InitialKeyTime = 225;
        private const int FinalKeyTime = 250;

        private ItemsPresenter _presenter;
        private Canvas _itemsCanvas;
        private Grid _expanderPanel;

        /// <summary>
        /// Occurs when the view expands.
        /// </summary>
        public event RoutedEventHandler Expanded;

        /// <summary>
        /// Occurs when the view collapses.
        /// </summary>
        public event RoutedEventHandler Collapsed;

        /// <summary>
        /// Identifies the <see cref="Expander"/> property.
        /// </summary>
        public static readonly DependencyProperty ExpanderProperty =
            DependencyProperty.Register("Expander", typeof(object), typeof(ExpanderView), new PropertyMetadata(null, OnExpanderPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="ExpanderTemplate"/> property.
        /// </summary>
        public static readonly DependencyProperty ExpanderTemplateProperty =
            DependencyProperty.Register("ExpanderTemplate", typeof(DataTemplate), typeof(ExpanderView), new PropertyMetadata(null, OnExpanderTemplatePropertyChanged));

        /// <summary>
        /// Identifies the <see cref="NonExpandableHeader"/> property.
        /// </summary>
        public static readonly DependencyProperty NonExpandableHeaderProperty =
            DependencyProperty.Register("NonExpandableHeader", typeof(object), typeof(ExpanderView), new PropertyMetadata(null, OnNonExpandableHeaderPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="NonExpandableHeaderTemplate"/> property.
        /// </summary>
        public static readonly DependencyProperty NonExpandableHeaderTemplateProperty =
            DependencyProperty.Register("NonExpandableHeaderTemplate", typeof(DataTemplate), typeof(ExpanderView), new PropertyMetadata(null, OnNonExpandableHeaderTemplatePropertyChanged));

        /// <summary>
        /// Identifies the <see cref="IsExpanded"/> property.
        /// </summary>
        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register("IsExpanded", typeof(bool), typeof(ExpanderView), new PropertyMetadata(false, OnIsExpandedPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="HasItems"/> property.
        /// </summary>
        public static new readonly DependencyProperty HasItemsProperty =
            DependencyProperty.Register("HasItems", typeof(bool), typeof(ExpanderView), new PropertyMetadata(false));

        /// <summary>
        /// Identifies the <see cref="IsNonExpandable"/> property.
        /// </summary>
        public static readonly DependencyProperty IsNonExpandableProperty =
            DependencyProperty.Register("IsNonExpandable", typeof(bool), typeof(ExpanderView), new PropertyMetadata(false, OnIsNonExpandablePropertyChanged));

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpanderView"/> class.
        /// </summary>
        public ExpanderView()
        {
            SizeChanged += OnSizeChanged;
        }

        static ExpanderView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ExpanderView),
                new FrameworkPropertyMetadata(typeof(ExpanderView)));
        }

        /// <summary>
        /// Gets or sets the expander object.
        /// </summary>
        public object Expander
        {
            get { return GetValue(ExpanderProperty); }
            set { SetValue(ExpanderProperty, value); }
        }

        /// <summary>
        /// Gets or sets the expander template.
        /// </summary>
        public DataTemplate ExpanderTemplate
        {
            get { return (DataTemplate)GetValue(ExpanderTemplateProperty); }
            set { SetValue(ExpanderTemplateProperty, value); }
        }

        /// <summary>
        /// Gets or sets the header shown when the view is non-expandable.
        /// </summary>
        public object NonExpandableHeader
        {
            get { return GetValue(NonExpandableHeaderProperty); }
            set { SetValue(NonExpandableHeaderProperty, value); }
        }

        /// <summary>
        /// Gets or sets the template for the non-expandable header.
        /// </summary>
        public DataTemplate NonExpandableHeaderTemplate
        {
            get { return (DataTemplate)GetValue(NonExpandableHeaderTemplateProperty); }
            set { SetValue(NonExpandableHeaderTemplateProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether the view is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set
            {
                if (IsNonExpandable)
                {
                    throw new InvalidOperationException("A non-expandable ExpanderView cannot be expanded.");
                }

                SetValue(IsExpandedProperty, value);
            }
        }

        /// <summary>
        /// Gets whether the view has items.
        /// </summary>
        public new bool HasItems
        {
            get { return (bool)GetValue(HasItemsProperty); }
            private set { SetValue(HasItemsProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether the view can be expanded.
        /// </summary>
        public bool IsNonExpandable
        {
            get { return (bool)GetValue(IsNonExpandableProperty); }
            set { SetValue(IsNonExpandableProperty, value); }
        }

        /// <inheritdoc />
        public override void OnApplyTemplate()
        {
            if (_expanderPanel != null)
            {
                _expanderPanel.MouseLeftButtonUp -= OnExpanderPanelClick;
            }

            if (_presenter != null)
            {
                _presenter.SizeChanged -= OnPresenterSizeChanged;
            }

            base.OnApplyTemplate();

            _expanderPanel = GetTemplateChild("ExpanderPanel") as Grid;
            _presenter = GetTemplateChild("Presenter") as ItemsPresenter;
            _itemsCanvas = GetTemplateChild("ItemsCanvas") as Canvas;

            if (_presenter != null)
            {
                _presenter.SizeChanged += OnPresenterSizeChanged;
            }

            if (_expanderPanel != null)
            {
                _expanderPanel.Background = Brushes.Transparent;
                _expanderPanel.MouseLeftButtonUp += OnExpanderPanelClick;
            }

            UpdateVisualState(false);
        }

        private static void OnIsExpandedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ExpanderView view = (ExpanderView)d;
            RoutedEventArgs args = new RoutedEventArgs();
            if ((bool)e.NewValue)
            {
                view.OnExpanded(args);
            }
            else
            {
                view.OnCollapsed(args);
            }

            view.UpdateVisualState(true);
        }

        private static void OnIsNonExpandablePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ExpanderView view = (ExpanderView)d;
            if ((bool)e.NewValue && view.IsExpanded)
            {
                view.SetValue(IsExpandedProperty, false);
            }

            view.UpdateVisualState(true);
        }

        private static void OnExpanderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ExpanderView)d).OnExpanderChanged(e.OldValue, e.NewValue);
        }

        private static void OnExpanderTemplatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ExpanderView)d).OnExpanderTemplateChanged((DataTemplate)e.OldValue, (DataTemplate)e.NewValue);
        }

        private static void OnNonExpandableHeaderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ExpanderView)d).OnNonExpandableHeaderChanged(e.OldValue, e.NewValue);
        }

        private static void OnNonExpandableHeaderTemplatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ExpanderView)d).OnNonExpandableHeaderTemplateChanged((DataTemplate)e.OldValue, (DataTemplate)e.NewValue);
        }

        /// <inheritdoc />
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            HasItems = Items.Count > 0;
        }

        internal void UpdateVisualState(bool useTransitions)
        {
            if (_presenter != null && _itemsCanvas != null)
            {
                double targetHeight = IsExpanded ? _presenter.DesiredSize.Height : 0;
                if (useTransitions)
                {
                    DoubleAnimation heightAnimation = new DoubleAnimation(targetHeight, TimeSpan.FromMilliseconds(150))
                    {
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                    };
                    _itemsCanvas.BeginAnimation(FrameworkElement.HeightProperty, heightAnimation);

                    DoubleAnimation opacityAnimation = new DoubleAnimation(IsExpanded ? 1.0 : 0.0, TimeSpan.FromMilliseconds(150));
                    _itemsCanvas.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
                }
                else
                {
                    _itemsCanvas.Height = targetHeight;
                    _itemsCanvas.Opacity = IsExpanded ? 1.0 : 0.0;
                }
            }

            VisualStateManager.GoToState(this, IsExpanded ? "Expanded" : "Collapsed", useTransitions);
            VisualStateManager.GoToState(this, IsNonExpandable ? "NonExpandable" : "Expandable", useTransitions);

            if (useTransitions && IsExpanded)
            {
                AnimateContainerDropDown();
            }
        }

        private void OnExpanderPanelClick(object sender, MouseButtonEventArgs e)
        {
            if (!IsNonExpandable)
            {
                IsExpanded = !IsExpanded;
                e.Handled = true;
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_presenter != null)
            {
                try
                {
                    GeneralTransform transform = TransformToVisual(_presenter);
                    Point childToParent = transform.Transform(new Point(0, 0));
                    _presenter.Width = Math.Max(0, RenderSize.Width + childToParent.X);
                }
                catch (ArgumentException)
                {
                }
            }
        }

        private void OnPresenterSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_itemsCanvas != null && _presenter != null && IsExpanded)
            {
                _itemsCanvas.Height = _presenter.DesiredSize.Height;
            }
        }

        internal void AnimateContainerDropDown()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                FrameworkElement container = ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                if (container == null)
                {
                    break;
                }

                Storyboard itemDropDown = new Storyboard();
                IEasingFunction quadraticEase = new QuadraticEase { EasingMode = EasingMode.EaseOut };
                int initialKeyTime = InitialKeyTime + KeyTimeStep * i;
                int finalKeyTime = FinalKeyTime + KeyTimeStep * i;

                TranslateTransform translation = new TranslateTransform();
                container.RenderTransform = translation;

                DoubleAnimationUsingKeyFrames translateAnimation = new DoubleAnimationUsingKeyFrames();
                translateAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(-150.0, KeyTime.FromTimeSpan(TimeSpan.Zero)) { EasingFunction = quadraticEase });
                translateAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(initialKeyTime))) { EasingFunction = quadraticEase });
                translateAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(finalKeyTime))) { EasingFunction = quadraticEase });
                Storyboard.SetTarget(translateAnimation, translation);
                Storyboard.SetTargetProperty(translateAnimation, new PropertyPath(TranslateTransform.YProperty));
                itemDropDown.Children.Add(translateAnimation);

                DoubleAnimationUsingKeyFrames opacityAnimation = new DoubleAnimationUsingKeyFrames();
                opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.Zero)) { EasingFunction = quadraticEase });
                opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(initialKeyTime - 150))) { EasingFunction = quadraticEase });
                opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(finalKeyTime))) { EasingFunction = quadraticEase });
                Storyboard.SetTarget(opacityAnimation, container);
                Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(UIElement.OpacityProperty));
                itemDropDown.Children.Add(opacityAnimation);

                itemDropDown.Begin();
            }
        }

        /// <summary>
        /// Called when <see cref="Expander"/> changes.
        /// </summary>
        protected virtual void OnExpanderChanged(object oldExpander, object newExpander)
        {
        }

        /// <summary>
        /// Called when <see cref="ExpanderTemplate"/> changes.
        /// </summary>
        protected virtual void OnExpanderTemplateChanged(DataTemplate oldTemplate, DataTemplate newTemplate)
        {
        }

        /// <summary>
        /// Called when <see cref="NonExpandableHeader"/> changes.
        /// </summary>
        protected virtual void OnNonExpandableHeaderChanged(object oldHeader, object newHeader)
        {
        }

        /// <summary>
        /// Called when <see cref="NonExpandableHeaderTemplate"/> changes.
        /// </summary>
        protected virtual void OnNonExpandableHeaderTemplateChanged(DataTemplate oldTemplate, DataTemplate newTemplate)
        {
        }

        /// <summary>
        /// Raises the <see cref="Expanded"/> event.
        /// </summary>
        protected virtual void OnExpanded(RoutedEventArgs e)
        {
            RoutedEventHandler handler = Expanded;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="Collapsed"/> event.
        /// </summary>
        protected virtual void OnCollapsed(RoutedEventArgs e)
        {
            RoutedEventHandler handler = Collapsed;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
