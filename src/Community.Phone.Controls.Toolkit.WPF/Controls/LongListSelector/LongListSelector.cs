#region 程序集 Microsoft.Phone, Version=8.0.0.0, Culture=neutral, PublicKeyToken=24eec0d8c86cda1e
// C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\WindowsPhone\v8.0\Microsoft.Phone.dll
#endregion

using Microsoft.Phone.Controls.Primitives;
using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Microsoft.Phone.Controls
{
    //
    // 摘要:
    //     包含具有某种机制的可选项列表，该机制可使用户跳转到该列表的特定部分。
    [StyleTypedProperty(Property = "JumpListStyle", StyleTargetType = typeof(LongListSelector))]
    [TemplatePart(Name = "ViewportControl", Type = typeof(ViewportControl))]
    [TemplatePart(Name = "VerticalScrollBar", Type = typeof(ScrollBar))]
    [TemplateVisualState(Name = "NotScrolling", GroupName = "ScrollStates")]
    [TemplateVisualState(Name = "Scrolling", GroupName = "ScrollStates")]
    public class LongListSelector : Control, INotifyPropertyChanged
    {
        //
        // 摘要:
        //     标识 Microsoft.Phone.Controls.LongListSelector.GridCellSize 依赖项属性。
        //
        // 返回结果:
        //     Microsoft.Phone.Controls.LongListSelector.GridCellSize 依赖项属性的标识符。
        public static readonly DependencyProperty GridCellSizeProperty;
        //
        // 摘要:
        //     标识 Microsoft.Phone.Controls.LongListSelector.GroupFooterTemplate 依赖项属性。
        //
        // 返回结果:
        //     Microsoft.Phone.Controls.LongListSelector.GroupFooterTemplate 依赖项属性的标识符。
        public static readonly DependencyProperty GroupFooterTemplateProperty;
        //
        // 摘要:
        //     标识 Microsoft.Phone.Controls.LongListSelector.GroupHeaderTemplate 依赖项属性。
        //
        // 返回结果:
        //     Microsoft.Phone.Controls.LongListSelector.GroupHeaderTemplate 依赖项属性的标识符。
        public static readonly DependencyProperty GroupHeaderTemplateProperty;
        //
        // 摘要:
        //     标识 Microsoft.Phone.Controls.LongListSelector.HideEmptyGroups 依赖项属性。
        //
        // 返回结果:
        //     Microsoft.Phone.Controls.LongListSelector.HideEmptyGroups 依赖项属性的标识符。
        public static readonly DependencyProperty HideEmptyGroupsProperty;
        //
        // 摘要:
        //     标识 Microsoft.Phone.Controls.LongListSelector.IsGroupingEnabled 依赖项属性。
        //
        // 返回结果:
        //     Microsoft.Phone.Controls.LongListSelector.IsGroupingEnabled 依赖项属性的标识符。
        public static readonly DependencyProperty IsGroupingEnabledProperty;
        //
        // 摘要:
        //     标识 Microsoft.Phone.Controls.LongListSelector.ItemsSource 依赖项属性。
        //
        // 返回结果:
        //     Microsoft.Phone.Controls.LongListSelector.ItemsSource 依赖项属性的标识符。
        public static readonly DependencyProperty ItemsSourceProperty;
        //
        // 摘要:
        //     标识 Microsoft.Phone.Controls.LongListSelector.ItemTemplate 依赖项属性。
        //
        // 返回结果:
        //     Microsoft.Phone.Controls.LongListSelector.ItemTemplate 依赖项属性的标识符。
        public static readonly DependencyProperty ItemTemplateProperty;
        //
        // 摘要:
        //     标识 Microsoft.Phone.Controls.LongListSelector.JumpListStyle 依赖项属性。
        //
        // 返回结果:
        //     Microsoft.Phone.Controls.LongListSelector.JumpListStyle 依赖项属性的标识符。
        public static readonly DependencyProperty JumpListStyleProperty;
        //
        // 摘要:
        //     标识 Microsoft.Phone.Controls.LongListSelector.ListFooter 依赖项属性。
        //
        // 返回结果:
        //     Microsoft.Phone.Controls.LongListSelector.ListFooter 依赖项属性的标识符。
        public static readonly DependencyProperty ListFooterProperty;
        //
        // 摘要:
        //     标识 Microsoft.Phone.Controls.LongListSelector.ListFooterTemplate 依赖项属性。
        //
        // 返回结果:
        //     Microsoft.Phone.Controls.LongListSelector.ListFooterTemplate 依赖项属性的标识符。
        public static readonly DependencyProperty ListFooterTemplateProperty;
        //
        // 摘要:
        //     标识 Microsoft.Phone.Controls.LongListSelector.ListHeader 依赖项属性。
        //
        // 返回结果:
        //     Microsoft.Phone.Controls.LongListSelector.ListHeader 依赖项属性的标识符。
        public static readonly DependencyProperty ListHeaderProperty;
        //
        // 摘要:
        //     标识 Microsoft.Phone.Controls.LongListSelector.ListHeaderTemplate 依赖项属性。
        //
        // 返回结果:
        //     Microsoft.Phone.Controls.LongListSelector.ListHeaderTemplate 依赖项属性的标识符。
        public static readonly DependencyProperty ListHeaderTemplateProperty;

        //
        // 摘要:
        //     初始化 Microsoft.Phone.Controls.LongListSelector 类的新实例。
        public LongListSelector() { }

        //
        // 摘要:
        //     获取或设置当在 Microsoft.Phone.Controls.LongListSelector 中显示项时使用的大小。
        //
        // 返回结果:
        //     在显示项时使用的大小。
        public Size GridCellSize { get; set; }
        //
        // 摘要:
        //     获取或设置 Microsoft.Phone.Controls.LongListSelector 控件中组页脚的模板。
        //
        // 返回结果:
        //     为 Microsoft.Phone.Controls.LongListSelector 中的组页脚提供模板的 System.Windows.DataTemplate。
        public DataTemplate GroupFooterTemplate { get; set; }
        //
        // 摘要:
        //     获取或设置 Microsoft.Phone.Controls.LongListSelector 控件中组页眉的模板。
        //
        // 返回结果:
        //     Microsoft.Phone.Controls.LongListSelector 中组页眉的 System.Windows.DataTemplate。
        public DataTemplate GroupHeaderTemplate { get; set; }
        //
        // 摘要:
        //     获取或设置一个值，该值指示是否隐藏 Microsoft.Phone.Controls.LongListSelector 中的空组。
        //
        // 返回结果:
        //     如果隐藏空组，则为 true；否则为 false。
        public bool HideEmptyGroups { get; set; }
        //
        // 摘要:
        //     获取或设置一个值，该值指示是否在 Microsoft.Phone.Controls.LongListSelector 中启用了分组。
        //
        // 返回结果:
        //     如果启用了分组，则为 true；否则为 false。
        public bool IsGroupingEnabled { get; set; }
        //
        // 摘要:
        //     获取或设置用于生成 Microsoft.Phone.Controls.LongListSelector 的内容的集合。
        //
        // 返回结果:
        //     用于生成 Microsoft.Phone.Controls.LongListSelector 的内容的集合。
        public IList ItemsSource { get; set; }
        //
        // 摘要:
        //     获取或设置项视图中项的模板。
        //
        // 返回结果:
        //     项视图中项的 System.Windows.DataTemplate。
        public DataTemplate ItemTemplate { get; set; }
        //
        // 摘要:
        //     获取或设置 Microsoft.Phone.Controls.LongListSelector 中跳转列表的 System.Windows.Style。
        //
        // 返回结果:
        //     Microsoft.Phone.Controls.LongListSelector 中跳转列表的 System.Windows.Style。
        public Style JumpListStyle { get; set; }
        //
        // 摘要:
        //     获取或设置一个值，该值指定 Microsoft.Phone.Controls.LongListSelector 是处于来自 Microsoft.Phone.Controls.LongListSelectorLayoutMode
        //     枚举的列表模式还是网格模式。
        //
        // 返回结果:
        //     一个 Microsoft.Phone.Controls.LongListSelectorLayoutMode 值，该值指定 Microsoft.Phone.Controls.LongListSelector
        //     是处于列表模式还是网格模式。
        public LongListSelectorLayoutMode LayoutMode { get; set; }
        //
        // 摘要:
        //     获取或设置在 Microsoft.Phone.Controls.LongListSelector 底端显示的对象。
        //
        // 返回结果:
        //     在 Microsoft.Phone.Controls.LongListSelector 底端显示的 System.Object。
        public object ListFooter { get; set; }
        //
        // 摘要:
        //     获取或设置在 Microsoft.Phone.Controls.LongListSelector 底端显示的项的 System.Windows.DataTemplate。
        //
        // 返回结果:
        //     在 Microsoft.Phone.Controls.LongListSelector 底端显示的项的 System.Windows.DataTemplate。
        public DataTemplate ListFooterTemplate { get; set; }
        //
        // 摘要:
        //     获取或设置在 Microsoft.Phone.Controls.LongListSelector 顶端显示的对象。
        //
        // 返回结果:
        //     在 Microsoft.Phone.Controls.LongListSelector 顶端显示的 System.Object。
        public object ListHeader { get; set; }
        //
        // 摘要:
        //     获取或设置在 Microsoft.Phone.Controls.LongListSelector 顶端显示的项的 System.Windows.DataTemplate。
        //
        // 返回结果:
        //     在 Microsoft.Phone.Controls.LongListSelector 顶端显示的项的 System.Windows.DataTemplate。
        public DataTemplate ListHeaderTemplate { get; set; }
        //
        // 摘要:
        //     获取在 Microsoft.Phone.Controls.LongListSelector 控件上处理的操作状态。
        //
        // 返回结果:
        //     在 Microsoft.Phone.Controls.LongListSelector 控件上处理的操作状态。
        public ManipulationState ManipulationState { get; }
        //
        // 摘要:
        //     获取 Microsoft.Phone.Controls.LongListSelector 中当前选择的项。
        //
        // 返回结果:
        //     一个 System.Object，它表示 Microsoft.Phone.Controls.LongListSelector 中的当前选择的项。
        public object SelectedItem { get; set; }

        //
        // 摘要:
        //     在实现新项时发生。
        public event EventHandler<ItemRealizationEventArgs> ItemRealized;
        //
        // 摘要:
        //     在 Microsoft.Phone.Controls.LongListSelector 中的项未实现时发生。
        public event EventHandler<ItemRealizationEventArgs> ItemUnrealized;
        //
        // 摘要:
        //     在关闭跳转列表时发生。
        public event EventHandler JumpListClosed;
        //
        // 摘要:
        //     在打开跳转列表时发生。
        public event EventHandler JumpListOpening;
        //
        // 摘要:
        //     在 Microsoft.Phone.Controls.ManipulationState 更改时发生。
        public event EventHandler ManipulationStateChanged;
        //
        // 摘要:
        //     在更改属性值时发生。
        public event PropertyChangedEventHandler PropertyChanged;
        //
        // 摘要:
        //     在当前选定项更改时发生。
        public event SelectionChangedEventHandler SelectionChanged;

        //
        // 摘要:
        //     当应用新模板时生成 Microsoft.Phone.Controls.LongListSelector 控件的可视化树。
        public override void OnApplyTemplate() { }
        //
        // 摘要:
        //     滚动到 Microsoft.Phone.Controls.LongListSelector 中的指定项目。
        //
        // 参数:
        //   item:
        //     要滚动到的列表项。
        public void ScrollTo(object item) { }
        //
        // 摘要:
        //     提供布局的测量处理过程的行为。
        //
        // 参数:
        //   availableSize:
        //     此对象可以赋给子对象的可用大小。可以指定无穷大值 (System.Double.PositiveInfinity)，以指示对象的大小将调整为可用内容的大小。
        //
        // 返回结果:
        //     此对象在布局过程中基于其对子对象分配大小的计算或者基于固定容器大小等其他因素而确定的它所需的大小。
        protected override Size MeasureOverride(Size availableSize)
        {
            throw new NotImplementedException();
        }
    }
}