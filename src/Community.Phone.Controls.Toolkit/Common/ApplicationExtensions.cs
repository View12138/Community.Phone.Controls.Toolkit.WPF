using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Microsoft.Phone.Controls;

public static class ApplicationExtensions
{
    extension(Application application)
    {
        public UIElement RootVisual
        {
            get
            {
                return application?.MainWindow?.Content as UIElement;
            }
        }
    }
}
