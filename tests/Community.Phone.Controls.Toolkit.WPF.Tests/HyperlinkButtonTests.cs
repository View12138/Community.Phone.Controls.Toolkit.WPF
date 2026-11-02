using System;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using Xunit;

namespace Community.Phone.Controls.Toolkit.WPF.Tests
{
    public class HyperlinkButtonTests
    {
        private const string TargetPageUri =
            "pack://application:,,,/Community.Phone.Controls.Toolkit.WPF.Tests;component/NavigateTargetPage.xaml";

        [Fact]
        public void HyperlinkButton_NavigatesApplicationUriInTheNearestFrame()
        {
            StaTest.Run(() =>
            {
                HyperlinkButton button = new HyperlinkButton
                {
                    Content = "go",
                    NavigateUri = new Uri(TargetPageUri, UriKind.Absolute),
                };
                Page source = new Page { Content = button };
                Frame frame = new Frame();
                Window window = new Window { Width = 480, Height = 800, Content = frame };

                try
                {
                    window.Show();
                    frame.Navigate(source);
                    WpfTestHelper.PumpDispatcher();

                    int clicks = 0;
                    button.Click += (sender, e) => clicks++;

                    button.PerformClick();

                    Assert.Equal(1, clicks);
                    Assert.True(
                        WpfTestHelper.PumpUntil(() => frame.Content is NavigateTargetPage),
                        "The frame should navigate to the page named by NavigateUri; current content: " + frame.Content);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void HyperlinkButton_OpensExternalUrisThroughTheShell()
        {
            StaTest.Run(() =>
            {
                RecordingHyperlinkButton http = new RecordingHyperlinkButton
                {
                    Content = "web",
                    NavigateUri = new Uri("https://example.com/path", UriKind.Absolute),
                };
                http.PerformClick();
                Assert.Equal(new Uri("https://example.com/path"), http.OpenedUri);

                RecordingHyperlinkButton mail = new RecordingHyperlinkButton
                {
                    Content = "mail",
                    NavigateUri = new Uri("mailto:someone@example.com", UriKind.Absolute),
                };
                mail.PerformClick();
                Assert.Equal(new Uri("mailto:someone@example.com"), mail.OpenedUri);

                RecordingHyperlinkButton none = new RecordingHyperlinkButton { Content = "none" };
                none.PerformClick();
                Assert.Null(none.OpenedUri);
            });
        }

        [Fact]
        public void HyperlinkButton_RetainsTargetNameAndExposesAHyperlinkPeer()
        {
            StaTest.Run(() =>
            {
                RecordingHyperlinkButton button = new RecordingHyperlinkButton
                {
                    Content = "web",
                    NavigateUri = new Uri("http://example.com", UriKind.Absolute),
                    TargetName = "_blank",
                };
                WpfTestHelper.ApplyDefaultStyle(button);

                Assert.Equal("_blank", button.TargetName);

                AutomationPeer peer = UIElementAutomationPeer.CreatePeerForElement(button);
                Assert.IsType<HyperlinkButtonAutomationPeer>(peer);
                Assert.Equal(AutomationControlType.Hyperlink, peer.GetAutomationControlType());

                ((IInvokeProvider)peer).Invoke();
                Assert.Equal(new Uri("http://example.com"), button.OpenedUri);
            });
        }

        private sealed class RecordingHyperlinkButton : HyperlinkButton
        {
            public Uri OpenedUri { get; private set; }

            protected override void OpenExternalUri(Uri uri)
            {
                OpenedUri = uri;
            }
        }
    }
}
