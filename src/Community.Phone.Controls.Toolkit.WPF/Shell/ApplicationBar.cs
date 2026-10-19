// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Microsoft.Phone.Shell
{
    /// <summary>
    /// Represents the application bar displayed at the bottom of a phone page.
    /// </summary>
    public class ApplicationBar : DependencyObject
    {
        /// <summary>
        /// Identifies the IsVisible property.
        /// </summary>
        public static readonly DependencyProperty IsVisibleProperty =
            DependencyProperty.Register("IsVisible", typeof(bool), typeof(ApplicationBar), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the IsMenuEnabled property.
        /// </summary>
        public static readonly DependencyProperty IsMenuEnabledProperty =
            DependencyProperty.Register("IsMenuEnabled", typeof(bool), typeof(ApplicationBar), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the BackgroundColor property.
        /// </summary>
        public static readonly DependencyProperty BackgroundColorProperty =
            DependencyProperty.Register("BackgroundColor", typeof(Color), typeof(ApplicationBar), new PropertyMetadata(Color.FromArgb(255, 31, 31, 31)));

        /// <summary>
        /// Identifies the ForegroundColor property.
        /// </summary>
        public static readonly DependencyProperty ForegroundColorProperty =
            DependencyProperty.Register("ForegroundColor", typeof(Color), typeof(ApplicationBar), new PropertyMetadata(Colors.White));

        /// <summary>
        /// Identifies the Opacity property.
        /// </summary>
        public static readonly DependencyProperty OpacityProperty =
            DependencyProperty.Register("Opacity", typeof(double), typeof(ApplicationBar), new PropertyMetadata(1.0));

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationBar"/> class.
        /// </summary>
        public ApplicationBar()
        {
            Buttons = new ObservableCollection<ApplicationBarIconButton>();
            MenuItems = new ObservableCollection<ApplicationBarMenuItem>();
        }

        /// <summary>
        /// Gets or sets whether the application bar is visible.
        /// </summary>
        public bool IsVisible
        {
            get { return (bool)GetValue(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether the menu button is enabled.
        /// </summary>
        public bool IsMenuEnabled
        {
            get { return (bool)GetValue(IsMenuEnabledProperty); }
            set { SetValue(IsMenuEnabledProperty, value); }
        }

        /// <summary>
        /// Gets or sets the background color.
        /// </summary>
        public Color BackgroundColor
        {
            get { return (Color)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        /// <summary>
        /// Gets or sets the foreground color.
        /// </summary>
        public Color ForegroundColor
        {
            get { return (Color)GetValue(ForegroundColorProperty); }
            set { SetValue(ForegroundColorProperty, value); }
        }

        /// <summary>
        /// Gets or sets the opacity.
        /// </summary>
        public double Opacity
        {
            get { return (double)GetValue(OpacityProperty); }
            set { SetValue(OpacityProperty, value); }
        }

        /// <summary>
        /// Gets the icon buttons.
        /// </summary>
        public ObservableCollection<ApplicationBarIconButton> Buttons { get; private set; }

        /// <summary>
        /// Gets the menu items.
        /// </summary>
        public ObservableCollection<ApplicationBarMenuItem> MenuItems { get; private set; }
    }

    /// <summary>
    /// Base class for application bar items.
    /// </summary>
    public abstract class ApplicationBarItem : DependencyObject
    {
        /// <summary>
        /// Identifies the IsEnabled property.
        /// </summary>
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register("IsEnabled", typeof(bool), typeof(ApplicationBarItem), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the Text property.
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(ApplicationBarItem), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Gets or sets whether the item is enabled.
        /// </summary>
        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }

        /// <summary>
        /// Gets or sets the item text.
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
    }

    /// <summary>
    /// An icon button in the application bar.
    /// </summary>
    public class ApplicationBarIconButton : ApplicationBarItem
    {
        /// <summary>
        /// Identifies the IconUri property.
        /// </summary>
        public static readonly DependencyProperty IconUriProperty =
            DependencyProperty.Register("IconUri", typeof(Uri), typeof(ApplicationBarIconButton), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the Command property.
        /// </summary>
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(ApplicationBarIconButton), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the CommandParameter property.
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(ApplicationBarIconButton), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the icon URI.
        /// </summary>
        public Uri IconUri
        {
            get { return (Uri)GetValue(IconUriProperty); }
            set { SetValue(IconUriProperty, value); }
        }

        /// <summary>
        /// Gets or sets the command executed when the button is clicked.
        /// </summary>
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        /// <summary>
        /// Gets or sets the command parameter.
        /// </summary>
        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        /// <summary>
        /// Occurs when the button is clicked.
        /// </summary>
        public event EventHandler Click;

        /// <summary>
        /// Raises the <see cref="Click"/> event and executes the command.
        /// </summary>
        public void RaiseClick()
        {
            if (!IsEnabled)
            {
                return;
            }

            EventHandler handler = Click;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }

            ICommand command = Command;
            if (command != null && command.CanExecute(CommandParameter))
            {
                command.Execute(CommandParameter);
            }
        }
    }

    /// <summary>
    /// A menu item in the application bar.
    /// </summary>
    public class ApplicationBarMenuItem : ApplicationBarItem
    {
        /// <summary>
        /// Occurs when the item is clicked.
        /// </summary>
        public event EventHandler Click;

        /// <summary>
        /// Raises the <see cref="Click"/> event.
        /// </summary>
        public void RaiseClick()
        {
            if (!IsEnabled)
            {
                return;
            }

            EventHandler handler = Click;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}
