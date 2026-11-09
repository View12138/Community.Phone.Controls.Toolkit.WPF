using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Microsoft.Phone.Controls;

public static class MessageBoxExtensions
{
    extension(MessageBox)
    {
        public static void ShowCustomer(string message, string caption = null)
        {
            CustomMessageBox customMessageBox = new CustomMessageBox
            {
                Caption = caption,
                Message = message,
                LeftButtonContent = "ok",
                RightButtonContent = null,
                IsFullScreen = false,
                IsCenterScreen = false
            };
            customMessageBox.Show();
        }
    }
}
