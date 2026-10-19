// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Microsoft.Phone.Controls.LocalizedResources;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// A picker control that displays a list of items in a popup or full
    /// screen window.
    /// </summary>
    [TemplatePart(Name = "PickerButton", Type = typeof(Button))]
    [TemplatePart(Name = "PickerPopup", Type = typeof(Popup))]
    [TemplatePart(Name = "PickerListBox", Type = typeof(ListBox))]
    [TemplateVisualState(Name = "Normal", GroupName = "PickerStates")]
    [TemplateVisualState(Name = "Highlighted", GroupName = "PickerStates")]
    [TemplateVisualState(Name = "Disabled", GroupName = "PickerStates")]
    public class ListPicker : ItemsControl
    {
        private Button _pickerButton;
        private Popup _pickerPopup;
        private ListBox _pickerListBox;
        private bool _isUpdatingSelection;
        private Window _fullModeWindow;

        /// <summary>
        /// Identifies the <see cref="SummaryForSelectedItemsDelegate"/> property.
        /// </summary>
        public static readonly DependencyProperty SummaryForSelectedItemsDelegateProperty =
            DependencyProperty.Register("SummaryForSelectedItemsDelegate", typeof(Func<IList, string>), typeof(ListPicker), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="ListPickerMode"/> property.
        /// </summary>
        public static readonly DependencyProperty ListPickerModeProperty =
            DependencyProperty.Register("ListPickerMode", typeof(ListPickerMode), typeof(ListPicker), new PropertyMetadata(ListPickerMode.Normal, OnListPickerModeChanged));

        /// <summary>
        /// Identifies the <see cref="SelectedIndex"/> property.
        /// </summary>
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(ListPicker), new PropertyMetadata(-1, OnSelectedIndexChanged));

        /// <summary>
        /// Identifies the <see cref="SelectedItem"/> property.
        /// </summary>
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(ListPicker), new PropertyMetadata(null, OnSelectedItemChanged));

        /// <summary>
        /// Identifies the <see cref="FullModeItemTemplate"/> property.
        /// </summary>
        public static readonly DependencyProperty FullModeItemTemplateProperty =
            DependencyProperty.Register("FullModeItemTemplate", typeof(DataTemplate), typeof(ListPicker), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="Header"/> property.
        /// </summary>
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(object), typeof(ListPicker), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="HeaderTemplate"/> property.
        /// </summary>
        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.Register("HeaderTemplate", typeof(DataTemplate), typeof(ListPicker), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="FullModeHeader"/> property.
        /// </summary>
        public static readonly DependencyProperty FullModeHeaderProperty =
            DependencyProperty.Register("FullModeHeader", typeof(object), typeof(ListPicker), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="ItemCountThreshold"/> property.
        /// </summary>
        public static readonly DependencyProperty ItemCountThresholdProperty =
            DependencyProperty.Register("ItemCountThreshold", typeof(int), typeof(ListPicker), new PropertyMetadata(5));

        /// <summary>
        /// Identifies the <see cref="PickerPageUri"/> property.
        /// </summary>
        public static readonly DependencyProperty PickerPageUriProperty =
            DependencyProperty.Register("PickerPageUri", typeof(Uri), typeof(ListPicker), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="ExpansionMode"/> property.
        /// </summary>
        public static readonly DependencyProperty ExpansionModeProperty =
            DependencyProperty.Register("ExpansionMode", typeof(ExpansionMode), typeof(ListPicker), new PropertyMetadata(ExpansionMode.ExpansionAllowed));

        /// <summary>
        /// Identifies the <see cref="SelectionMode"/> property.
        /// </summary>
        public static readonly DependencyProperty SelectionModeProperty =
            DependencyProperty.Register("SelectionMode", typeof(SelectionMode), typeof(ListPicker), new PropertyMetadata(SelectionMode.Single, OnSelectionModeChanged));

        /// <summary>
        /// Identifies the <see cref="SelectedItems"/> property.
        /// </summary>
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(IList), typeof(ListPicker), new PropertyMetadata(null));

        /// <summary>
        /// Occurs when the selection changes.
        /// </summary>
        public event SelectionChangedEventHandler SelectionChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListPicker"/> class.
        /// </summary>
        public ListPicker()
        {
            SetValue(SelectedItemsProperty, new ArrayList());
        }

        static ListPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ListPicker),
                new FrameworkPropertyMetadata(typeof(ListPicker)));
        }

        /// <summary>
        /// Gets or sets a delegate used to summarize multiple selected items.
        /// </summary>
        public Func<IList, string> SummaryForSelectedItemsDelegate
        {
            get { return (Func<IList, string>)GetValue(SummaryForSelectedItemsDelegateProperty); }
            set { SetValue(SummaryForSelectedItemsDelegateProperty, value); }
        }

        /// <summary>
        /// Gets or sets the picker mode.
        /// </summary>
        public ListPickerMode ListPickerMode
        {
            get { return (ListPickerMode)GetValue(ListPickerModeProperty); }
            set { SetValue(ListPickerModeProperty, value); }
        }

        /// <summary>
        /// Gets or sets the selected index.
        /// </summary>
        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        /// <summary>
        /// Gets or sets the selected item.
        /// </summary>
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        /// <summary>
        /// Gets or sets the item template used in full mode.
        /// </summary>
        public DataTemplate FullModeItemTemplate
        {
            get { return (DataTemplate)GetValue(FullModeItemTemplateProperty); }
            set { SetValue(FullModeItemTemplateProperty, value); }
        }

        /// <summary>
        /// Gets or sets the header.
        /// </summary>
        public object Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        /// <summary>
        /// Gets or sets the header template.
        /// </summary>
        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        /// <summary>
        /// Gets or sets the full mode header.
        /// </summary>
        public object FullModeHeader
        {
            get { return GetValue(FullModeHeaderProperty); }
            set { SetValue(FullModeHeaderProperty, value); }
        }

        /// <summary>
        /// Gets or sets the item count threshold for full mode.
        /// </summary>
        public int ItemCountThreshold
        {
            get { return (int)GetValue(ItemCountThresholdProperty); }
            set { SetValue(ItemCountThresholdProperty, value); }
        }

        /// <summary>
        /// Gets or sets the full mode page URI. Retained for compatibility.
        /// </summary>
        public Uri PickerPageUri
        {
            get { return (Uri)GetValue(PickerPageUriProperty); }
            set { SetValue(PickerPageUriProperty, value); }
        }

        /// <summary>
        /// Gets or sets the expansion mode.
        /// </summary>
        public ExpansionMode ExpansionMode
        {
            get { return (ExpansionMode)GetValue(ExpansionModeProperty); }
            set { SetValue(ExpansionModeProperty, value); }
        }

        /// <summary>
        /// Gets or sets the selection mode.
        /// </summary>
        public SelectionMode SelectionMode
        {
            get { return (SelectionMode)GetValue(SelectionModeProperty); }
            set { SetValue(SelectionModeProperty, value); }
        }

        /// <summary>
        /// Gets or sets the selected items.
        /// </summary>
        public IList SelectedItems
        {
            get { return (IList)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        /// <summary>
        /// Opens the picker.
        /// </summary>
        /// <returns><c>true</c> if the picker was opened.</returns>
        public bool Open()
        {
            if (Items.Count == 0)
            {
                return false;
            }

            if (ListPickerMode == ListPickerMode.Full
                || ExpansionMode == ExpansionMode.FullScreenOnly
                || Items.Count > ItemCountThreshold)
            {
                OpenFullMode();
                return true;
            }

            if (ListPickerMode == ListPickerMode.Expanded)
            {
                OpenFullMode();
                return true;
            }

            return OpenPopup();
        }

        /// <inheritdoc />
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_pickerButton != null)
            {
                _pickerButton.Click -= OnPickerButtonClick;
            }

            if (_pickerListBox != null)
            {
                _pickerListBox.SelectionChanged -= OnPopupSelectionChanged;
            }

            _pickerButton = GetTemplateChild("PickerButton") as Button;
            _pickerPopup = GetTemplateChild("PickerPopup") as Popup;
            _pickerListBox = GetTemplateChild("PickerListBox") as ListBox;

            if (_pickerButton != null)
            {
                _pickerButton.Click += OnPickerButtonClick;
            }

            if (_pickerListBox != null)
            {
                _pickerListBox.SelectionMode = SelectionMode;
                _pickerListBox.SelectionChanged += OnPopupSelectionChanged;
            }

            UpdatePickerContent();
            ChangeVisualState(false);
        }

        private void OnPickerButtonClick(object sender, RoutedEventArgs e)
        {
            Open();
        }

        private bool OpenPopup()
        {
            if (_pickerPopup == null || _pickerListBox == null)
            {
                return false;
            }

            _pickerListBox.ItemsSource = ItemsSource ?? Items;
            _pickerListBox.ItemTemplate = FullModeItemTemplate ?? ItemTemplate;
            _pickerListBox.SelectionMode = SelectionMode;
            _isUpdatingSelection = true;
            SyncListBoxSelection();
            _isUpdatingSelection = false;
            _pickerPopup.IsOpen = true;
            return true;
        }

        private void OpenFullMode()
        {
            if (_fullModeWindow != null)
            {
                return;
            }

            Window owner = Application.Current == null ? null : Application.Current.MainWindow;
            Window window = new Window
            {
                Title = FullModeHeader as string ?? Header as string ?? "Select",
                Owner = owner,
                WindowStartupLocation = owner == null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner,
                Width = owner == null ? 480 : Math.Min(480, Math.Max(360, owner.ActualWidth * 0.9)),
                Height = owner == null ? 640 : Math.Min(720, Math.Max(400, owner.ActualHeight * 0.9)),
                Background = new SolidColorBrush(Color.FromRgb(0, 0, 0))
            };

            Grid root = new Grid { Margin = new Thickness(12) };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            TextBlock title = new TextBlock
            {
                Text = (FullModeHeader as string ?? Header as string ?? "Select").ToLowerInvariant(),
                FontSize = 19,
                Foreground = new SolidColorBrush(Color.FromRgb(166, 166, 166)),
                Margin = new Thickness(0, 0, 0, 12)
            };
            root.Children.Add(title);

            ListBox listBox = new ListBox
            {
                ItemsSource = ItemsSource ?? Items,
                ItemTemplate = FullModeItemTemplate ?? ItemTemplate,
                SelectionMode = SelectionMode,
                Background = new SolidColorBrush(Color.FromRgb(31, 31, 31)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(166, 166, 166)),
                BorderThickness = new Thickness(1)
            };
            Grid.SetRow(listBox, 1);
            root.Children.Add(listBox);

            StackPanel buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 12, 0, 0)
            };
            Button cancel = new Button { Content = ControlResources.Cancel, Padding = new Thickness(12, 8, 12, 8) };
            cancel.Click += delegate { window.DialogResult = false; };
            Button done = new Button { Content = ControlResources.DateTimePickerDoneText, Padding = new Thickness(12, 8, 12, 8) };
            done.Click += delegate { window.DialogResult = true; };
            buttons.Children.Add(cancel);
            buttons.Children.Add(done);
            Grid.SetRow(buttons, 2);
            root.Children.Add(buttons);

            SyncListBoxSelection(listBox);
            window.Content = root;
            _fullModeWindow = window;

            bool? result = window.ShowDialog();
            if (result == true)
            {
                ApplyListBoxSelection(listBox);
            }

            _fullModeWindow = null;
        }

        private void SyncListBoxSelection()
        {
            if (_pickerListBox == null)
            {
                return;
            }

            SyncListBoxSelection(_pickerListBox);
        }

        private void SyncListBoxSelection(ListBox listBox)
        {
            _isUpdatingSelection = true;
            if (SelectionMode == SelectionMode.Single)
            {
                listBox.SelectedItem = SelectedItem;
            }
            else
            {
                listBox.SelectedItems.Clear();
                if (SelectedItems != null)
                {
                    foreach (object item in SelectedItems)
                    {
                        listBox.SelectedItems.Add(item);
                    }
                }
            }

            _isUpdatingSelection = false;
        }

        private void ApplyListBoxSelection(ListBox listBox)
        {
            object oldItem = SelectedItem;
            int oldIndex = SelectedIndex;

            _isUpdatingSelection = true;
            if (SelectionMode == SelectionMode.Single)
            {
                SelectedItem = listBox.SelectedItem;
                SelectedIndex = listBox.SelectedIndex;
                if (SelectedItems != null)
                {
                    SelectedItems.Clear();
                    if (SelectedItem != null)
                    {
                        SelectedItems.Add(SelectedItem);
                    }
                }
            }
            else
            {
                SelectedItems = new ArrayList();
                foreach (object item in listBox.SelectedItems)
                {
                    SelectedItems.Add(item);
                }

                if (SelectedItems.Count > 0)
                {
                    SelectedItem = SelectedItems[0];
                    SelectedIndex = Items.IndexOf(SelectedItem);
                }
                else
                {
                    SelectedItem = null;
                    SelectedIndex = -1;
                }
            }

            _isUpdatingSelection = false;
            UpdatePickerContent();

            if (!object.Equals(oldItem, SelectedItem) || oldIndex != SelectedIndex)
            {
                RaiseSelectionChanged(oldItem, SelectedItem);
            }
        }

        private void OnPopupSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingSelection || _pickerListBox == null)
            {
                return;
            }

            if (SelectionMode == SelectionMode.Single)
            {
                ApplyListBoxSelection(_pickerListBox);
                if (_pickerPopup != null)
                {
                    _pickerPopup.IsOpen = false;
                }
            }
            else
            {
                ApplyListBoxSelection(_pickerListBox);
            }
        }

        private void UpdatePickerContent()
        {
            if (_pickerButton == null)
            {
                return;
            }

            string summary = null;
            if (SelectionMode == SelectionMode.Multiple && SelectedItems != null && SelectedItems.Count > 0)
            {
                if (SummaryForSelectedItemsDelegate != null)
                {
                    summary = SummaryForSelectedItemsDelegate(SelectedItems);
                }

                if (summary == null)
                {
                    List<string> values = new List<string>();
                    foreach (object item in SelectedItems)
                    {
                        values.Add(item == null ? string.Empty : item.ToString());
                    }

                    summary = string.Join(", ", values.ToArray());
                }
            }
            else
            {
                summary = SelectedItem == null ? string.Empty : SelectedItem.ToString();
            }

            _pickerButton.Content = summary;
        }

        private void ChangeVisualState(bool useTransitions)
        {
            VisualStateManager.GoToState(this, IsEnabled ? "Normal" : "Disabled", useTransitions);
        }

        private static void OnListPickerModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ListPicker)d).UpdatePickerContent();
        }

        private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ListPicker picker = (ListPicker)d;
            if (picker._isUpdatingSelection)
            {
                return;
            }

            int index = (int)e.NewValue;
            object oldItem = picker.SelectedItem;
            object newItem = index >= 0 && index < picker.Items.Count ? picker.Items[index] : null;
            picker._isUpdatingSelection = true;
            picker.SelectedItem = newItem;
            picker._isUpdatingSelection = false;
            picker.UpdatePickerContent();
            picker.RaiseSelectionChanged(oldItem, newItem);
        }

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ListPicker picker = (ListPicker)d;
            if (picker._isUpdatingSelection)
            {
                return;
            }

            int index = picker.Items.IndexOf(e.NewValue);
            picker._isUpdatingSelection = true;
            picker.SelectedIndex = index;
            picker._isUpdatingSelection = false;

            if (picker.SelectedItems != null && picker.SelectionMode == SelectionMode.Single)
            {
                picker.SelectedItems.Clear();
                if (e.NewValue != null)
                {
                    picker.SelectedItems.Add(e.NewValue);
                }
            }

            picker.UpdatePickerContent();
            picker.RaiseSelectionChanged(e.OldValue, e.NewValue);
        }

        private static void OnSelectionModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ListPicker picker = (ListPicker)d;
            if (picker._pickerListBox != null)
            {
                picker._pickerListBox.SelectionMode = (SelectionMode)e.NewValue;
            }
        }

        private void RaiseSelectionChanged(object oldItem, object newItem)
        {
            SelectionChangedEventHandler handler = SelectionChanged;
            if (handler == null)
            {
                return;
            }

            List<object> removed = new List<object>();
            List<object> added = new List<object>();
            if (oldItem != null)
            {
                removed.Add(oldItem);
            }

            if (newItem != null)
            {
                added.Add(newItem);
            }

            handler(this, new SelectionChangedEventArgs(Selector.SelectionChangedEvent, removed, added));
        }
    }
}
