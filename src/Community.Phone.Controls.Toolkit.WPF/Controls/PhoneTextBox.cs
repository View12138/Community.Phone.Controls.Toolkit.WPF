// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// An extended TextBox with hint text, an action icon, and a length
    /// indicator.
    /// </summary>
    [TemplateVisualState(Name = "LengthIndicatorVisible", GroupName = "LengthIndicatorStates")]
    [TemplateVisualState(Name = "LengthIndicatorHidden", GroupName = "LengthIndicatorStates")]
    [TemplateVisualState(Name = "Normal", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Disabled", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "ReadOnly", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Focused", GroupName = "FocusStates")]
    [TemplateVisualState(Name = "Unfocused", GroupName = "FocusStates")]
    [TemplatePart(Name = "Text", Type = typeof(TextBox))]
    [TemplatePart(Name = "HintContent", Type = typeof(ContentControl))]
    [TemplatePart(Name = "LengthIndicator", Type = typeof(TextBlock))]
    public class PhoneTextBox : TextBox
    {
        private Grid _rootGrid;
        private TextBox _textBox;
        private TextBlock _measurementTextBlock;
        private Brush _foregroundBrushInactive;
        private Brush _foregroundBrushEdit;
        private ContentControl _hintContent;
        private Border _hintBorder;
        private TextBlock _lengthIndicator;
        private bool _ignorePropertyChange;
        private bool _ignoreFocus;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneTextBox"/> class.
        /// </summary>
        public PhoneTextBox()
        {
        }

        static PhoneTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(PhoneTextBox),
                new FrameworkPropertyMetadata(typeof(PhoneTextBox)));
        }

        /// <summary>
        /// Gets the border used for the action icon.
        /// </summary>
        protected Border ActionIconBorder { get; set; }

        /// <summary>
        /// Identifies the <see cref="Hint"/> property.
        /// </summary>
        public static readonly DependencyProperty HintProperty =
            DependencyProperty.Register("Hint", typeof(string), typeof(PhoneTextBox), new PropertyMetadata(null, OnHintPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="HintStyle"/> property.
        /// </summary>
        public static readonly DependencyProperty HintStyleProperty =
            DependencyProperty.Register("HintStyle", typeof(Style), typeof(PhoneTextBox), null);

        /// <summary>
        /// Identifies the <see cref="ActualHintVisibility"/> property.
        /// </summary>
        public static readonly DependencyProperty ActualHintVisibilityProperty =
            DependencyProperty.Register("ActualHintVisibility", typeof(Visibility), typeof(PhoneTextBox), null);

        /// <summary>
        /// Identifies the <see cref="LengthIndicatorVisible"/> property.
        /// </summary>
        public static readonly DependencyProperty LengthIndicatorVisibleProperty =
            DependencyProperty.Register("LengthIndicatorVisible", typeof(bool), typeof(PhoneTextBox), null);

        /// <summary>
        /// Identifies the <see cref="LengthIndicatorThreshold"/> property.
        /// </summary>
        public static readonly DependencyProperty LengthIndicatorThresholdProperty =
            DependencyProperty.Register("LengthIndicatorThreshold", typeof(int), typeof(PhoneTextBox), new PropertyMetadata(0, OnLengthIndicatorThresholdChanged));

        /// <summary>
        /// Identifies the <see cref="DisplayedMaxLength"/> property.
        /// </summary>
        public static readonly DependencyProperty DisplayedMaxLengthProperty =
            DependencyProperty.Register("DisplayedMaxLength", typeof(int), typeof(PhoneTextBox), new PropertyMetadata(0, OnDisplayedMaxLengthChanged));

        /// <summary>
        /// Identifies the <see cref="ActionIcon"/> property.
        /// </summary>
        public static readonly DependencyProperty ActionIconProperty =
            DependencyProperty.Register("ActionIcon", typeof(ImageSource), typeof(PhoneTextBox), new PropertyMetadata(null, OnActionIconChanged));

        /// <summary>
        /// Identifies the <see cref="HidesActionItemWhenEmpty"/> property.
        /// </summary>
        public static readonly DependencyProperty HidesActionItemWhenEmptyProperty =
            DependencyProperty.Register("HidesActionItemWhenEmpty", typeof(bool), typeof(PhoneTextBox), new PropertyMetadata(false, OnActionIconChanged));

        /// <summary>
        /// Occurs when the action icon is tapped.
        /// </summary>
        public event EventHandler ActionIconTapped;

        /// <summary>
        /// Gets or sets the hint text.
        /// </summary>
        public string Hint
        {
            get { return (string)GetValue(HintProperty); }
            set { SetValue(HintProperty, value); }
        }

        /// <summary>
        /// Gets or sets the hint style.
        /// </summary>
        public Style HintStyle
        {
            get { return (Style)GetValue(HintStyleProperty); }
            set { SetValue(HintStyleProperty, value); }
        }

        /// <summary>
        /// Gets the actual hint visibility.
        /// </summary>
        public Visibility ActualHintVisibility
        {
            get { return (Visibility)GetValue(ActualHintVisibilityProperty); }
            set { SetValue(ActualHintVisibilityProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether the length indicator is visible.
        /// </summary>
        public bool LengthIndicatorVisible
        {
            get { return (bool)GetValue(LengthIndicatorVisibleProperty); }
            set { SetValue(LengthIndicatorVisibleProperty, value); }
        }

        /// <summary>
        /// Gets or sets the threshold after which the length indicator appears.
        /// </summary>
        public int LengthIndicatorThreshold
        {
            get { return (int)GetValue(LengthIndicatorThresholdProperty); }
            set { SetValue(LengthIndicatorThresholdProperty, value); }
        }

        /// <summary>
        /// Gets or sets the displayed maximum length.
        /// </summary>
        public int DisplayedMaxLength
        {
            get { return (int)GetValue(DisplayedMaxLengthProperty); }
            set { SetValue(DisplayedMaxLengthProperty, value); }
        }

        /// <summary>
        /// Gets or sets the action icon.
        /// </summary>
        public ImageSource ActionIcon
        {
            get { return (ImageSource)GetValue(ActionIconProperty); }
            set { SetValue(ActionIconProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether the action item is hidden when the text is empty.
        /// </summary>
        public bool HidesActionItemWhenEmpty
        {
            get { return (bool)GetValue(HidesActionItemWhenEmptyProperty); }
            set { SetValue(HidesActionItemWhenEmptyProperty, value); }
        }

        /// <inheritdoc />
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            UpdateHintVisibility();
            base.OnLostFocus(e);
        }

        /// <inheritdoc />
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            if (_ignoreFocus)
            {
                _ignoreFocus = false;
                return;
            }

            if (_textBox != null && _textBox.IsEnabled && !_textBox.IsKeyboardFocusWithin)
            {
                _textBox.Focus();
            }

            if (_foregroundBrushEdit != null)
            {
                Foreground = _foregroundBrushEdit;
            }

            if (_hintContent != null)
            {
                ActualHintVisibility = Visibility.Collapsed;
            }

            base.OnGotFocus(e);
        }

        /// <inheritdoc />
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            if (_textBox != null && _textBox.IsEnabled && !_textBox.IsKeyboardFocusWithin)
            {
                _textBox.Focus();
            }
        }

        /// <inheritdoc />
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_textBox != null)
            {
                _textBox.TextChanged -= OnTextChanged;
            }

            if (ActionIconBorder != null)
            {
                ActionIconBorder.MouseLeftButtonDown -= OnActionIconTapped;
            }

            _rootGrid = GetTemplateChild("RootGrid") as Grid;
            _textBox = GetTemplateChild("Text") as TextBox;
            _foregroundBrushEdit = Foreground;
            _foregroundBrushInactive = TryFindResource("PhoneTextBoxReadOnlyBrush") as Brush ?? Foreground;
            _hintContent = GetTemplateChild("HintContent") as ContentControl;
            _hintBorder = GetTemplateChild("HintBorder") as Border;
            _lengthIndicator = GetTemplateChild("LengthIndicator") as TextBlock;
            ActionIconBorder = GetTemplateChild("ActionIconBorder") as Border;
            _measurementTextBlock = GetTemplateChild("MeasurementTextBlock") as TextBlock;

            if (_hintContent != null)
            {
                UpdateHintVisibility();
            }

            if (_rootGrid != null && _lengthIndicator != null)
            {
                UpdateLengthIndicatorVisibility();
            }

            if (_textBox != null)
            {
                _textBox.TextChanged += OnTextChanged;
            }

            if (ActionIconBorder != null)
            {
                ActionIconBorder.MouseLeftButtonDown += OnActionIconTapped;
                UpdateActionIconVisibility();
            }
        }

        private static void OnHintPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            PhoneTextBox phoneTextBox = sender as PhoneTextBox;
            if (phoneTextBox != null && phoneTextBox._hintContent != null)
            {
                phoneTextBox.UpdateHintVisibility();
            }
        }

        private void UpdateHintVisibility()
        {
            if (_hintContent == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(Text))
            {
                ActualHintVisibility = Visibility.Visible;
                if (_foregroundBrushInactive != null)
                {
                    Foreground = _foregroundBrushInactive;
                }
            }
            else
            {
                ActualHintVisibility = Visibility.Collapsed;
                if (_foregroundBrushEdit != null)
                {
                    Foreground = _foregroundBrushEdit;
                }
            }
        }

        private static void OnLengthIndicatorThresholdChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            PhoneTextBox phoneTextBox = sender as PhoneTextBox;
            if (phoneTextBox == null)
            {
                return;
            }

            if (phoneTextBox._ignorePropertyChange)
            {
                phoneTextBox._ignorePropertyChange = false;
                return;
            }

            if (phoneTextBox.LengthIndicatorThreshold < 0)
            {
                phoneTextBox._ignorePropertyChange = true;
                phoneTextBox.SetValue(LengthIndicatorThresholdProperty, args.OldValue);
                throw new ArgumentOutOfRangeException("LengthIndicatorThreshold", "The length indicator visibility threshold must be greater than or equal to zero.");
            }
        }

        private static void OnDisplayedMaxLengthChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            PhoneTextBox phoneTextBox = sender as PhoneTextBox;
            if (phoneTextBox != null && phoneTextBox.DisplayedMaxLength > phoneTextBox.MaxLength && phoneTextBox.MaxLength > 0)
            {
                throw new ArgumentOutOfRangeException("DisplayedMaxLength", "The displayed maximum length cannot be greater than MaxLength.");
            }
        }

        private void UpdateLengthIndicatorVisibility()
        {
            if (_rootGrid == null || _lengthIndicator == null)
            {
                return;
            }

            bool isHidden = true;
            if (LengthIndicatorVisible)
            {
                _lengthIndicator.Text = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}/{1}",
                    Text.Length,
                    DisplayedMaxLength > 0 ? DisplayedMaxLength : MaxLength);

                if (Text.Length >= LengthIndicatorThreshold)
                {
                    isHidden = false;
                }
            }

            VisualStateManager.GoToState(this, isHidden ? "LengthIndicatorHidden" : "LengthIndicatorVisible", false);
        }

        private static void OnActionIconChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            PhoneTextBox phoneTextBox = sender as PhoneTextBox;
            if (phoneTextBox != null)
            {
                phoneTextBox.UpdateActionIconVisibility();
            }
        }

        private void UpdateActionIconVisibility()
        {
            if (ActionIconBorder == null)
            {
                return;
            }

            bool hide = ActionIcon == null || (HidesActionItemWhenEmpty && string.IsNullOrEmpty(Text));
            if (hide)
            {
                ActionIconBorder.Visibility = Visibility.Collapsed;
                if (_hintBorder != null)
                {
                    _hintBorder.Padding = new Thickness(0);
                }
            }
            else
            {
                ActionIconBorder.Visibility = Visibility.Visible;
                if (_hintBorder != null && TextWrapping != TextWrapping.Wrap)
                {
                    _hintBorder.Padding = new Thickness(0, 0, 48, 0);
                }
            }
        }

        private void OnActionIconTapped(object sender, RoutedEventArgs e)
        {
            _ignoreFocus = true;
            EventHandler handler = ActionIconTapped;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnTextChanged(object sender, RoutedEventArgs e)
        {
            UpdateLengthIndicatorVisibility();
            UpdateActionIconVisibility();
            UpdateHintVisibility();
            ResizeTextBox();
        }

        private void ResizeTextBox()
        {
            if (ActionIcon == null || TextWrapping != TextWrapping.Wrap || _measurementTextBlock == null)
            {
                return;
            }

            _measurementTextBlock.Width = ActualWidth;

            if (_measurementTextBlock.ActualHeight > ActualHeight - 72)
            {
                Height = ActualHeight + 72;
            }
            else if (ActualHeight > _measurementTextBlock.ActualHeight + 144)
            {
                Height = ActualHeight - 72;
            }
        }
    }
}
