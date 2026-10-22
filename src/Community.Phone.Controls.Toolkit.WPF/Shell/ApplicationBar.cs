// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections;
using System.Windows.Markup;
using System.Windows.Media;

namespace Microsoft.Phone.Shell
{
    /// <summary>
    /// Represents the application bar displayed at the bottom of a phone page.
    /// The API follows the original <c>Microsoft.Phone.Shell.ApplicationBar</c>;
    /// in the WPF port the bar is drawn by an
    /// <see cref="ApplicationBarPresenter"/> instead of the native compositor.
    /// </summary>
    [ContentProperty("Buttons")]
    public sealed class ApplicationBar : IApplicationBar
    {
        /// <summary>
        /// The maximum number of icon buttons an application bar accepts.
        /// </summary>
        internal static readonly int MaxIconButtons = 4;

        /// <summary>
        /// The maximum number of menu items an application bar accepts.
        /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationBar"/> class.
        /// </summary>
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

            _buttons = new ApplicationBarItemList<IApplicationBarIconButton>(ApplicationBarItemType.ImageButton);
            _buttons.MaxItems = MaxIconButtons;
            _buttons.Changed += OnItemsChanged;

            _menuItems = new ApplicationBarItemList<IApplicationBarMenuItem>(ApplicationBarItemType.TextItem);
            _menuItems.MaxItems = MaxMenuItems;
            _menuItems.Changed += OnItemsChanged;
        }

        /// <summary>
        /// Occurs when the application bar menu opens or closes.
        /// </summary>
        public event EventHandler<ApplicationBarStateChangedEventArgs> StateChanged;

        /// <summary>
        /// Occurs when a property of the bar or of one of its items changed.
        /// </summary>
        internal event EventHandler Changed;

        /// <summary>
        /// Gets or sets a value indicating whether the application bar is visible.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the opacity of the application bar. Values outside the
        /// range 0..1 are clamped, like the original.
        /// </summary>
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

        /// <summary>
        /// Gets or sets a value indicating whether the menu is available.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the background color of the application bar.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the foreground color of the application bar.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the display mode of the application bar.
        /// </summary>
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

        /// <summary>
        /// Gets the height of the application bar in the default mode.
        /// </summary>
        public double DefaultSize
        {
            get { return AppBarDefaultSize; }
        }

        /// <summary>
        /// Gets the height of the application bar in the minimized mode.
        /// </summary>
        public double MiniSize
        {
            get { return AppBarMiniSize; }
        }

        /// <summary>
        /// Gets the icon buttons shown by the application bar.
        /// </summary>
        public IList Buttons
        {
            get { return _buttons; }
        }

        /// <summary>
        /// Gets the menu items shown by the application bar.
        /// </summary>
        public IList MenuItems
        {
            get { return _menuItems; }
        }

        /// <summary>
        /// Gets a value indicating whether the page layout reserves space for
        /// the bar. Mirrors the original rule: an invisible, translucent or
        /// semi-transparent bar floats over the page instead of reserving room.
        /// </summary>
        internal bool ReservesSpace
        {
            get { return _reservesSpace; }
        }

        /// <summary>
        /// Gets the height the bar occupies in the page layout.
        /// </summary>
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

        /// <summary>
        /// Gets the height of the bar itself.
        /// </summary>
        internal double BarHeight
        {
            get { return _mode == ApplicationBarMode.Minimized ? AppBarMiniSize : AppBarDefaultSize; }
        }

        /// <summary>
        /// Gets the container that renders the icon button at the given index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The container.</returns>
        internal ApplicationBarItemContainer GetButtonContainer(int index)
        {
            return _buttons.GetContainer(index);
        }

        /// <summary>
        /// Gets the container that renders the menu item at the given index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The container.</returns>
        internal ApplicationBarItemContainer GetMenuItemContainer(int index)
        {
            return _menuItems.GetContainer(index);
        }

        /// <summary>
        /// Raises <see cref="StateChanged"/> for an opened menu.
        /// </summary>
        internal void OnEnterMenu()
        {
            EventHandler<ApplicationBarStateChangedEventArgs> handler = StateChanged;
            if (handler != null)
            {
                handler(this, new ApplicationBarStateChangedEventArgs(true));
            }
        }

        /// <summary>
        /// Raises <see cref="StateChanged"/> for a closed menu.
        /// </summary>
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
