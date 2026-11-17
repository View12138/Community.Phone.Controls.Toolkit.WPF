// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace Microsoft.Phone.Shell
{
    [ContentProperty("Buttons")]
    public sealed class ApplicationBar : IApplicationBar
    {
        internal static readonly int MaxIconButtons = 4;
        internal static readonly int MaxMenuItems = 50;
        private const double AppBarDefaultSize = 72.0;
        private const double AppBarMiniSize = 30.0;

        private readonly ApplicationBarItemList<IApplicationBarIconButton> _buttons;
        private readonly ApplicationBarItemList<IApplicationBarMenuItem> _menuItems;

        private bool _isVisible;
        private double _opacity;
        private bool _isMenuEnabled;
        private bool _reservesSpace;
        private Color _backgroundColor;
        private Color _foregroundColor;
        private ApplicationBarMode _mode;

        public ApplicationBar()
        {
            _isVisible = true;
            _opacity = 1.0;
            _isMenuEnabled = true;
            _reservesSpace = true;

            // The native bar takes its default colors from the shell theme, which
            // is not observable from managed code; the port uses the Metro chrome
            // colors so the bar is visible without being configured.
            _backgroundColor = Color.FromArgb(255, 31, 31, 31);
            _foregroundColor = Colors.White;
            if (Application.Current.FindResource("PhoneChromeColor") is Color backgroundColor)
            {
                _backgroundColor = backgroundColor;
            }
            if (Application.Current.FindResource("PhoneForegroundColor") is Color foregroundColor)
            {
                _foregroundColor = foregroundColor;
            }

            ThemeManager.ThemeChanged += (s, e) =>
            {
                if (Application.Current.FindResource("PhoneChromeColor") is Color backgroundColor)
                {
                    BackgroundColor = backgroundColor;
                }
                if (Application.Current.FindResource("PhoneForegroundColor") is Color foregroundColor)
                {
                    ForegroundColor = foregroundColor;
                }
            };

            _buttons = new ApplicationBarItemList<IApplicationBarIconButton>(ApplicationBarItemType.ImageButton);
            _buttons.MaxItems = MaxIconButtons;
            _buttons.Changed += OnItemsChanged;

            _menuItems = new ApplicationBarItemList<IApplicationBarMenuItem>(ApplicationBarItemType.TextItem);
            _menuItems.MaxItems = MaxMenuItems;
            _menuItems.Changed += OnItemsChanged;
        }

        public event EventHandler<ApplicationBarStateChangedEventArgs> StateChanged;

        internal event EventHandler Changed;

        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }

            set
            {
                if (value != _isVisible)
                {
                    _isVisible = value;
                    UpdateReservesSpace();
                    OnChanged();
                }
            }
        }

        public double Opacity
        {
            get
            {
                return _opacity;
            }

            set
            {
                if (value > 1.0)
                {
                    value = 1.0;
                }
                else if (value < 0.0)
                {
                    value = 0.0;
                }

                if (value != _opacity)
                {
                    _opacity = value;
                    UpdateReservesSpace();
                    OnChanged();
                }
            }
        }

        public bool IsMenuEnabled
        {
            get
            {
                return _isMenuEnabled;
            }

            set
            {
                if (value != _isMenuEnabled)
                {
                    _isMenuEnabled = value;
                    OnChanged();
                }
            }
        }

        public Color BackgroundColor
        {
            get
            {
                return _backgroundColor;
            }

            set
            {
                if (!value.Equals(_backgroundColor))
                {
                    _backgroundColor = value;
                    UpdateReservesSpace();
                    OnChanged();
                }
            }
        }

        public Color ForegroundColor
        {
            get
            {
                return _foregroundColor;
            }

            set
            {
                if (!value.Equals(_foregroundColor))
                {
                    _foregroundColor = value;
                    OnChanged();
                }
            }
        }

        public ApplicationBarMode Mode
        {
            get
            {
                return _mode;
            }

            set
            {
                if (!value.Equals(_mode))
                {
                    _mode = value;
                    OnChanged();
                }
            }
        }

        public double DefaultSize
        {
            get { return AppBarDefaultSize; }
        }

        public double MiniSize
        {
            get { return AppBarMiniSize; }
        }

        public IList Buttons
        {
            get { return _buttons; }
        }

        public IList MenuItems
        {
            get { return _menuItems; }
        }

        internal bool ReservesSpace
        {
            get { return _reservesSpace; }
        }

        internal double ReservedHeight
        {
            get
            {
                if (!_reservesSpace)
                {
                    return 0;
                }

                return _mode == ApplicationBarMode.Minimized ? AppBarMiniSize : AppBarDefaultSize;
            }
        }

        internal double BarHeight
        {
            get { return _mode == ApplicationBarMode.Minimized ? AppBarMiniSize : AppBarDefaultSize; }
        }

        internal ApplicationBarItemContainer GetButtonContainer(int index)
        {
            return _buttons.GetContainer(index);
        }

        internal ApplicationBarItemContainer GetMenuItemContainer(int index)
        {
            return _menuItems.GetContainer(index);
        }

        internal void OnEnterMenu()
        {
            EventHandler<ApplicationBarStateChangedEventArgs> handler = StateChanged;
            if (handler != null)
            {
                handler(this, new ApplicationBarStateChangedEventArgs(true));
            }
        }

        internal void OnExitMenu()
        {
            EventHandler<ApplicationBarStateChangedEventArgs> handler = StateChanged;
            if (handler != null)
            {
                handler(this, new ApplicationBarStateChangedEventArgs(false));
            }
        }

        private void OnItemsChanged(object sender, EventArgs e)
        {
            OnChanged();
        }

        private void UpdateReservesSpace()
        {
            _reservesSpace = _isVisible && _opacity >= 1.0 && (_backgroundColor.A >= byte.MaxValue || _backgroundColor.A == 0);
        }

        private void OnChanged()
        {
            EventHandler handler = Changed;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}
