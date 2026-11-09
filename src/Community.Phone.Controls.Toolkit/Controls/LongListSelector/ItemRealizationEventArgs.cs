#region 程序集 Microsoft.Phone, Version=8.0.0.0, Culture=neutral, PublicKeyToken=24eec0d8c86cda1e
// C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\WindowsPhone\v8.0\Microsoft.Phone.dll
#endregion

using System;
using System.Windows.Controls;

namespace Microsoft.Phone.Controls
{
    //
    // 摘要:
    //     为 Microsoft.Phone.Controls.LongListSelector 的 Microsoft.Phone.Controls.LongListSelector.ItemRealized
    //     事件提供数据。
    public sealed class ItemRealizationEventArgs : EventArgs
    {
        internal ItemRealizationEventArgs(ContentPresenter container, LongListSelectorItemKind itemKind)
        {
            Container = container;
            ItemKind = itemKind;
        }

        //
        // 摘要:
        //     获取正在显示项目的 System.Windows.Controls.ContentPresenter。
        //
        // 返回结果:
        //     正在显示项目的 System.Windows.Controls.ContentPresenter。
        public ContentPresenter Container { get; }
        //
        // 摘要:
        //     获取实现的项目的类型。
        //
        // 返回结果:
        //     实现的项目的类型。
        public LongListSelectorItemKind ItemKind { get; }
    }
}