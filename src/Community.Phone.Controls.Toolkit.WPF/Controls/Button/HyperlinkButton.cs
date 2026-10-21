using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;

namespace Microsoft.Phone.Controls
{
    [TemplateVisualState(Name = "MouseOver", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Focused", GroupName = "FocusStates")]
    [TemplateVisualState(Name = "Normal", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Pressed", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Disabled", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Unfocused", GroupName = "FocusStates")]
    public class HyperlinkButton : ButtonBase
    {
        public Uri NavigateUri
        {
            get { return (Uri)GetValue(NavigateUriProperty); }
            set { SetValue(NavigateUriProperty, value); }
        }

        public static readonly DependencyProperty NavigateUriProperty =
            DependencyProperty.Register(nameof(NavigateUri), typeof(Uri), typeof(HyperlinkButton), new PropertyMetadata(null));

        public string TargetName
        {
            get { return (string)GetValue(TargetNameProperty); }
            set { SetValue(TargetNameProperty, value); }
        }
        public static readonly DependencyProperty TargetNameProperty =
            DependencyProperty.Register(nameof(TargetName), typeof(string), typeof(HyperlinkButton), new PropertyMetadata(null));


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        protected override void OnClick()
        {
            base.OnClick();
        }
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return base.OnCreateAutomationPeer();
        }

    }
}
