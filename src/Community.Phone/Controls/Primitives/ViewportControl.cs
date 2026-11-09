using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace System.Windows.Controls.Primitives
{
    //
    // 摘要:
    //     表示 Microsoft.Phone.Controls.LongListSelector 中的虚拟化可滚动区域。
    [TemplatePart(Name = "ContentElement", Type = typeof(ContentPresenter))]
    public sealed class ViewportControl : ContentControl
    {
        //
        // 摘要:
        //     标识 System.Windows.Controls.Primitives.ViewportControl.Bounds 依赖项属性。
        //
        // 返回结果:
        //     System.Windows.Controls.Primitives.ViewportControl.Bounds 依赖项属性的标识符。
        public static readonly DependencyProperty BoundsProperty;
        //
        // 摘要:
        //     标识 System.Windows.Controls.Primitives.ViewportControl.ManipulationLockMode 依赖项属性。
        //
        // 返回结果:
        //     System.Windows.Controls.Primitives.ViewportControl.ManipulationLockMode 依赖项属性的标识符。
        public static readonly DependencyProperty ManipulationLockModeProperty;
        //
        // 摘要:
        //     标识 System.Windows.Controls.Primitives.ViewportControl.ManipulationState 依赖项属性。
        //
        // 返回结果:
        //     System.Windows.Controls.Primitives.ViewportControl.ManipulationState 依赖项属性的标识符。
        public static readonly DependencyProperty ManipulationStateProperty;
        //
        // 摘要:
        //     标识 System.Windows.Controls.Primitives.ViewportControl.Viewport 依赖项属性。
        //
        // 返回结果:
        //     System.Windows.Controls.Primitives.ViewportControl.Viewport 依赖项属性的标识符。
        public static readonly DependencyProperty ViewportProperty;

        //
        // 摘要:
        //     初始化 System.Windows.Controls.Primitives.ViewportControl 类的新实例。
        public ViewportControl() { }

        //
        // 摘要:
        //     获取或设置 System.Windows.Controls.Primitives.ViewportControl 中的可滚动图面的边界。
        //
        // 返回结果:
        //     System.Windows.Controls.Primitives.ViewportControl 中可滚动图面的边界。
        public Rect Bounds { get; set; }
        //
        // 摘要:
        //     获取或设置笔势方向。
        //
        // 返回结果:
        //     笔势方向。
        public ManipulationLockMode ManipulationLockMode { get; set; }
        //
        // 摘要:
        //     获取当前 System.Windows.Controls.Primitives.ManipulationState。
        //
        // 返回结果:
        //     当前的 System.Windows.Controls.Primitives.ManipulationState。默认值为 System.Windows.Controls.Primitives.ManipulationState.Idle。
        public ManipulationState ManipulationState { get; }
        //
        // 摘要:
        //     获取当前视区。
        //
        // 返回结果:
        //     当前视区。
        public Rect Viewport { get; }

        //
        // 摘要:
        //     操作状态更改时发生。
        public event EventHandler<ManipulationStateChangedEventArgs> ManipulationStateChanged;
        //
        // 摘要:
        //     当视区更改时发生。
        public event EventHandler<ViewportChangedEventArgs> ViewportChanged;

        //
        // 摘要:
        //     当应用新模板时生成 System.Windows.Controls.Primitives.ViewportControl 的可视化树。
        public override void OnApplyTemplate() { }
        //
        // 摘要:
        //     滚动到指定的视区原点。
        //
        // 参数:
        //   location:
        //     指定的视区原点。
        public void SetViewportOrigin(Point location) { }
        protected override void OnManipulationCompleted(ManipulationCompletedEventArgs e) { }
        protected override void OnManipulationDelta(ManipulationDeltaEventArgs e) { }
        protected override void OnManipulationStarted(ManipulationStartedEventArgs e) { }
    }
}
