// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Microsoft.Phone.Shell
{
    /// <summary>
    /// The shared visual for one application bar item: handles the hover and
    /// keyboard highlight, the enabled state and the click, and leaves the
    /// content to the derived view.
    /// </summary>
    internal abstract class ApplicationBarItemView : Border
    {
        private static readonly Brush TransparentBrush = CreateFrozenBrush(Color.FromArgb(0, 0, 0, 0));
        private static readonly Brush HighlightBrush = CreateFrozenBrush(Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF));

        private Brush _foregroundBrush = Brushes.White;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationBarItemView"/> class.
        /// </summary>
        /// <param name="container">The container that tracks the item.</param>
        protected ApplicationBarItemView(ApplicationBarItemContainer container)
        {
            Container = container;

            Background = TransparentBrush;
            Cursor = Cursors.Hand;
            Focusable = true;
            SnapsToDevicePixels = true;

            MouseEnter += OnMouseEnter;
            MouseLeave += OnMouseLeave;
            MouseLeftButtonUp += OnMouseLeftButtonUp;
            KeyDown += OnKeyDown;
            GotKeyboardFocus += OnGotKeyboardFocus;
            LostKeyboardFocus += OnLostKeyboardFocus;

            Refresh();
        }

        /// <summary>
        /// Occurs after the item was activated by the user.
        /// </summary>
        internal event EventHandler Activated;

        /// <summary>
        /// Gets the container that tracks the item.
        /// </summary>
        internal ApplicationBarItemContainer Container { get; }

        /// <summary>
        /// Gets or sets the brush used for the item text.
        /// </summary>
        internal Brush ForegroundBrush
        {
            get
            {
                return _foregroundBrush;
            }

            set
            {
                if (!Equals(_foregroundBrush, value))
                {
                    _foregroundBrush = value;
                    UpdateContent();
                }
            }
        }

        /// <summary>
        /// Applies the current container state to the view.
        /// </summary>
        internal virtual void Refresh()
        {
            IsEnabled = Container.IsEnabled;
            Opacity = Container.IsEnabled ? 1.0 : 0.4;
            AutomationProperties.SetName(this, Container.Text ?? string.Empty);
            UpdateContent();
        }

        /// <summary>
        /// Rebuilds the content from the container state.
        /// </summary>
        protected abstract void UpdateContent();

        /// <summary>
        /// Called after the item was activated by the user.
        /// </summary>
        protected virtual void OnActivated()
        {
        }

        /// <summary>
        /// Creates a frozen brush, so it can be shared across threads.
        /// </summary>
        /// <param name="color">The brush color.</param>
        /// <returns>The frozen brush.</returns>
        protected static Brush CreateFrozenBrush(Color color)
        {
            SolidColorBrush brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        /// <summary>
        /// Activates the item, as a mouse click or the keyboard would.
        /// </summary>
        internal void Activate()
        {
            if (!Container.IsEnabled)
            {
                return;
            }

            Container.ClickEvent();
            OnActivated();

            EventHandler handler = Activated;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            Background = HighlightBrush;
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            Background = TransparentBrush;
        }

        private void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Background = HighlightBrush;
        }

        private void OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Background = TransparentBrush;
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Activate();
            e.Handled = true;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                Activate();
                e.Handled = true;
            }
        }
    }
}
