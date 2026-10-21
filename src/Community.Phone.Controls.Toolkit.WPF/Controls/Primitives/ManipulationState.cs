
namespace System.Windows.Controls.Primitives
{
    //
    // 摘要:
    //     指定 ViewPortControl 互斥操作的不同状态。
    public enum ManipulationState
    {
        //
        // 摘要:
        //     指定不会进行任何操作或进行动画处理。
        Idle = 0,
        //
        // 摘要:
        //     指定正在识别笔势、手指处于按下状态并已接收任何增量、拖动/平移或轻拂。
        Manipulating = 1,
        //
        // 摘要:
        //     指定当前无笔势发生，但还有一些动画发生，如滚动动画或压缩动画。
        Animating = 2
    }
}

