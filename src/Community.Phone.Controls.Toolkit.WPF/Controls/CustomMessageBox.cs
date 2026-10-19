// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents a popup dialog with one or two buttons.
    /// </summary>
    public class CustomMessageBox : ContentControl
    {
        private static CustomMessageBox _currentInstance;

        private Window _window;
        private UIElement _content;
        private bool _closeRequested;
        private CustomMessageBoxResult _result = CustomMessageBoxResult.None;
        private bool _isDismissing;

        /// <summary>
        /// Identifies the <see cref="Title"/> property.
        /// </summary>
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(CustomMessageBox), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="Caption"/> property.
        /// </summary>
        public static readonly DependencyProperty CaptionProperty =
            DependencyProperty.Register("Caption", typeof(string), typeof(CustomMessageBox), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="Message"/> property.
        /// </summary>
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(CustomMessageBox), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="LeftButtonContent"/> property.
        /// </summary>
        public static readonly DependencyProperty LeftButtonContentProperty =
            DependencyProperty.Register("LeftButtonContent", typeof(object), typeof(CustomMessageBox), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="RightButtonContent"/> property.
        /// </summary>
        public static readonly DependencyProperty RightButtonContentProperty =
            DependencyProperty.Register("RightButtonContent", typeof(object), typeof(CustomMessageBox), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="IsLeftButtonEnabled"/> property.
        /// </summary>
        public static readonly DependencyProperty IsLeftButtonEnabledProperty =
            DependencyProperty.Register("IsLeftButtonEnabled", typeof(bool), typeof(CustomMessageBox), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="IsRightButtonEnabled"/> property.
        /// </summary>
        public static readonly DependencyProperty IsRightButtonEnabledProperty =
            DependencyProperty.Register("IsRightButtonEnabled", typeof(bool), typeof(CustomMessageBox), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="IsFullScreen"/> property.
        /// </summary>
        public static readonly DependencyProperty IsFullScreenProperty =
            DependencyProperty.Register("IsFullScreen", typeof(bool), typeof(CustomMessageBox), new PropertyMetadata(false));

        /// <summary>
        /// Occurs when the message box is about to be dismissed.
        /// </summary>
        public event EventHandler<DismissingEventArgs> Dismissing;

        /// <summary>
        /// Occurs when the message box is dismissed.
        /// </summary>
        public event EventHandler<DismissedEventArgs> Dismissed;

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        /// <summary>
        /// Gets or sets the caption.
        /// </summary>
        public string Caption
        {
            get { return (string)GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        /// <summary>
        /// Gets or sets the left button content.
        /// </summary>
        public object LeftButtonContent
        {
            get { return GetValue(LeftButtonContentProperty); }
            set { SetValue(LeftButtonContentProperty, value); }
        }

        /// <summary>
        /// Gets or sets the right button content.
        /// </summary>
        public object RightButtonContent
        {
            get { return GetValue(RightButtonContentProperty); }
            set { SetValue(RightButtonContentProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether the left button is enabled.
        /// </summary>
        public bool IsLeftButtonEnabled
        {
            get { return (bool)GetValue(IsLeftButtonEnabledProperty); }
            set { SetValue(IsLeftButtonEnabledProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether the right button is enabled.
        /// </summary>
        public bool IsRightButtonEnabled
        {
            get { return (bool)GetValue(IsRightButtonEnabledProperty); }
            set { SetValue(IsRightButtonEnabledProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether the message box occupies the full window.
        /// </summary>
        public bool IsFullScreen
        {
            get { return (bool)GetValue(IsFullScreenProperty); }
            set { SetValue(IsFullScreenProperty, value); }
        }

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
            _content = BuildContent();
            ProjectionHost host = new ProjectionHost { Child = _content };

            Window window = new Window
            {
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = (Brush)(FindThemeResource("PhoneBackgroundBrush") ?? new SolidColorBrush(Color.FromRgb(31, 31, 31))),
                Content = host
            };

            Window owner = Application.Current == null ? null : Application.Current.MainWindow;
            if (owner != null && !ReferenceEquals(owner, window))
            {
                window.Owner = owner;
                if (IsFullScreen)
                {
                    window.Width = Math.Max(320, owner.ActualWidth);
                    window.Height = Math.Max(240, owner.ActualHeight);
                }
                else
                {
                    window.Width = Math.Min(480, Math.Max(320, owner.ActualWidth * 0.9));
                    window.Height = double.NaN;
                    window.SizeToContent = SizeToContent.Height;
                }
            }
            else
            {
                window.Width = IsFullScreen ? 480 : 420;
                window.SizeToContent = IsFullScreen ? SizeToContent.Manual : SizeToContent.Height;
                window.Height = IsFullScreen ? 640 : double.NaN;
            }

            window.Closed += OnWindowClosed;
            window.ContentRendered += OnWindowContentRendered;
            _window = window;
            window.Show();
        }

        private void OnWindowContentRendered(object sender, EventArgs e)
        {
            Window window = sender as Window;
            if (window != null)
            {
                window.ContentRendered -= OnWindowContentRendered;
            }

            // Windows Phone swivels the message box in with a backward swivel.
            ITransition transition = Transitions.Swivel(_content, SwivelTransitionMode.BackwardIn);
            ProjectionHost host = ProjectionService.GetProjectionHost(_content);
            if (host != null)
            {
                host.Prefer3D = true;
            }

            transition.Begin();
        }

        private void CloseWindow()
        {
            if (_closeRequested)
            {
                return;
            }

            _closeRequested = true;

            Window window = _window;
            if (window != null)
            {
                window.Close();
            }
        }

        /// <summary>
        /// Dismisses the message box.
        /// </summary>
        public void Dismiss()
        {
            Dismiss(CustomMessageBoxResult.None);
        }

        private void Dismiss(CustomMessageBoxResult result)
        {
            if (_window == null || _isDismissing)
            {
                return;
            }

            DismissingEventArgs args = new DismissingEventArgs(result);
            EventHandler<DismissingEventArgs> handler = Dismissing;
            if (handler != null)
            {
                handler(this, args);
            }

            if (args.Cancel)
            {
                return;
            }

            _isDismissing = true;
            _result = result;

            if (_window == null || _content == null || !_window.IsVisible)
            {
                CloseWindow();
                return;
            }

            // Swivel out before closing, matching the Windows Phone message
            // box. The timer is a safety net in case the window stops
            // rendering (it would never complete the storyboard).
            ITransition transition = Transitions.Swivel(_content, SwivelTransitionMode.BackwardOut);
            transition.Completed += delegate
            {
                transition.Stop();
                CloseWindow();
            };
            transition.Begin();

            DispatcherTimer timer = new DispatcherTimer(
                TimeSpan.FromMilliseconds(500),
                DispatcherPriority.Normal,
                delegate
                {
                    CloseWindow();
                },
                _window.Dispatcher);
            timer.Start();
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            Window window = _window;
            _window = null;
            _content = null;
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

        private UIElement BuildContent()
        {
            Grid root = new Grid
            {
                Margin = new Thickness(24),
                Background = Brushes.Transparent
            };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            StackPanel header = new StackPanel();
            if (!string.IsNullOrEmpty(Title))
            {
                header.Children.Add(new TextBlock
                {
                    Text = Title,
                    FontSize = 32,
                    FontFamily = new FontFamily("Segoe UI Light"),
                    Foreground = Brushes.White,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 4)
                });
            }

            if (!string.IsNullOrEmpty(Caption))
            {
                header.Children.Add(new TextBlock
                {
                    Text = Caption.ToLowerInvariant(),
                    FontSize = 19,
                    Foreground = new SolidColorBrush(Color.FromRgb(166, 166, 166)),
                    TextWrapping = TextWrapping.Wrap
                });
            }

            Grid.SetRow(header, 0);
            root.Children.Add(header);

            if (!string.IsNullOrEmpty(Message))
            {
                TextBlock message = new TextBlock
                {
                    Text = Message,
                    FontSize = 15,
                    Foreground = Brushes.White,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 12, 0, 12)
                };
                Grid.SetRow(message, 1);
                root.Children.Add(message);
            }

            if (Content is UIElement)
            {
                FrameworkElement content = (FrameworkElement)Content;
                content.Margin = new Thickness(0, 12, 0, 12);
                Grid.SetRow(content, 2);
                root.Children.Add(content);
            }

            Grid buttons = new Grid { Margin = new Thickness(0, 12, 0, 0) };
            buttons.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            buttons.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            Grid.SetRow(buttons, 3);

            if (LeftButtonContent != null)
            {
                Button left = CreateButton(LeftButtonContent, IsLeftButtonEnabled, CustomMessageBoxResult.LeftButton);
                left.Margin = new Thickness(0, 0, 6, 0);
                Grid.SetColumn(left, 0);
                buttons.Children.Add(left);
            }

            if (RightButtonContent != null)
            {
                Button right = CreateButton(RightButtonContent, IsRightButtonEnabled, CustomMessageBoxResult.RightButton);
                right.Margin = new Thickness(6, 0, 0, 0);
                Grid.SetColumn(right, 1);
                buttons.Children.Add(right);
            }

            root.Children.Add(buttons);
            return root;
        }

        private Button CreateButton(object content, bool isEnabled, CustomMessageBoxResult result)
        {
            Button button = new Button
            {
                Content = content,
                IsEnabled = isEnabled,
                Padding = new Thickness(12, 8, 12, 8),
                Background = new SolidColorBrush(Color.FromRgb(31, 31, 31)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(166, 166, 166)),
                BorderThickness = new Thickness(2),
                FontFamily = new FontFamily("Segoe UI")
            };

            button.Click += delegate { Dismiss(result); };
            return button;
        }

        private object FindThemeResource(object key)
        {
            if (Application.Current != null)
            {
                object value = Application.Current.TryFindResource(key);
                if (value != null)
                {
                    return value;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Describes the result of dismissing a <see cref="CustomMessageBox"/>.
    /// </summary>
    public enum CustomMessageBoxResult
    {
        /// <summary>The left button produced the result.</summary>
        LeftButton,
        /// <summary>The right button produced the result.</summary>
        RightButton,
        /// <summary>The message box has no result.</summary>
        None
    }

    /// <summary>
    /// Provides data for the <see cref="CustomMessageBox.Dismissing"/> event.
    /// </summary>
    public class DismissingEventArgs : CancelEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DismissingEventArgs"/> class.
        /// </summary>
        public DismissingEventArgs(CustomMessageBoxResult result)
        {
            Result = result;
        }

        /// <summary>
        /// Gets the result value.
        /// </summary>
        public CustomMessageBoxResult Result { get; private set; }
    }

    /// <summary>
    /// Provides data for the <see cref="CustomMessageBox.Dismissed"/> event.
    /// </summary>
    public class DismissedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DismissedEventArgs"/> class.
        /// </summary>
        public DismissedEventArgs(CustomMessageBoxResult result)
        {
            Result = result;
        }

        /// <summary>
        /// Gets the result value.
        /// </summary>
        public CustomMessageBoxResult Result { get; private set; }
    }
}
