namespace System.Windows.Controls.Primitives
{
    //
    // 摘要:
    //     指定值，该值可用于锁定对 System.Windows.Controls.Primitives.ViewportControl 的操作。
    public enum ManipulationLockMode
    {
        //
        // 摘要:
        //     可以自由地拖动 System.Windows.Controls.Primitives.ViewportControl 的所有边界并轻弹控件。
        Free = 0,
        //
        // 摘要:
        //     System.Windows.Controls.Primitives.ViewportControl 仅可水平滚动。
        Horizontal = 1,
        //
        // 摘要:
        //     System.Windows.Controls.Primitives.ViewportControl 仅可以垂直滚动。
        Vertical = 2,
        //
        // 摘要:
        //     System.Windows.Controls.Primitives.ViewportControl 被锁定在 X 和 Y 轴两者任意一侧的 22.5 度角上。
        PreferHorizontalOrVertical = 3,
        //
        // 摘要:
        //     System.Windows.Controls.Primitives.ViewportControl 被锁定在 X 或 Y 轴两者任意一侧的 45 度角上。
        HorizontalOrVertical = 4
    }
}
