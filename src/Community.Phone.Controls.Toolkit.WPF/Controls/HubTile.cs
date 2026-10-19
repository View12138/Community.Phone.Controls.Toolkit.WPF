// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents an animated Metro-style hub tile.
    /// </summary>
    [TemplateVisualState(Name = "Expanded", GroupName = "ImageState")]
    [TemplateVisualState(Name = "Semiexpanded", GroupName = "ImageState")]
    [TemplateVisualState(Name = "Collapsed", GroupName = "ImageState")]
    [TemplateVisualState(Name = "Flipped", GroupName = "ImageState")]
    public class HubTile : Control
    {
        private bool _canDrop;
        private bool _canFlip;
        private DispatcherTimer _timer;
        private int _step;
        private TextBlock _notificationBlock;
        private TextBlock _messageBlock;

        /// <summary>
        /// Identifies the <see cref="Source"/> property.
        /// </summary>
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ImageSource), typeof(HubTile), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="Title"/> property.
        /// </summary>
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(HubTile), new PropertyMetadata(string.Empty, OnTitleChanged));

        /// <summary>
        /// Identifies the <see cref="Notification"/> property.
        /// </summary>
        public static readonly DependencyProperty NotificationProperty =
            DependencyProperty.Register("Notification", typeof(string), typeof(HubTile), new PropertyMetadata(string.Empty, OnBackContentChanged));

        /// <summary>
        /// Identifies the <see cref="Message"/> property.
        /// </summary>
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(HubTile), new PropertyMetadata(string.Empty, OnBackContentChanged));

        /// <summary>
        /// Identifies the <see cref="DisplayNotification"/> property.
        /// </summary>
        public static readonly DependencyProperty DisplayNotificationProperty =
            DependencyProperty.Register("DisplayNotification", typeof(bool), typeof(HubTile), new PropertyMetadata(false, OnBackContentChanged));

        /// <summary>
        /// Identifies the <see cref="IsFrozen"/> property.
        /// </summary>
        public static readonly DependencyProperty IsFrozenProperty =
            DependencyProperty.Register("IsFrozen", typeof(bool), typeof(HubTile), new PropertyMetadata(false, OnIsFrozenChanged));

        /// <summary>
        /// Identifies the <see cref="GroupTag"/> property.
        /// </summary>
        public static readonly DependencyProperty GroupTagProperty =
            DependencyProperty.Register("GroupTag", typeof(string), typeof(HubTile), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Identifies the <see cref="Size"/> property.
        /// </summary>
        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register("Size", typeof(TileSize), typeof(HubTile), new PropertyMetadata(TileSize.Default, OnSizeChanged));

        internal static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(ImageState), typeof(HubTile), new PropertyMetadata(ImageState.Expanded, OnStateChanged));

        /// <summary>
        /// Initializes a new instance of the <see cref="HubTile"/> class.
        /// </summary>
        public HubTile()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            SizeChanged += OnSizeChangedInternal;
        }

        static HubTile()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(HubTile),
                new FrameworkPropertyMetadata(typeof(HubTile)));
        }

        /// <summary>
        /// Gets or sets the image source.
        /// </summary>
        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        /// <summary>
        /// Gets or sets the notification text.
        /// </summary>
        public string Notification
        {
            get { return (string)GetValue(NotificationProperty); }
            set { SetValue(NotificationProperty, value); }
        }

        /// <summary>
        /// Gets or sets the back message.
        /// </summary>
        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether the notification is displayed instead of the message.
        /// </summary>
        public bool DisplayNotification
        {
            get { return (bool)GetValue(DisplayNotificationProperty); }
            set { SetValue(DisplayNotificationProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether the tile animation is frozen.
        /// </summary>
        public bool IsFrozen
        {
            get { return (bool)GetValue(IsFrozenProperty); }
            set { SetValue(IsFrozenProperty, value); }
        }

        /// <summary>
        /// Gets or sets a group tag used to freeze a set of tiles.
        /// </summary>
        public string GroupTag
        {
            get { return (string)GetValue(GroupTagProperty); }
            set { SetValue(GroupTagProperty, value); }
        }

        /// <summary>
        /// Gets or sets the tile size.
        /// </summary>
        public TileSize Size
        {
            get { return (TileSize)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        internal ImageState State
        {
            get { return (ImageState)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        /// <inheritdoc />
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _notificationBlock = GetTemplateChild("NotificationBlock") as TextBlock;
            _messageBlock = GetTemplateChild("MessageBlock") as TextBlock;
            UpdateBackContentVisibility();
            UpdateVisualState(false);
        }

        private void UpdateBackContentVisibility()
        {
            if (_notificationBlock != null)
            {
                _notificationBlock.Visibility = DisplayNotification ? Visibility.Visible : Visibility.Collapsed;
            }

            if (_messageBlock != null)
            {
                _messageBlock.Visibility = DisplayNotification ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HubTile tile = (HubTile)d;
            if (string.IsNullOrEmpty((string)e.NewValue))
            {
                tile._canDrop = false;
                tile.State = ImageState.Expanded;
            }
            else
            {
                tile._canDrop = true;
            }
        }

        private static void OnBackContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HubTile tile = (HubTile)d;
            bool hasNotification = !string.IsNullOrEmpty(tile.Notification) && tile.DisplayNotification;
            bool hasMessage = !string.IsNullOrEmpty(tile.Message) && !tile.DisplayNotification;
            tile._canFlip = hasNotification || hasMessage;
            tile.UpdateBackContentVisibility();
            if (!tile._canFlip)
            {
                tile.State = ImageState.Expanded;
            }
        }

        private static void OnIsFrozenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HubTile tile = (HubTile)d;
            if ((bool)e.NewValue)
            {
                tile.StopTimer();
            }
            else
            {
                tile.StartTimer();
            }
        }

        private static void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HubTile)d).ApplySize();
        }

        private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HubTile)d).UpdateVisualState(true);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ApplySize();
            StartTimer();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            StopTimer();
        }

        private void OnSizeChangedInternal(object sender, SizeChangedEventArgs e)
        {
        }

        private void ApplySize()
        {
            switch (Size)
            {
                case TileSize.Small:
                    Width = 99;
                    Height = 99;
                    break;
                case TileSize.Medium:
                    Width = 210;
                    Height = 210;
                    break;
                case TileSize.Large:
                    Width = 432;
                    Height = 210;
                    break;
                default:
                    Width = 173;
                    Height = 173;
                    break;
            }
        }

        private void StartTimer()
        {
            if (IsFrozen || HubTileService.IsGroupFrozen(GroupTag))
            {
                return;
            }

            if (_timer == null)
            {
                _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
                _timer.Tick += OnTimerTick;
            }

            _timer.Start();
        }

        private void StopTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
            }
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            if (IsFrozen || HubTileService.IsGroupFrozen(GroupTag))
            {
                StopTimer();
                return;
            }

            _step++;
            if (_step % 6 == 0)
            {
                State = ImageState.Expanded;
            }
            else if (_step % 6 == 1 && _canDrop)
            {
                State = ImageState.Semiexpanded;
            }
            else if (_step % 6 == 2)
            {
                State = ImageState.Collapsed;
            }
            else if (_step % 6 == 3 && _canFlip)
            {
                State = ImageState.Flipped;
            }
            else
            {
                State = ImageState.Expanded;
            }
        }

        private void UpdateVisualState(bool useTransitions)
        {
            VisualStateManager.GoToState(this, State.ToString(), useTransitions);
        }
    }

    internal enum ImageState
    {
        Expanded = 0,
        Semiexpanded = 1,
        Collapsed = 2,
        Flipped = 3
    }

    /// <summary>
    /// Represents the size of a hub tile.
    /// </summary>
    public enum TileSize
    {
        /// <summary>Default size (173 x 173).</summary>
        Default,
        /// <summary>Small size (99 x 99).</summary>
        Small,
        /// <summary>Medium size (210 x 210).</summary>
        Medium,
        /// <summary>Large size (432 x 210).</summary>
        Large
    }
}
