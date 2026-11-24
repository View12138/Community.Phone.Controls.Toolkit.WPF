// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Contains a list of selectable items with a mechanism that lets the user
    /// jump to a particular part of the list.
    /// </summary>
    /// <remarks>
    /// This is the WPF port of the Windows Phone 8.1 control. The public
    /// surface (dependency properties, events, selection and item realization)
    /// follows the phone control. The implementation uses a WPF items host with
    /// recycling virtualization for list mode and a fixed-cell panel for grid
    /// mode instead of the phone's Canvas-based virtualizer.
    /// </remarks>
    [StyleTypedProperty(Property = "JumpListStyle", StyleTargetType = typeof(LongListSelector))]
    [TemplatePart(Name = "ViewportControl", Type = typeof(ViewportControl))]
    [TemplateVisualState(Name = "NotScrolling", GroupName = "ScrollStates")]
    [TemplateVisualState(Name = "Scrolling", GroupName = "ScrollStates")]
    public class LongListSelector : Control, INotifyPropertyChanged
    {
        /// <summary>
        /// Identifies the GridCellSize dependency property.
        /// </summary>
        public static readonly DependencyProperty GridCellSizeProperty = DependencyProperty.Register(
            nameof(GridCellSize),
            typeof(Size),
            typeof(LongListSelector),
            new PropertyMetadata(Size.Empty, OnGridCellSizeChanged));

        /// <summary>
        /// Identifies the GroupFooterTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty GroupFooterTemplateProperty = DependencyProperty.Register(
            nameof(GroupFooterTemplate),
            typeof(DataTemplate),
            typeof(LongListSelector),
            new PropertyMetadata(null, OnTemplateChanged));

        /// <summary>
        /// Identifies the GroupHeaderTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty GroupHeaderTemplateProperty = DependencyProperty.Register(
            nameof(GroupHeaderTemplate),
            typeof(DataTemplate),
            typeof(LongListSelector),
            new PropertyMetadata(null, OnTemplateChanged));

        /// <summary>
        /// Identifies the HideEmptyGroups dependency property.
        /// </summary>
        public static readonly DependencyProperty HideEmptyGroupsProperty = DependencyProperty.Register(
            nameof(HideEmptyGroups),
            typeof(bool),
            typeof(LongListSelector),
            new PropertyMetadata(false, OnHideEmptyGroupsChanged));

        /// <summary>
        /// Identifies the IsGroupingEnabled dependency property.
        /// </summary>
        public static readonly DependencyProperty IsGroupingEnabledProperty = DependencyProperty.Register(
            nameof(IsGroupingEnabled),
            typeof(bool),
            typeof(LongListSelector),
            new PropertyMetadata(false, OnIsGroupingEnabledChanged));

        /// <summary>
        /// Identifies the ItemsSource dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IList),
            typeof(LongListSelector),
            new PropertyMetadata(null, OnItemsSourceChanged));

        /// <summary>
        /// Identifies the ItemTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
            nameof(ItemTemplate),
            typeof(DataTemplate),
            typeof(LongListSelector),
            new PropertyMetadata(null, OnTemplateChanged));

        /// <summary>
        /// Identifies the JumpListStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty JumpListStyleProperty = DependencyProperty.Register(
            nameof(JumpListStyle),
            typeof(Style),
            typeof(LongListSelector),
            new PropertyMetadata(null));

        /// <summary>
        /// Identifies the ListFooter dependency property.
        /// </summary>
        public static readonly DependencyProperty ListFooterProperty = DependencyProperty.Register(
            nameof(ListFooter),
            typeof(object),
            typeof(LongListSelector),
            new PropertyMetadata(null, OnListPartChanged));

        /// <summary>
        /// Identifies the ListFooterTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty ListFooterTemplateProperty = DependencyProperty.Register(
            nameof(ListFooterTemplate),
            typeof(DataTemplate),
            typeof(LongListSelector),
            new PropertyMetadata(null, OnTemplateChanged));

        /// <summary>
        /// Identifies the ListHeader dependency property.
        /// </summary>
        public static readonly DependencyProperty ListHeaderProperty = DependencyProperty.Register(
            nameof(ListHeader),
            typeof(object),
            typeof(LongListSelector),
            new PropertyMetadata(null, OnListPartChanged));

        /// <summary>
        /// Identifies the ListHeaderTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty ListHeaderTemplateProperty = DependencyProperty.Register(
            nameof(ListHeaderTemplate),
            typeof(DataTemplate),
            typeof(LongListSelector),
            new PropertyMetadata(null, OnTemplateChanged));

        /// <summary>
        /// Identifies the LayoutMode dependency property.
        /// </summary>
        public static readonly DependencyProperty LayoutModeProperty = DependencyProperty.Register(
            nameof(LayoutMode),
            typeof(LongListSelectorLayoutMode),
            typeof(LongListSelector),
            new PropertyMetadata(LongListSelectorLayoutMode.List, OnLayoutModeChanged));

        /// <summary>
        /// Identifies the SelectedItem dependency property. The phone control
        /// exposes a plain property; the port promotes it to a dependency
        /// property so it can be data bound without a code-behind handler.
        /// </summary>
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(object),
            typeof(LongListSelector),
            new PropertyMetadata(null, OnSelectedItemChanged));

        private static readonly ItemsPanelTemplate ListItemsPanelTemplate =
            CreateItemsPanelTemplate(typeof(VirtualizingStackPanel));

        private static readonly ItemsPanelTemplate GridItemsPanelTemplate =
            CreateItemsPanelTemplate(typeof(LongListSelectorGridPanel));

        private static readonly ControlTemplate ItemsHostTemplate = CreateItemsHostTemplate();

        private readonly LongListSelectorItemsHost _itemsHost;
        private readonly Dictionary<object, ContentPresenter> _realizedContainers =
            new Dictionary<object, ContentPresenter>();
        private readonly List<INotifyCollectionChanged> _groupNotifiers = new List<INotifyCollectionChanged>();
        private readonly DispatcherTimer _scrollIdleTimer;

        private ContentControl _viewportHost;
        private ScrollViewer _scrollViewer;
        private Panel _itemsPanel;
        private List<LongListSelectorItem> _flattenedItems = new List<LongListSelectorItem>();
        private INotifyCollectionChanged _itemsSourceNotifier;
        private ManipulationState _manipulationState;
        private bool _templateApplied;
        private bool _itemsDirty = true;
        private object _pendingScrollItem;
        private bool _hasPendingScroll;
        private Popup _jumpListPopup;
        private LongListSelector _jumpListSelector;
        private bool _jumpListClosedRaised;
        private int _scrollResetVersion;

        /// <summary>
        /// Initializes a new instance of the LongListSelector class.
        /// </summary>
        public LongListSelector()
        {
            DefaultStyleKey = typeof(LongListSelector);
            Focusable = true;

            _itemsHost = new LongListSelectorItemsHost(this)
            {
                Focusable = true,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                ItemTemplateSelector = new LongListSelectorTemplateSelector(this),
                Template = ItemsHostTemplate,
            };
            _itemsHost.ItemContainerGenerator.StatusChanged += OnItemContainerStatusChanged;
            _itemsHost.PreviewMouseLeftButtonDown += OnItemsHostPreviewMouseLeftButtonDown;
            _itemsHost.PreviewKeyDown += OnItemsHostPreviewKeyDown;
            _itemsHost.Loaded += OnItemsHostLoaded;

            _scrollIdleTimer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromMilliseconds(120),
            };
            _scrollIdleTimer.Tick += OnScrollIdleTimerTick;

            SizeChanged += OnSelectorSizeChanged;
        }

        /// <summary>
        /// Gets or sets the size used to display items in grid layout mode.
        /// </summary>
        public Size GridCellSize
        {
            get => (Size)GetValue(GridCellSizeProperty);
            set => SetValue(GridCellSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the template for the group footer.
        /// </summary>
        public DataTemplate GroupFooterTemplate
        {
            get => (DataTemplate)GetValue(GroupFooterTemplateProperty);
            set => SetValue(GroupFooterTemplateProperty, value);
        }

        /// <summary>
        /// Gets or sets the template for the group header.
        /// </summary>
        public DataTemplate GroupHeaderTemplate
        {
            get => (DataTemplate)GetValue(GroupHeaderTemplateProperty);
            set => SetValue(GroupHeaderTemplateProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether empty groups are hidden.
        /// </summary>
        public bool HideEmptyGroups
        {
            get => (bool)GetValue(HideEmptyGroupsProperty);
            set => SetValue(HideEmptyGroupsProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the items source contains groups.
        /// </summary>
        public bool IsGroupingEnabled
        {
            get => (bool)GetValue(IsGroupingEnabledProperty);
            set => SetValue(IsGroupingEnabledProperty, value);
        }

        /// <summary>
        /// Gets or sets the collection used to generate the content of the control.
        /// </summary>
        public IList ItemsSource
        {
            get => (IList)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        /// <summary>
        /// Gets or sets the template for items.
        /// </summary>
        public DataTemplate ItemTemplate
        {
            get => (DataTemplate)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        /// <summary>
        /// Gets or sets the style of the jump list.
        /// </summary>
        public Style JumpListStyle
        {
            get => (Style)GetValue(JumpListStyleProperty);
            set => SetValue(JumpListStyleProperty, value);
        }

        /// <summary>
        /// Gets or sets the object displayed at the bottom of the list.
        /// </summary>
        public object ListFooter
        {
            get => GetValue(ListFooterProperty);
            set => SetValue(ListFooterProperty, value);
        }

        /// <summary>
        /// Gets or sets the template for the object displayed at the bottom of the list.
        /// </summary>
        public DataTemplate ListFooterTemplate
        {
            get => (DataTemplate)GetValue(ListFooterTemplateProperty);
            set => SetValue(ListFooterTemplateProperty, value);
        }

        /// <summary>
        /// Gets or sets the object displayed at the top of the list.
        /// </summary>
        public object ListHeader
        {
            get => GetValue(ListHeaderProperty);
            set => SetValue(ListHeaderProperty, value);
        }

        /// <summary>
        /// Gets or sets the template for the object displayed at the top of the list.
        /// </summary>
        public DataTemplate ListHeaderTemplate
        {
            get => (DataTemplate)GetValue(ListHeaderTemplateProperty);
            set => SetValue(ListHeaderTemplateProperty, value);
        }

        /// <summary>
        /// Gets or sets a value that specifies whether the control is in list or grid mode.
        /// </summary>
        public LongListSelectorLayoutMode LayoutMode
        {
            get => (LongListSelectorLayoutMode)GetValue(LayoutModeProperty);
            set => SetValue(LayoutModeProperty, value);
        }

        /// <summary>
        /// Gets the manipulation state of the control.
        /// </summary>
        public ManipulationState ManipulationState => _manipulationState;

        /// <summary>
        /// Gets or sets the currently selected item.
        /// </summary>
        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        /// <summary>
        /// Occurs when a new item container is realized.
        /// </summary>
        public event EventHandler<ItemRealizationEventArgs> ItemRealized;

        /// <summary>
        /// Occurs when an item container is unrealized.
        /// </summary>
        public event EventHandler<ItemRealizationEventArgs> ItemUnrealized;

        /// <summary>
        /// Occurs when the jump list is being opened.
        /// </summary>
        public event EventHandler JumpListOpening;

        /// <summary>
        /// Occurs when the jump list has been closed.
        /// </summary>
        public event EventHandler JumpListClosed;

        /// <summary>
        /// Occurs when the manipulation state changes.
        /// </summary>
        public event EventHandler ManipulationStateChanged;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Occurs when the current selection changes.
        /// </summary>
        public event SelectionChangedEventHandler SelectionChanged;

        /// <summary>
        /// Gets the items host. Used by tests and by the feathering helper.
        /// </summary>
        internal ItemsControl ItemsHost => _itemsHost;

        /// <summary>
        /// Gets the flattened sequence of list parts, group parts and items.
        /// </summary>
        internal IReadOnlyList<LongListSelectorItem> FlattenedItems => _flattenedItems;

        /// <summary>
        /// Gets the scroll viewer that hosts the items, when the template is applied.
        /// </summary>
        internal ScrollViewer ScrollViewer => _scrollViewer;

        /// <summary>
        /// Gets the panel that arranges the realized item containers.
        /// </summary>
        internal Panel ItemsPanel => GetItemsPanel();

        /// <summary>
        /// Gets the selector shown in the jump list, while it is open.
        /// </summary>
        internal LongListSelector JumpListSelector => _jumpListSelector;

        /// <summary>
        /// Gets the template for a flattened element. The items host uses the
        /// template directly and puts <see cref="LongListSelectorItem.Data"/> in
        /// the container, so data bindings see the item (or group) itself.
        /// </summary>
        internal DataTemplate GetTemplateFor(LongListSelectorItem item)
        {
            return item?.ItemKind switch
            {
                LongListSelectorItemKind.ListHeader => ListHeaderTemplate,
                LongListSelectorItemKind.GroupHeader => GroupHeaderTemplate,
                LongListSelectorItemKind.Item => ItemTemplate,
                LongListSelectorItemKind.GroupFooter => GroupFooterTemplate,
                LongListSelectorItemKind.ListFooter => ListFooterTemplate,
                _ => null,
            };
        }

        /// <summary>
        /// Builds the visual tree for the control when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            DetachTemplateParts();
            base.OnApplyTemplate();

            _viewportHost = GetTemplateChild("ViewportControl") as ContentControl;

            if (_viewportHost != null)
            {
                _viewportHost.Content = _itemsHost;
            }

            _templateApplied = true;
            _itemsHost.ApplyTemplate();
            InitializeScrollViewer();
            ApplyLayoutMode();
            RebuildItems(preserveScroll: false);
            UpdateVisualState(false);

            if (_hasPendingScroll && _pendingScrollItem != null)
            {
                object pending = _pendingScrollItem;
                _pendingScrollItem = null;
                _hasPendingScroll = false;
                Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() => ScrollTo(pending)));
            }
        }

        /// <summary>
        /// Scrolls to the specified item.
        /// </summary>
        /// <param name="item">The item to scroll to.</param>
        public void ScrollTo(object item)
        {
            if (ItemsSource == null)
            {
                throw new ArgumentException("The items source does not contain the item.", nameof(item));
            }

            if (!_templateApplied)
            {
                _pendingScrollItem = item;
                _hasPendingScroll = true;
                return;
            }

            EnsureItemsBuilt();

            LongListSelectorItem entry = FindEntry(item);
            if (entry == null)
            {
                throw new ArgumentException("The items source does not contain the item.", nameof(item));
            }

            ScrollToIndex(_flattenedItems.IndexOf(entry));
        }

        /// <summary>
        /// Provides the behavior for the Measure pass of layout.
        /// </summary>
        /// <param name="availableSize">The available size.</param>
        /// <returns>The desired size.</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            return base.MeasureOverride(availableSize);
        }

        /// <summary>
        /// Handles key navigation when the control itself has focus.
        /// </summary>
        /// <param name="e">The key event data.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (!e.Handled)
            {
                HandleNavigationKey(e);
            }
        }

        private static void OnTemplateChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((LongListSelector)dependencyObject).RebuildItems(preserveScroll: true);
        }

        private static void OnListPartChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((LongListSelector)dependencyObject).RebuildItems(preserveScroll: true);
        }

        private static void OnHideEmptyGroupsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((LongListSelector)dependencyObject).RebuildItems(preserveScroll: true);
        }

        private static void OnIsGroupingEnabledChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            LongListSelector selector = (LongListSelector)dependencyObject;
            selector.SubscribeGroupChanges();
            selector.RebuildItems(preserveScroll: true);
        }

        private static void OnItemsSourceChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            LongListSelector selector = (LongListSelector)dependencyObject;
            selector.DetachItemsSourceNotifier();
            selector.AttachItemsSourceNotifier();
            selector.SubscribeGroupChanges();
            selector.RebuildItems(preserveScroll: false);
        }

        private static void OnLayoutModeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((LongListSelector)dependencyObject).ApplyLayoutMode();
        }

        private static void OnGridCellSizeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            LongListSelector selector = (LongListSelector)dependencyObject;
            if (selector.LayoutMode == LongListSelectorLayoutMode.Grid)
            {
                selector.RefreshContainerSizes();
            }
        }

        private static void OnSelectedItemChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            LongListSelector selector = (LongListSelector)dependencyObject;

            SelectionChangedEventArgs args = new SelectionChangedEventArgs(
                Selector.SelectionChangedEvent,
                new object[] { e.OldValue },
                new object[] { e.NewValue });
            selector.SelectionChanged?.Invoke(selector, args);
            selector.PropertyChanged?.Invoke(selector, new PropertyChangedEventArgs(nameof(SelectedItem)));
        }

        private static ItemsPanelTemplate CreateItemsPanelTemplate(Type panelType)
        {
            FrameworkElementFactory factory = new FrameworkElementFactory(panelType);
            ItemsPanelTemplate template = new ItemsPanelTemplate(factory);
            template.Seal();
            return template;
        }

        private static ControlTemplate CreateItemsHostTemplate()
        {
            FrameworkElementFactory scrollViewer = new FrameworkElementFactory(typeof(ScrollViewer), "ScrollViewer");
            scrollViewer.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
            scrollViewer.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
            scrollViewer.SetValue(UIElement.FocusableProperty, false);
            scrollViewer.SetValue(ScrollViewer.PanningModeProperty, PanningMode.VerticalOnly);
            scrollViewer.AppendChild(new FrameworkElementFactory(typeof(ItemsPresenter)));

            ControlTemplate template = new ControlTemplate(typeof(ItemsControl))
            {
                VisualTree = scrollViewer,
            };
            template.Seal();
            return template;
        }

        private void DetachTemplateParts()
        {
            if (_scrollViewer != null)
            {
                _scrollViewer.ScrollChanged -= OnScrollChanged;
                _scrollViewer = null;
            }

            if (_viewportHost != null)
            {
                _viewportHost.Content = null;
                _viewportHost = null;
            }
        }

        private void DetachItemsSourceNotifier()
        {
            if (_itemsSourceNotifier != null)
            {
                _itemsSourceNotifier.CollectionChanged -= OnSourceCollectionChanged;
                _itemsSourceNotifier = null;
            }
        }

        private void AttachItemsSourceNotifier()
        {
            _itemsSourceNotifier = ItemsSource as INotifyCollectionChanged;
            if (_itemsSourceNotifier != null)
            {
                _itemsSourceNotifier.CollectionChanged += OnSourceCollectionChanged;
            }
        }

        private void SubscribeGroupChanges()
        {
            foreach (INotifyCollectionChanged notifier in _groupNotifiers)
            {
                notifier.CollectionChanged -= OnGroupCollectionChanged;
            }

            _groupNotifiers.Clear();

            if (!IsGroupingEnabled || ItemsSource == null)
            {
                return;
            }

            HashSet<INotifyCollectionChanged> seen = new HashSet<INotifyCollectionChanged>();
            foreach (object group in ItemsSource)
            {
                if (group is INotifyCollectionChanged notifier && seen.Add(notifier))
                {
                    notifier.CollectionChanged += OnGroupCollectionChanged;
                    _groupNotifiers.Add(notifier);
                }
            }
        }

        private void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(DispatcherPriority.DataBind, new Action(() => OnSourceCollectionChanged(sender, e)));
                return;
            }

            SubscribeGroupChanges();
            RebuildItems(preserveScroll: true);
        }

        private void OnGroupCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(DispatcherPriority.DataBind, new Action(() => OnGroupCollectionChanged(sender, e)));
                return;
            }

            RebuildItems(preserveScroll: true);
        }

        private void RebuildItems(bool preserveScroll)
        {
            if (!_templateApplied || _itemsHost == null)
            {
                _itemsDirty = true;
                return;
            }

            double offset = preserveScroll ? (_scrollViewer?.VerticalOffset ?? 0) : 0;

            _flattenedItems = BuildFlattenedItems();
            _itemsDirty = false;

            Dictionary<object, ContentPresenter> previouslyRealized =
                new Dictionary<object, ContentPresenter>(_realizedContainers);
            _realizedContainers.Clear();

            _itemsHost.ItemsSource = null;
            _itemsHost.ItemsSource = _flattenedItems;

            if (ItemUnrealized != null)
            {
                foreach (KeyValuePair<object, ContentPresenter> pair in previouslyRealized)
                {
                    if (pair.Key is LongListSelectorItem entry)
                    {
                        ItemUnrealized(this, new ItemRealizationEventArgs(pair.Value, entry.ItemKind));
                    }
                }
            }

            // A new ItemsSource can leave the old containers alive until the
            // next layout pass; make sure the sizes of every container match
            // the current layout mode.
            RefreshContainerSizes();

            int resetVersion = ++_scrollResetVersion;
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                // A ScrollTo issued after the rebuild must not be undone by the
                // deferred reset (the reset is only meant for the data change).
                if (_scrollViewer == null || resetVersion != _scrollResetVersion)
                {
                    return;
                }

                if (preserveScroll && offset > 0)
                {
                    _scrollViewer.ScrollToVerticalOffset(offset);
                }
                else if (!preserveScroll)
                {
                    _scrollViewer.ScrollToTop();
                }

            }));

        }

        private void EnsureItemsBuilt()
        {
            if (_itemsDirty)
            {
                RebuildItems(preserveScroll: true);
            }
        }

        private List<LongListSelectorItem> BuildFlattenedItems()
        {
            List<LongListSelectorItem> items = new List<LongListSelectorItem>();

            bool hasListHeader = ListHeader is UIElement || ListHeaderTemplate != null;
            bool hasListFooter = ListFooter is UIElement || ListFooterTemplate != null;

            if (hasListHeader)
            {
                items.Add(new LongListSelectorItem(ListHeader, LongListSelectorItemKind.ListHeader, null));
            }

            IList source = ItemsSource;
            if (source != null)
            {
                if (IsGroupingEnabled)
                {
                    bool hasGroupHeader = GroupHeaderTemplate != null;
                    bool hasGroupFooter = GroupFooterTemplate != null;

                    foreach (object group in source)
                    {
                        IList groupItems = group as IList;
                        bool isEmpty = groupItems == null || groupItems.Count == 0;

                        if (HideEmptyGroups && isEmpty)
                        {
                            continue;
                        }

                        if (hasGroupHeader)
                        {
                            items.Add(new LongListSelectorItem(group, LongListSelectorItemKind.GroupHeader, group));
                        }

                        if (groupItems != null)
                        {
                            foreach (object item in groupItems)
                            {
                                items.Add(new LongListSelectorItem(item, LongListSelectorItemKind.Item, group));
                            }
                        }

                        if (hasGroupFooter)
                        {
                            items.Add(new LongListSelectorItem(group, LongListSelectorItemKind.GroupFooter, group));
                        }
                    }
                }
                else
                {
                    foreach (object item in source)
                    {
                        items.Add(new LongListSelectorItem(item, LongListSelectorItemKind.Item, null));
                    }
                }
            }

            if (hasListFooter)
            {
                items.Add(new LongListSelectorItem(ListFooter, LongListSelectorItemKind.ListFooter, null));
            }

            return items;
        }

        private LongListSelectorItem FindEntry(object data)
        {
            // The phone control resolves a group before an item, so a group
            // object that is also an item scrolls to its header.
            if (IsGroupingEnabled)
            {
                foreach (LongListSelectorItem entry in _flattenedItems)
                {
                    if (entry.ItemKind == LongListSelectorItemKind.GroupHeader && Equals(entry.Data, data))
                    {
                        return entry;
                    }
                }
            }

            foreach (LongListSelectorItem entry in _flattenedItems)
            {
                if (entry.ItemKind == LongListSelectorItemKind.Item && Equals(entry.Data, data))
                {
                    return entry;
                }
            }

            return null;
        }

        private void ApplyLayoutMode()
        {
            if (_itemsHost == null)
            {
                return;
            }

            bool grid = LayoutMode == LongListSelectorLayoutMode.Grid;
            _itemsHost.ItemsPanel = grid ? GridItemsPanelTemplate : ListItemsPanelTemplate;
            VirtualizingPanel.SetIsVirtualizing(_itemsHost, !grid);
            VirtualizingPanel.SetVirtualizationMode(_itemsHost, VirtualizationMode.Recycling);
            // Pixel scrolling keeps the scrollbar, the ScrollTo math and the
            // realized-range heights in the same unit (DIPs). Item scrolling
            // would report Extent/Viewport/VerticalOffset in item counts.
            VirtualizingPanel.SetScrollUnit(_itemsHost, ScrollUnit.Pixel);
            ScrollViewer.SetCanContentScroll(_itemsHost, !grid);
            _itemsPanel = null;
            _itemsHost.ApplyTemplate();
            InitializeScrollViewer();

            if (_scrollViewer != null)
            {
                _scrollViewer.CanContentScroll = !grid;
                _scrollViewer.PanningMode = PanningMode.VerticalOnly;
            }

            RefreshContainerSizes();
        }

        private void RefreshContainerSizes()
        {
            if (_itemsHost == null || !_templateApplied)
            {
                return;
            }

            // Re-running the generator applies the cell size (or clears it) on
            // every container; ItemsPanel already changed with the layout mode.
            _itemsHost.Items.Refresh();
            _itemsHost.InvalidateMeasure();
            _itemsPanel = null;
        }

        private void InitializeScrollViewer()
        {
            if (_itemsHost == null)
            {
                return;
            }

            ScrollViewer viewer = null;
            if (_itemsHost.Template != null)
            {
                _itemsHost.ApplyTemplate();
                viewer = _itemsHost.Template.FindName("ScrollViewer", _itemsHost) as ScrollViewer;
            }

            if (viewer == null)
            {
                viewer = FindDescendant<ScrollViewer>(_itemsHost);
            }

            if (ReferenceEquals(viewer, _scrollViewer))
            {
                return;
            }

            if (_scrollViewer != null)
            {
                _scrollViewer.ScrollChanged -= OnScrollChanged;
            }

            _scrollViewer = viewer;
            if (_scrollViewer != null)
            {
                _scrollViewer.ScrollChanged += OnScrollChanged;
                _scrollViewer.PanningMode = PanningMode.VerticalOnly;
            }

        }

        private void OnItemsHostLoaded(object sender, RoutedEventArgs e)
        {
            InitializeScrollViewer();
        }

        private void OnSelectorSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _itemsPanel = null;
            _itemsHost?.InvalidateMeasure();
        }

        private void OnItemContainerStatusChanged(object sender, EventArgs e)
        {
            if (_itemsHost == null || _itemsHost.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
            {
                return;
            }

            SynchronizeRealizedContainers();
        }

        private void SynchronizeRealizedContainers()
        {
            Dictionary<object, ContentPresenter> current = new Dictionary<object, ContentPresenter>();
            Panel panel = GetItemsPanel();

            if (panel != null && _itemsHost != null)
            {
                ItemContainerGenerator generator = _itemsHost.ItemContainerGenerator;
                foreach (UIElement child in panel.Children)
                {
                    if (child is not ContentPresenter container)
                    {
                        continue;
                    }

                    object entry = generator.ItemFromContainer(container);
                    if (entry is LongListSelectorItem && !current.ContainsKey(entry))
                    {
                        current[entry] = container;
                    }
                }
            }

            if (ItemUnrealized != null)
            {
                foreach (KeyValuePair<object, ContentPresenter> pair in _realizedContainers)
                {
                    if (!current.ContainsKey(pair.Key) && pair.Key is LongListSelectorItem entry)
                    {
                        ItemUnrealized(this, new ItemRealizationEventArgs(pair.Value, entry.ItemKind));
                    }
                }
            }

            if (ItemRealized != null)
            {
                foreach (KeyValuePair<object, ContentPresenter> pair in current)
                {
                    if (!_realizedContainers.ContainsKey(pair.Key) && pair.Key is LongListSelectorItem entry)
                    {
                        ItemRealized(this, new ItemRealizationEventArgs(pair.Value, entry.ItemKind));
                    }
                }
            }

            _realizedContainers.Clear();
            foreach (KeyValuePair<object, ContentPresenter> pair in current)
            {
                _realizedContainers[pair.Key] = pair.Value;
            }
        }

        private Panel GetItemsPanel()
        {
            if (_itemsHost == null)
            {
                return null;
            }

            if (_itemsPanel != null && ReferenceEquals(ItemsControl.GetItemsOwner(_itemsPanel), _itemsHost))
            {
                return _itemsPanel;
            }

            _itemsPanel = FindDescendant<Panel>(_itemsHost, panel => panel.IsItemsHost);
            return _itemsPanel;
        }

        /// <summary>
        /// Enumerates the containers that are currently realized.
        /// </summary>
        internal IEnumerable<ContentPresenter> GetRealizedContainers()
        {
            Panel panel = GetItemsPanel();
            if (panel == null)
            {
                yield break;
            }

            foreach (UIElement child in panel.Children)
            {
                if (child is ContentPresenter container && container.ActualHeight > 0)
                {
                    yield return container;
                }
            }
        }

        private void OnItemsHostPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            ContentPresenter container = FindContainerFromSource(e.OriginalSource as DependencyObject);
            if (container == null)
            {
                return;
            }

            object entryObject = _itemsHost.ItemContainerGenerator.ItemFromContainer(container);
            if (entryObject is not LongListSelectorItem entry)
            {
                return;
            }

            if (entry.ItemKind == LongListSelectorItemKind.Item)
            {
                SelectedItem = entry.Data;
            }
            else if (entry.ItemKind == LongListSelectorItemKind.GroupHeader &&
                     IsGroupingEnabled &&
                     JumpListStyle != null)
            {
                // Opening the popup from the mouse-down handler would hand the
                // popup the button capture and close it immediately.
                e.Handled = true;
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() => OpenJumpList(container)));
            }
        }

        private ContentPresenter FindContainerFromSource(DependencyObject source)
        {
            if (source == null || _itemsHost == null)
            {
                return null;
            }

            return ItemsControl.ContainerFromElement(_itemsHost, source) as ContentPresenter;
        }

        private void OnItemsHostPreviewKeyDown(object sender, KeyEventArgs e)
        {
            HandleNavigationKey(e);
        }

        private void HandleNavigationKey(KeyEventArgs e)
        {
            int direction;
            int index;

            switch (e.Key)
            {
                case Key.Down:
                    direction = 1;
                    index = IndexOfSelectedEntry();
                    if (index < 0)
                    {
                        index = -1;
                    }

                    break;

                case Key.Up:
                    direction = -1;
                    index = IndexOfSelectedEntry();
                    if (index < 0)
                    {
                        index = _flattenedItems.Count;
                    }

                    break;

                case Key.Home:
                    direction = 1;
                    index = -1;
                    break;

                case Key.End:
                    direction = -1;
                    index = _flattenedItems.Count;
                    break;

                default:
                    return;
            }

            if (MoveSelectionFrom(direction, index))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Moves the selection to the next or previous selectable item.
        /// </summary>
        internal bool MoveSelection(int direction)
        {
            if (direction == 0 || _flattenedItems.Count == 0)
            {
                return false;
            }

            int index = IndexOfSelectedEntry();
            if (index < 0)
            {
                index = direction > 0 ? -1 : _flattenedItems.Count;
            }

            return MoveSelectionFrom(direction, index);
        }

        private bool MoveSelectionFrom(int direction, int index)
        {
            if (direction == 0 || _flattenedItems.Count == 0)
            {
                return false;
            }

            int candidate = index;
            while (true)
            {
                candidate += direction;
                if (candidate < 0 || candidate >= _flattenedItems.Count)
                {
                    return false;
                }

                LongListSelectorItem entry = _flattenedItems[candidate];
                if (entry.ItemKind == LongListSelectorItemKind.Item)
                {
                    SelectedItem = entry.Data;
                    ScrollToIndex(candidate);
                    return true;
                }
            }
        }

        private int IndexOfSelectedEntry()
        {
            object selected = SelectedItem;
            if (selected == null)
            {
                return -1;
            }

            for (int i = 0; i < _flattenedItems.Count; i++)
            {
                LongListSelectorItem entry = _flattenedItems[i];
                if (entry.ItemKind == LongListSelectorItemKind.Item && Equals(entry.Data, selected))
                {
                    return i;
                }
            }

            return -1;
        }

        private void ScrollToIndex(int index)
        {
            if (_itemsHost == null || _scrollViewer == null || index < 0 || index >= _flattenedItems.Count)
            {
                return;
            }

            _scrollResetVersion++;
            SetManipulationState(ManipulationState.Animating);

            for (int attempt = 0; attempt < 8; attempt++)
            {
                if (_itemsHost.ItemContainerGenerator.ContainerFromIndex(index) is FrameworkElement container)
                {
                    container.BringIntoView();
                    RestartIdleTimer();
                    return;
                }

                double offset;
                if (TryGetRealizedRange(out int firstIndex, out int lastIndex, out double top, out double bottom))
                {
                    double average = (bottom - top) / Math.Max(1, lastIndex - firstIndex + 1);
                    if (index < firstIndex)
                    {
                        offset = _scrollViewer.VerticalOffset - ((firstIndex - index) * average);
                    }
                    else
                    {
                        offset = _scrollViewer.VerticalOffset + ((index - lastIndex) * average);
                    }
                }
                else
                {
                    double extent = _scrollViewer.ExtentHeight;
                    double viewport = _scrollViewer.ViewportHeight;
                    offset = (extent - viewport) * index / Math.Max(1, _flattenedItems.Count - 1);
                }

                double maximum = Math.Max(0, _scrollViewer.ExtentHeight - _scrollViewer.ViewportHeight);
                offset = Math.Min(Math.Max(0, offset), maximum);
                _scrollViewer.ScrollToVerticalOffset(offset);
                UpdateLayout();
            }

            RestartIdleTimer();
        }

        private bool TryGetRealizedRange(out int firstIndex, out int lastIndex, out double top, out double bottom)
        {
            firstIndex = int.MaxValue;
            lastIndex = -1;
            top = 0;
            bottom = 0;

            Panel panel = GetItemsPanel();
            if (panel == null || _itemsHost == null)
            {
                return false;
            }

            ItemContainerGenerator generator = _itemsHost.ItemContainerGenerator;
            foreach (UIElement child in panel.Children)
            {
                if (child is not ContentPresenter container || container.ActualHeight <= 0)
                {
                    continue;
                }

                int index = generator.IndexFromContainer(container);
                if (index < 0)
                {
                    continue;
                }

                GeneralTransform transform;
                try
                {
                    transform = container.TransformToAncestor(_itemsHost);
                }
                catch (ArgumentException)
                {
                    continue;
                }

                double y = transform.Transform(new Point(0, 0)).Y;
                if (index < firstIndex)
                {
                    firstIndex = index;
                    top = y;
                }

                if (index > lastIndex)
                {
                    lastIndex = index;
                    bottom = y + container.ActualHeight;
                }
            }

            return lastIndex >= 0 && lastIndex >= firstIndex;
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (Math.Abs(e.VerticalChange) > 0)
            {
                SetManipulationState(ManipulationState.Animating);

                RestartIdleTimer();
            }
        }

        private void RestartIdleTimer()
        {
            _scrollIdleTimer.Stop();
            _scrollIdleTimer.Start();
        }

        private void OnScrollIdleTimerTick(object sender, EventArgs e)
        {
            _scrollIdleTimer.Stop();
            SetManipulationState(ManipulationState.Idle);
        }

        private void SetManipulationState(ManipulationState state)
        {
            if (_manipulationState == state)
            {
                return;
            }

            _manipulationState = state;
            UpdateVisualState(true);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ManipulationState)));
            ManipulationStateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateVisualState(bool useTransitions)
        {
            if (!_templateApplied)
            {
                return;
            }

            string state = _manipulationState == ManipulationState.Idle ? "NotScrolling" : "Scrolling";
            VisualStateManager.GoToState(this, state, useTransitions);
        }

        /// <summary>
        /// Opens the jump list. The phone control declares this as an internal
        /// entry point so the group header tap can start the animation.
        /// </summary>
        /// <param name="groupHeaderUsedToOpenJumplist">The group header that was tapped.</param>
        internal void OpenJumpList(ContentPresenter groupHeaderUsedToOpenJumplist)
        {
            _ = groupHeaderUsedToOpenJumplist;

            if (JumpListStyle == null || ItemsSource == null || _jumpListPopup != null || !IsGroupingEnabled)
            {
                return;
            }

            List<object> groups = new List<object>();
            foreach (object group in ItemsSource)
            {
                bool isEmpty = !(group is IList list) || list.Count == 0;
                if (HideEmptyGroups && isEmpty)
                {
                    continue;
                }

                groups.Add(group);
            }

            if (groups.Count == 0)
            {
                return;
            }

            LongListSelector jumpList = new LongListSelector
            {
                IsGroupingEnabled = false,
                ItemsSource = groups,
                Style = JumpListStyle,
            };
            jumpList.SelectionChanged += OnJumpListSelectionChanged;
            jumpList.ItemRealized += OnJumpListItemRealized;

            Border overlay = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(0x9E, 0, 0, 0)),
                Width = Math.Max(0, ActualWidth),
                Height = Math.Max(0, ActualHeight),
                Child = jumpList,
            };

            Popup popup = new Popup
            {
                Child = overlay,
                PlacementTarget = this,
                Placement = PlacementMode.Relative,
                StaysOpen = false,
                AllowsTransparency = true,
            };
            popup.Closed += OnJumpListPopupClosed;

            _jumpListSelector = jumpList;
            _jumpListPopup = popup;
            _jumpListClosedRaised = false;

            JumpListOpening?.Invoke(this, EventArgs.Empty);

            popup.IsOpen = true;
            jumpList.Focus();

            if (groups.Count > 1 && !HasItems(groups[0]))
            {
                foreach (object group in groups)
                {
                    if (HasItems(group))
                    {
                        jumpList.SelectedItem = null;
                        jumpList.ScrollTo(group);
                        break;
                    }
                }
            }
        }

        private void OnJumpListItemRealized(object sender, ItemRealizationEventArgs e)
        {
            if (e.Container != null)
            {
                e.Container.IsHitTestVisible = HasItems(e.Container.DataContext);
            }
        }

        private void OnJumpListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object selected = ((LongListSelector)sender).SelectedItem;
            CloseJumpList(selected);
        }

        private void OnJumpListPopupClosed(object sender, EventArgs e)
        {
            if (_jumpListPopup != null)
            {
                CloseJumpList(null);
            }
        }

        private void CloseJumpList(object selectedItem)
        {
            if (_jumpListPopup == null)
            {
                return;
            }

            Popup popup = _jumpListPopup;
            LongListSelector jumpList = _jumpListSelector;
            _jumpListPopup = null;
            _jumpListSelector = null;

            if (jumpList != null)
            {
                jumpList.SelectionChanged -= OnJumpListSelectionChanged;
                jumpList.ItemRealized -= OnJumpListItemRealized;
            }

            popup.Closed -= OnJumpListPopupClosed;
            popup.IsOpen = false;

            RaiseJumpListClosed();

            if (selectedItem != null)
            {
                try
                {
                    ScrollTo(selectedItem);
                }
                catch (ArgumentException)
                {
                    // The group disappeared while the jump list was open.
                }
            }
        }

        private void RaiseJumpListClosed()
        {
            if (_jumpListClosedRaised)
            {
                return;
            }

            _jumpListClosedRaised = true;
            JumpListClosed?.Invoke(this, EventArgs.Empty);
        }

        private static bool HasItems(object item)
        {
            if (item is IList list)
            {
                return list.Count > 0;
            }

            if (item is IEnumerable enumerable)
            {
                IEnumerator enumerator = enumerable.GetEnumerator();
                try
                {
                    return enumerator.MoveNext();
                }
                finally
                {
                    (enumerator as IDisposable)?.Dispose();
                }
            }

            return false;
        }

        private static T FindDescendant<T>(DependencyObject root, Predicate<T> predicate = null)
            where T : DependencyObject
        {
            if (root == null)
            {
                return null;
            }

            int count = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(root, i);
                if (child is T typed && (predicate == null || predicate(typed)))
                {
                    return typed;
                }

                T descendant = FindDescendant(child, predicate);
                if (descendant != null)
                {
                    return descendant;
                }
            }

            return null;
        }
    }
}
