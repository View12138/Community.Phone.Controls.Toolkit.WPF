// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Navigation;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents a button control that navigates to a URI when it is clicked.
    /// </summary>
    [TemplateVisualState(Name = "MouseOver", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Focused", GroupName = "FocusStates")]
    [TemplateVisualState(Name = "Normal", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Pressed", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Disabled", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Unfocused", GroupName = "FocusStates")]
    public class HyperlinkButton : ButtonBase
    {
        /// <summary>
        /// Identifies the <see cref="NavigateUri"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty NavigateUriProperty =
            DependencyProperty.Register(
                nameof(NavigateUri),
                typeof(Uri),
                typeof(HyperlinkButton),
                new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="TargetName"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TargetNameProperty =
            DependencyProperty.Register(
                nameof(TargetName),
                typeof(string),
                typeof(HyperlinkButton),
                new PropertyMetadata(null));

        /// <summary>
        /// Initializes a new instance of the HyperlinkButton class.
        /// </summary>
        public HyperlinkButton()
        {
            IsEnabledChanged += (sender, e) => UpdateVisualState();
        }

        /// <summary>
        /// Gets or sets the URI that the button navigates to when it is clicked.
        /// </summary>
        public Uri NavigateUri
        {
            get => (Uri)GetValue(NavigateUriProperty);
            set => SetValue(NavigateUriProperty, value);
        }

        /// <summary>
        /// Gets or sets the name of the target window or frame.
        /// </summary>
        /// <remarks>
        /// WPF has no named navigation targets. The value is retained for API
        /// compatibility; in this port relative URIs use the nearest frame and
        /// external URIs are opened by the shell.
        /// </remarks>
        public string TargetName
        {
            get => (string)GetValue(TargetNameProperty);
            set => SetValue(TargetNameProperty, value);
        }

        /// <summary>
        /// Called when the button is clicked; navigates to
        /// <see cref="NavigateUri"/> after the Click event has been raised.
        /// </summary>
        protected override void OnClick()
        {
            base.OnClick();
            NavigateToUri();
        }

        /// <summary>
        /// Activates the link with Enter or Space, like the phone control.
        /// </summary>
        /// <param name="e">The key event data.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Handled)
            {
                return;
            }

            if (e.Key == Key.Enter)
            {
                OnClick();
                e.Handled = true;
            }
            else if (e.Key == Key.Space)
            {
                IsPressed = true;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Completes Space activation on key release.
        /// </summary>
        /// <param name="e">The key event data.</param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (e.Handled)
            {
                return;
            }

            if (e.Key == Key.Space && IsPressed)
            {
                IsPressed = false;
                OnClick();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Updates the CommonStates visual state for the template.
        /// </summary>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            UpdateVisualState();
        }

        /// <summary>
        /// Updates the CommonStates visual state for the template.
        /// </summary>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            UpdateVisualState();
        }

        /// <summary>
        /// Updates the CommonStates visual state for the template.
        /// </summary>
        protected override void OnIsPressedChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsPressedChanged(e);
            UpdateVisualState();
        }

        /// <summary>
        /// Creates the automation peer for the link.
        /// </summary>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new HyperlinkButtonAutomationPeer(this);
        }

        /// <summary>
        /// Opens an external URI. The WPF port uses the default shell handler
        /// (a browser for http/https, the mail client for mailto, and so on).
        /// </summary>
        /// <param name="uri">The absolute URI to open.</param>
        protected virtual void OpenExternalUri(Uri uri)
        {
            if (uri == null || !uri.IsAbsoluteUri)
            {
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo(uri.AbsoluteUri)
                {
                    UseShellExecute = true,
                });
            }
            catch (Win32Exception)
            {
                // No handler is registered for the scheme.
            }
            catch (InvalidOperationException)
            {
                // The shell could not start the process.
            }
        }

        /// <summary>
        /// Raises Click and performs the navigation, as a pointer or keyboard
        /// activation would.
        /// </summary>
        internal void PerformClick()
        {
            OnClick();
        }

        /// <summary>
        /// Performs the navigation for the current <see cref="NavigateUri"/>.
        /// Exposed internally so the navigation decision can be tested without
        /// a real pointer.
        /// </summary>
        internal void NavigateToUri()
        {
            Uri uri = NavigateUri;
            if (uri == null)
            {
                return;
            }

            if (uri.IsAbsoluteUri && !IsApplicationUri(uri))
            {
                OpenExternalUri(uri);
                return;
            }

            NavigationService navigationService = NavigationService.GetNavigationService(this);
            if (navigationService != null)
            {
                navigationService.Navigate(uri);
                return;
            }

            Frame frame = FindAncestor<Frame>(this);
            if (frame != null)
            {
                frame.Navigate(uri);
                return;
            }

            if (uri.IsAbsoluteUri)
            {
                OpenExternalUri(uri);
            }
        }

        private static bool IsApplicationUri(Uri uri)
        {
            // Relative URIs are resolved in the application frame. The only
            // absolute application scheme WPF uses is the pack URI scheme.
            return string.Equals(uri.Scheme, "pack", StringComparison.OrdinalIgnoreCase);
        }

        private static T FindAncestor<T>(DependencyObject element)
            where T : DependencyObject
        {
            DependencyObject current = element;
            while (current != null)
            {
                if (current is T match)
                {
                    return match;
                }

                current = current is FrameworkElement frameworkElement
                    ? frameworkElement.Parent
                    : null;
            }

            return null;
        }

        private void UpdateVisualState()
        {
            string state;
            if (!IsEnabled)
            {
                state = "Disabled";
            }
            else if (IsPressed)
            {
                state = "Pressed";
            }
            else if (IsMouseOver)
            {
                state = "MouseOver";
            }
            else
            {
                state = "Normal";
            }

            VisualStateManager.GoToState(this, state, true);
        }
    }

    /// <summary>
    /// Announces a <see cref="HyperlinkButton"/> as a hyperlink instead of a
    /// generic button.
    /// </summary>
    internal sealed class HyperlinkButtonAutomationPeer : ButtonBaseAutomationPeer, IInvokeProvider
    {
        public HyperlinkButtonAutomationPeer(HyperlinkButton owner)
            : base(owner)
        {
        }

        public void Invoke()
        {
            if (!IsEnabled())
            {
                throw new ElementNotEnabledException();
            }

            ((HyperlinkButton)Owner).PerformClick();
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            return patternInterface == PatternInterface.Invoke
                ? this
                : base.GetPattern(patternInterface);
        }

        protected override string GetClassNameCore()
        {
            return "HyperlinkButton";
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Hyperlink;
        }
    }
}
