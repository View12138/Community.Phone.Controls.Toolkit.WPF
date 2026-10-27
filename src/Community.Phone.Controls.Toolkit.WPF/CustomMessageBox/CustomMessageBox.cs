// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using Microsoft.Phone.Shell;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents a popup dialog with one or two buttons.
    /// </summary>
    [TemplatePart(Name = TitleTextBlock, Type = typeof(TextBlock))]
    [TemplatePart(Name = CaptionTextBlock, Type = typeof(TextBlock))]
    [TemplatePart(Name = MessageTextBlock, Type = typeof(TextBlock))]
    [TemplatePart(Name = LeftButton, Type = typeof(ButtonBase))]
    [TemplatePart(Name = RightButton, Type = typeof(ButtonBase))]
    public class CustomMessageBox : ContentControl
    {
        private static CustomMessageBox _currentInstance;

        private Window _window;
        private bool _closeRequested;
        private CustomMessageBoxResult _result = CustomMessageBoxResult.None;
        private bool _isDismissing;


        /// <summary>
        /// Title text block template part name.
        /// </summary>
        private const string TitleTextBlock = "TitleTextBlock";

        /// <summary>
        /// Caption text block template part name.
        /// </summary>
        private const string CaptionTextBlock = "CaptionTextBlock";

        /// <summary>
        /// Message text block template part name.
        /// </summary>
        private const string MessageTextBlock = "MessageTextBlock";

        /// <summary>
        /// Left button template part name.
        /// </summary>
        private const string LeftButton = "LeftButton";

        /// <summary>
        /// Right button template part name.
        /// </summary>
        private const string RightButton = "RightButton";

        /// <summary>
        /// Title text block template part.
        /// </summary>
        private TextBlock _titleTextBlock;

        /// <summary>
        /// Caption text block template part.
        /// </summary>
        private TextBlock _captionTextBlock;

        /// <summary>
        /// Message text block template part.
        /// </summary>
        private TextBlock _messageTextBlock;

        /// <summary>
        /// Left button template part.
        /// </summary>
        private Button _leftButton;

        /// <summary>
        /// Right button template part.
        /// </summary>
        private Button _rightButton;

        /// <summary>
        /// The child container of the popup.
        /// </summary>
        private Grid _container;

        #region Title DependencyProperty

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        /// <summary>
        /// Identifies the Title dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(CustomMessageBox), new PropertyMetadata(string.Empty, OnTitlePropertyChanged));

        /// <summary>
        /// Changes the visibility of the title text block based on its content.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="e">The event information.</param>
        private static void OnTitlePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CustomMessageBox target = (CustomMessageBox)obj;

            if (target._titleTextBlock != null)
            {
                string value = (string)e.NewValue;
                target._titleTextBlock.Visibility = GetVisibilityFromString(value);
            }
        }

        #endregion

        #region Caption DependencyProperty

        /// <summary>
        /// Gets or sets the caption.
        /// </summary>
        public string Caption
        {
            get { return (string)GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }

        /// <summary>
        /// Identifies the Caption dependency property.
        /// </summary>
        public static readonly DependencyProperty CaptionProperty =
            DependencyProperty.Register("Caption", typeof(string), typeof(CustomMessageBox), new PropertyMetadata(string.Empty, OnCaptionPropertyChanged));

        /// <summary>
        /// Changes the visibility of the caption text block based on its content.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="e">The event information.</param>
        private static void OnCaptionPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CustomMessageBox target = (CustomMessageBox)obj;

            if (target._captionTextBlock != null)
            {
                string value = (string)e.NewValue;
                target._captionTextBlock.Visibility = GetVisibilityFromString(value);
            }
        }

        #endregion

        #region Message DependencyProperty

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        /// <summary>
        /// Identifies the Message dependency property.
        /// </summary>
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(CustomMessageBox), new PropertyMetadata(string.Empty, OnMessagePropertyChanged));

        /// <summary>
        /// Changes the visibility of the message text block based on its content.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="e">The event information.</param>
        private static void OnMessagePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CustomMessageBox target = (CustomMessageBox)obj;

            if (target._messageTextBlock != null)
            {
                string value = (string)e.NewValue;
                target._messageTextBlock.Visibility = GetVisibilityFromString(value);
            }
        }

        #endregion

        #region LeftButtonContent DependencyProperty

        /// <summary>
        /// Gets or sets the left button content.
        /// </summary>
        public Object LeftButtonContent
        {
            get { return (Object)GetValue(LeftButtonContentProperty); }
            set { SetValue(LeftButtonContentProperty, value); }
        }

        /// <summary>
        /// Identifies the LeftButtonContent dependency property.
        /// </summary>
        public static readonly DependencyProperty LeftButtonContentProperty =
            DependencyProperty.Register("LeftButtonContent", typeof(Object), typeof(CustomMessageBox), new PropertyMetadata(null, OnLeftButtonContentPropertyChanged));

        /// <summary>
        /// Changes the visibility of the left button based on its content.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="e">The event information.</param>
        private static void OnLeftButtonContentPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CustomMessageBox target = (CustomMessageBox)obj;

            if (target._leftButton != null)
            {
                target._leftButton.Visibility = GetVisibilityFromObject(e.NewValue);
            }
        }

        #endregion

        #region RightButtonContent DependencyProperty

        /// <summary>
        /// Gets or sets the right button content.
        /// </summary>
        public Object RightButtonContent
        {
            get { return (Object)GetValue(RightButtonContentProperty); }
            set { SetValue(RightButtonContentProperty, value); }
        }

        /// <summary>
        /// Identifies the RightButtonContent dependency property.
        /// </summary>
        public static readonly DependencyProperty RightButtonContentProperty =
            DependencyProperty.Register("RightButtonContent", typeof(Object), typeof(CustomMessageBox), new PropertyMetadata(null, OnRightButtonContentPropertyChanged));

        /// <summary>
        /// Changes the visibility of the right button based on its content.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="e">The event information.</param>
        private static void OnRightButtonContentPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CustomMessageBox target = (CustomMessageBox)obj;

            if (target._rightButton != null)
            {
                target._rightButton.Visibility = GetVisibilityFromObject(e.NewValue);
            }
        }

        #endregion

        #region IsLeftButtonEnabled DependencyProperty

        /// <summary>
        /// Gets or sets whether the left button is enabled.
        /// </summary>
        public bool IsLeftButtonEnabled
        {
            get { return (bool)GetValue(IsLeftButtonEnabledProperty); }
            set { SetValue(IsLeftButtonEnabledProperty, value); }
        }

        /// <summary>
        /// Identifies the IsLeftButtonEnabled dependency property.
        /// </summary>
        public static readonly DependencyProperty IsLeftButtonEnabledProperty =
            DependencyProperty.Register("IsLeftButtonEnabled", typeof(bool), typeof(CustomMessageBox), new PropertyMetadata(true));

        #endregion

        #region IsRightButtonEnabled DependencyProperty

        /// <summary>
        /// Gets or sets whether the right button is enabled.
        /// </summary>
        public bool IsRightButtonEnabled
        {
            get { return (bool)GetValue(IsRightButtonEnabledProperty); }
            set { SetValue(IsRightButtonEnabledProperty, value); }
        }

        /// <summary>
        /// Identifies the IsRightButtonEnabled dependency property.
        /// </summary>
        public static readonly DependencyProperty IsRightButtonEnabledProperty =
            DependencyProperty.Register("IsRightButtonEnabled", typeof(bool), typeof(CustomMessageBox), new PropertyMetadata(true));

        #endregion

        #region IsFullScreen DependencyProperty

        /// <summary>
        /// Gets or sets whether the message box occupies the whole screen.
        /// </summary>
        public bool IsFullScreen
        {
            get { return (bool)GetValue(IsFullScreenProperty); }
            set { SetValue(IsFullScreenProperty, value); }
        }

        /// <summary>
        /// Identifies the IsFullScreen dependency property.
        /// </summary>
        public static readonly DependencyProperty IsFullScreenProperty =
            DependencyProperty.Register("IsFullScreen", typeof(bool), typeof(CustomMessageBox), new PropertyMetadata(false, OnIsFullScreenPropertyChanged));

        /// <summary>
        /// Modifies the vertical alignment of the message box depending
        /// on whether it should occupy the full screen or not.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="e"></param>
        private static void OnIsFullScreenPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CustomMessageBox target = (CustomMessageBox)obj;

            if ((bool)e.NewValue)
            {
                target.VerticalAlignment = VerticalAlignment.Stretch;
            }
            else
            {
                target.VerticalAlignment = VerticalAlignment.Top;
            }
        }

        #endregion

        #region IsCenterScreen DependencyProperty

        /// <summary>
        /// Gets or sets whether the message box is centered on the screen.
        /// </summary>
        public bool IsCenterScreen
        {
            get { return (bool)GetValue(IsCenterScreenProperty); }
            set { SetValue(IsCenterScreenProperty, value); }
        }

        /// <summary>
        /// Identifies the IsCenterScreen dependency property.
        /// </summary>
        public static readonly DependencyProperty IsCenterScreenProperty =
            DependencyProperty.Register("IsCenterScreen", typeof(bool), typeof(CustomMessageBox), new PropertyMetadata(false));

        #endregion

        /// <summary>
        /// Occurs when the message box is about to be dismissed.
        /// </summary>
        public event EventHandler<DismissingEventArgs> Dismissing;

        /// <summary>
        /// Occurs when the message box is dismissed.
        /// </summary>
        public event EventHandler<DismissedEventArgs> Dismissed;


        /// <summary>
        /// Gets the template parts and attaches event handlers.
        /// Animates the message box when the template is applied to it.
        /// </summary>
        public override void OnApplyTemplate()
        {
            if (_leftButton != null)
            {
                _leftButton.Click -= LeftButton_Click;
            }

            if (_rightButton != null)
            {
                _rightButton.Click -= RightButton_Click;
            }

            base.OnApplyTemplate();

            _titleTextBlock = base.GetTemplateChild(TitleTextBlock) as TextBlock;
            _captionTextBlock = base.GetTemplateChild(CaptionTextBlock) as TextBlock;
            _messageTextBlock = base.GetTemplateChild(MessageTextBlock) as TextBlock;
            _leftButton = base.GetTemplateChild(LeftButton) as Button;
            _rightButton = base.GetTemplateChild(RightButton) as Button;

            if (_titleTextBlock != null)
            {
                _titleTextBlock.Visibility = GetVisibilityFromString(Title);
            }

            if (_captionTextBlock != null)
            {
                _captionTextBlock.Visibility = GetVisibilityFromString(Caption);
            }

            if (_messageTextBlock != null)
            {
                _messageTextBlock.Visibility = GetVisibilityFromString(Message);
            }

            if (_leftButton != null)
            {
                _leftButton.Click += LeftButton_Click;
                _leftButton.Visibility = GetVisibilityFromObject(LeftButtonContent);
            }

            if (_rightButton != null)
            {
                _rightButton.Click += RightButton_Click;
                _rightButton.Visibility = GetVisibilityFromObject(RightButtonContent);
            }
        }
        public CustomMessageBox() : base()
        {
            DefaultStyleKey = typeof(CustomMessageBox);
        }

        private Size _windowSize = new Size(480, 800);
        private Size _clientSize = new Size(480, 768);
        /// <summary>
        /// Shows the message box.
        /// </summary>
        public void Show()
        {
            if (_window != null)
            {
                return;
            }

            if (_currentInstance != null && !ReferenceEquals(_currentInstance, this))
            {
                _currentInstance.Dismiss();
            }

            _currentInstance = this;
            _result = CustomMessageBoxResult.None;
            _isDismissing = false;
            _closeRequested = false;

            // The message box is hosted in a projection host so it can swivel
            // in and out like on Windows Phone.
            Rectangle overlay = new Rectangle();
            Color backgroundColor = Colors.Black;
            object backgroundColorResource = this.FindThemeResource("PhoneBackgroundColor");
            if (backgroundColorResource is Color)
            {
                backgroundColor = (Color)backgroundColorResource;
            }

            overlay.Fill = new SolidColorBrush(Color.FromArgb(0x99, backgroundColor.R, backgroundColor.G, backgroundColor.B));
            _container = new Grid();
            _container.Children.Add(overlay);

            ProjectionHost host = new ProjectionHost
            {
                Content = this,
                VerticalAlignment = IsFullScreen ? VerticalAlignment.Stretch : IsCenterScreen ? VerticalAlignment.Center : VerticalAlignment.Top
            };
            // Insert the message box.
            _container.Children.Add(host);

            // The host window is optional: a host without an Application (a test
            // host, a design surface) still gets a usable dialog.
            Window owner = Application.Current == null ? null : Application.Current.MainWindow;
            if (owner != null)
            {
                _windowSize = WindowHelper.GetVisibleWindowBounds(owner).Size;
                _clientSize = WindowHelper.GetClientSize(owner);
            }
            _window = new Window
            {
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
                Padding = new Thickness(0),
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = Brushes.Transparent,
                AllowsTransparency = true,
                Content = _container,
                Opacity = 0,
                Width = _clientSize.Width,
                Height = _clientSize.Height,
                Owner = owner,
            };

            _window.Closed += OnWindowClosed;
            _window.ContentRendered += OnWindowContentRendered;
            _window.ShowDialog();
        }

        private void OnWindowContentRendered(object sender, EventArgs e)
        {
            Window window = sender as Window;
            window?.ContentRendered -= OnWindowContentRendered;

            double titleHeight = _windowSize.Height - _clientSize.Height;
            if (_window != null && _window.Owner != null)
            {
                _window.Top = _window.Owner.Top + titleHeight;
            }

            _window?.Opacity = 1;
            SwivelTransition backwardIn = new SwivelTransition { Mode = SwivelTransitionMode.BackwardIn };
            ITransition swivelTransition = backwardIn.GetTransition(this);
            //swivelTransition.Completed += (s1, e1) => swivelTransition.Stop();
            swivelTransition.Begin();
        }

        private void CloseWindow()
        {
            if (_closeRequested)
            {
                return;
            }

            _closeRequested = true;

            _window?.Close();
        }

        /// <summary>
        /// Dismisses the message box.
        /// </summary>
        public void Dismiss()
        {
            Dismiss(CustomMessageBoxResult.None, true);
        }

        private void Dismiss(CustomMessageBoxResult result, bool useTransition)
        {
            if (_window == null || _isDismissing)
            {
                return;
            }

            var args = new DismissingEventArgs(result);
            Dismissing?.Invoke(this, args);
            if (args.Cancel)
            {
                return;
            }

            _isDismissing = true;
            _result = result;

            if (_window == null || !_window.IsVisible)
            {
                CloseWindow();
                return;
            }

            if (useTransition)
            {
                SwivelTransition backwardOut = new SwivelTransition { Mode = SwivelTransitionMode.BackwardOut };
                ITransition swivelTransition = backwardOut.GetTransition(this);
                swivelTransition.Completed += (s, e) =>
                {
                    swivelTransition.Stop();
                    CloseWindow();
                };
                swivelTransition.Begin();
            }
            else
            {
                CloseWindow();
            }
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            Window window = _window;
            _window = null;
            _closeRequested = false;
            if (window != null)
            {
                window.Closed -= OnWindowClosed;
                window.ContentRendered -= OnWindowContentRendered;
            }

            if (ReferenceEquals(_currentInstance, this))
            {
                _currentInstance = null;
            }

            _isDismissing = false;
            EventHandler<DismissedEventArgs> handler = Dismissed;
            if (handler != null)
            {
                handler(this, new DismissedEventArgs(_result));
            }
        }

        /// <summary>
        /// Dismisses the message box with the left button.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event information.</param>
        private void LeftButton_Click(object sender, RoutedEventArgs e)
        {
            Dismiss(CustomMessageBoxResult.LeftButton, true);
        }

        /// <summary>
        /// Dismisses the message box with the right button.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event information.</param>
        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            Dismiss(CustomMessageBoxResult.RightButton, true);
        }

        /// <summary>
        /// Returns a visibility value based on the value of a string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>
        /// Visibility.Collapsed if the string is null or empty.
        /// Visibility.Visible otherwise.
        /// </returns>
        private static Visibility GetVisibilityFromString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        /// <summary>
        /// Returns a visibility value based on the value of an object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>
        /// Visibility.Collapsed if the object is null.
        /// Visibility.Visible otherwise.
        /// </returns>
        private static Visibility GetVisibilityFromObject(Object obj)
        {
            if (obj == null)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object FindThemeResource(object key)
        {
            object value = TryFindResource(key);
            if (value != null)
            {
                return value;
            }

            if (Application.Current != null)
            {
                value = Application.Current.TryFindResource(key);
                if (value != null)
                {
                    return value;
                }
            }

            return null;
        }
    }
}
