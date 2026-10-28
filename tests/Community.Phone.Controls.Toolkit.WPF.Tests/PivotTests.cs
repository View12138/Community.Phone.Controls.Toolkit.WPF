using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.Primitives;
using Xunit;

namespace Community.Phone.Controls.Toolkit.WPF.Tests
{
    /// <summary>
    /// Covers the WPF adaptation of the phone <see cref="Pivot"/>: the container
    /// and header structure, the slide that changes the active item, header
    /// clicks, keyboard navigation and the locked state.
    /// </summary>
    public class PivotTests
    {
        [Fact]
        public void Theme_ContainsPivotStyles()
        {
            StaTest.Run(() =>
            {
                Assert.NotNull(WpfTestHelper.Theme[typeof(Pivot)]);
                Assert.NotNull(WpfTestHelper.Theme[typeof(PivotItem)]);
                Assert.NotNull(WpfTestHelper.Theme[typeof(PivotHeadersControl)]);
                Assert.NotNull(WpfTestHelper.Theme[typeof(PivotHeaderItem)]);
                Assert.NotNull(WpfTestHelper.Theme["PivotTitleStyle"]);
            });
        }

        [Fact]
        public void Pivot_ShowsTheFirstItemAndBuildsTheHeaderStrip()
        {
            StaTest.Run(() =>
            {
                Window window;
                Pivot pivot = CreatePivot(3, out window);
                try
                {
                    Assert.Equal(0, pivot.SelectedIndex);
                    Assert.Same(pivot.Items[0], pivot.SelectedItem);

                    PivotItem first = (PivotItem)pivot.Items[0];
                    PivotItem second = (PivotItem)pivot.Items[1];

                    Assert.Equal(Visibility.Visible, first.Visibility);
                    Assert.True(first.IsHitTestVisible);
                    Assert.Equal(Visibility.Collapsed, second.Visibility);
                    Assert.False(second.IsHitTestVisible);

                    PivotHeadersControl headers = FindHeaders(pivot);
                    Assert.NotNull(headers);
                    Assert.Equal(3, headers.Items.Count);

                    PivotHeaderItem firstHeader = GetHeader(headers, 0);
                    PivotHeaderItem secondHeader = GetHeader(headers, 1);

                    Assert.True(firstHeader.IsSelected);
                    Assert.False(secondHeader.IsSelected);
                    Assert.Equal(0.0, Canvas.GetLeft(firstHeader), 1);
                    Assert.True(
                        Canvas.GetLeft(secondHeader) >= firstHeader.ActualWidth,
                        "The next header has to be laid out to the right of the selected one.");
                    Assert.True(firstHeader.ActualWidth > 0, "The header has to be measured.");
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void Pivot_SelectedIndexSlidesTheIncomingItemAndCompletes()
        {
            StaTest.Run(() =>
            {
                Window window;
                Pivot pivot = CreatePivot(3, out window);
                try
                {
                    PivotItem first = (PivotItem)pivot.Items[0];
                    PivotItem second = (PivotItem)pivot.Items[1];

                    SelectionChangedEventArgs changed = null;
                    pivot.SelectionChanged += (sender, e) => changed = e;

                    double width = pivot.ActualWidth;
                    pivot.SelectedIndex = 1;

                    Assert.Equal(1, pivot.SelectedIndex);
                    Assert.Same(pivot.Items[1], pivot.SelectedItem);
                    Assert.NotNull(changed);
                    Assert.Same(pivot.Items[0], changed.RemovedItems[0]);
                    Assert.Same(pivot.Items[1], changed.AddedItems[0]);

                    Assert.Equal(Visibility.Visible, second.Visibility);
                    Assert.True(pivot.IsAnimating, "Changing the selected index starts a slide.");

                    WpfTestHelper.PumpDispatcher();

                    TranslateTransform incoming = GetTranslation(second);
                    Assert.NotNull(incoming);
                    Assert.True(incoming.X > 0, "The incoming item has to enter from the right; X=" + incoming.X);
                    Assert.True(incoming.X <= width + 1, "The incoming item cannot start further than one viewport away.");

                    Assert.True(WpfTestHelper.PumpUntil(() => !pivot.IsAnimating, 3000), "The slide has to complete.");
                    Assert.Equal(0.0, incoming.X, 1);
                    Assert.Equal(Visibility.Collapsed, first.Visibility);
                    Assert.False(first.IsHitTestVisible);
                    Assert.Equal(0.0, GetTranslation(first).X, 1);

                    // The header run scrolled so that the selected header is at the left edge.
                    PivotHeadersControl headers = FindHeaders(pivot);
                    Assert.True(
                        WpfTestHelper.PumpUntil(() => headers.VisualFirstIndex == 1, 3000),
                        "The header run has to reach the selected header.");
                    Assert.True(WpfTestHelper.PumpUntil(() => Math.Abs(Canvas.GetLeft(GetHeader(headers, 1))) < 1.0, 3000));

                    PivotHeaderItem firstHeader = GetHeader(headers, 0);
                    PivotHeaderItem secondHeader = GetHeader(headers, 1);
                    Assert.False(firstHeader.IsSelected);
                    Assert.True(secondHeader.IsSelected);
                    Assert.True(
                        Canvas.GetLeft(firstHeader) > Canvas.GetLeft(GetHeader(headers, 2)),
                        "The header that scrolled out wraps to the end of the circular run.");
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void Pivot_BackwardSelectionEntersFromTheLeft()
        {
            StaTest.Run(() =>
            {
                Window window;
                Pivot pivot = CreatePivot(3, out window);
                try
                {
                    pivot.SelectedIndex = 2;
                    Assert.True(WpfTestHelper.PumpUntil(() => !pivot.IsAnimating, 3000));

                    PivotItem second = (PivotItem)pivot.Items[1];
                    pivot.SelectedIndex = 1;
                    WpfTestHelper.PumpDispatcher();

                    TranslateTransform incoming = GetTranslation(second);
                    Assert.NotNull(incoming);
                    Assert.True(incoming.X < 0, "Going back has to enter from the left; X=" + incoming.X);

                    Assert.True(WpfTestHelper.PumpUntil(() => !pivot.IsAnimating, 3000));
                    Assert.Equal(0.0, incoming.X, 1);
                    Assert.Equal(Visibility.Visible, second.Visibility);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void Pivot_ClickingAHeaderSelectsItsItem()
        {
            StaTest.Run(() =>
            {
                Window window;
                Pivot pivot = CreatePivot(3, out window);
                try
                {
                    PivotHeadersControl headers = FindHeaders(pivot);
                    PivotHeaderItem last = GetHeader(headers, 2);

                    last.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
                    {
                        RoutedEvent = UIElement.MouseLeftButtonUpEvent
                    });

                    WpfTestHelper.PumpDispatcher();

                    Assert.Equal(2, pivot.SelectedIndex);
                    Assert.Same(pivot.Items[2], pivot.SelectedItem);
                    Assert.True(WpfTestHelper.PumpUntil(() => !pivot.IsAnimating, 3000));
                    Assert.True(
                        WpfTestHelper.PumpUntil(() => headers.VisualFirstIndex == 2, 3000),
                        "The header run has to reach the clicked header.");
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void Pivot_HeaderRunWrapsAroundAfterTheLastHeader()
        {
            StaTest.Run(() =>
            {
                Window window;
                Pivot pivot = CreatePivot(3, out window);
                try
                {
                    pivot.SelectedIndex = 2;
                    Assert.True(WpfTestHelper.PumpUntil(() => !pivot.IsAnimating, 3000));

                    PivotHeadersControl headers = FindHeaders(pivot);
                    Assert.True(WpfTestHelper.PumpUntil(() => headers.VisualFirstIndex == 2, 3000));

                    PivotHeaderItem first = GetHeader(headers, 0);
                    PivotHeaderItem second = GetHeader(headers, 1);
                    PivotHeaderItem last = GetHeader(headers, 2);

                    Assert.Equal(0.0, Canvas.GetLeft(last), 1);

                    // The run is circular: the first header follows the last one.
                    double afterLast = Canvas.GetLeft(last) + last.ActualWidth;
                    Assert.True(
                        Canvas.GetLeft(first) >= afterLast - 1,
                        "The first header has to follow the last one; x=" + Canvas.GetLeft(first));
                    Assert.True(
                        Canvas.GetLeft(second) >= Canvas.GetLeft(first) + first.ActualWidth - 1,
                        "The headers keep their item order after the wrap.");
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void Pivot_KeyboardNavigatesAndWrapsAround()
        {
            StaTest.Run(() =>
            {
                Window window;
                Pivot pivot = CreatePivot(3, out window);
                try
                {
                    RaiseKey(pivot, window, Key.Right);
                    Assert.Equal(1, pivot.SelectedIndex);
                    Assert.True(WpfTestHelper.PumpUntil(() => !pivot.IsAnimating, 3000));

                    RaiseKey(pivot, window, Key.Left);
                    Assert.Equal(0, pivot.SelectedIndex);
                    Assert.True(WpfTestHelper.PumpUntil(() => !pivot.IsAnimating, 3000));

                    RaiseKey(pivot, window, Key.Left);
                    Assert.Equal(2, pivot.SelectedIndex);

                    RaiseKey(pivot, window, Key.Home);
                    Assert.Equal(0, pivot.SelectedIndex);

                    RaiseKey(pivot, window, Key.End);
                    Assert.Equal(2, pivot.SelectedIndex);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void Pivot_IsLockedRefusesProgrammaticNavigationAndFadesHeaders()
        {
            StaTest.Run(() =>
            {
                Window window;
                Pivot pivot = CreatePivot(3, out window);
                try
                {
                    // A navigation stores a local value, which is what the lock guards.
                    pivot.SelectedIndex = 1;
                    Assert.True(WpfTestHelper.PumpUntil(() => !pivot.IsAnimating, 3000));

                    PivotHeadersControl headers = FindHeaders(pivot);
                    PivotHeaderItem header = GetHeader(headers, 1);

                    pivot.IsLocked = true;

                    Assert.True(WpfTestHelper.PumpUntil(() => header.Opacity < 0.01, 2000), "A locked pivot fades its headers out.");
                    Assert.False(header.IsHitTestVisible);

                    pivot.SelectedIndex = 2;
                    Assert.Equal(1, pivot.SelectedIndex);

                    header.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
                    {
                        RoutedEvent = UIElement.MouseLeftButtonUpEvent
                    });
                    WpfTestHelper.PumpDispatcher();
                    Assert.Equal(1, pivot.SelectedIndex);

                    pivot.IsLocked = false;
                    Assert.True(WpfTestHelper.PumpUntil(() => header.Opacity > 0.99, 2000));
                    Assert.True(header.IsHitTestVisible);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void Pivot_RaisesTheItemLifecycleEventsInOrder()
        {
            StaTest.Run(() =>
            {
                Window window;
                Pivot pivot = CreatePivot(3, out window);
                try
                {
                    List<string> events = new List<string>();
                    pivot.UnloadingPivotItem += (sender, e) => events.Add("Unloading:" + ((PivotItem)e.Item).Header);
                    pivot.LoadingPivotItem += (sender, e) => events.Add("Loading:" + ((PivotItem)e.Item).Header);
                    pivot.SelectionChanged += (sender, e) => events.Add("SelectionChanged");
                    pivot.UnloadedPivotItem += (sender, e) => events.Add("Unloaded:" + ((PivotItem)e.Item).Header);
                    pivot.LoadedPivotItem += (sender, e) => events.Add("Loaded:" + ((PivotItem)e.Item).Header);

                    pivot.SelectedIndex = 1;

                    Assert.Equal(
                        new[]
                        {
                            "Unloading:item 0",
                            "Loading:item 1",
                            "SelectionChanged",
                            "Unloaded:item 0",
                            "Loaded:item 1"
                        },
                        events);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void Pivot_ItemsAddedAfterTheTemplateAppears()
        {
            StaTest.Run(() =>
            {
                Pivot pivot = new Pivot();
                WpfTestHelper.ApplyDefaultStyle(pivot);

                Window window = new Window { Width = 480, Height = 800, Content = pivot };
                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();
                    window.UpdateLayout();

                    Assert.Null(pivot.SelectedItem);

                    PivotItem item = new PivotItem { Header = "late", Content = "late content" };
                    pivot.Items.Add(item);

                    Assert.True(WpfTestHelper.PumpUntil(() => pivot.SelectedItem != null, 3000));
                    Assert.Same(item, pivot.SelectedItem);
                    Assert.Equal(Visibility.Visible, item.Visibility);

                    PivotHeadersControl headers = FindHeaders(pivot);
                    Assert.True(WpfTestHelper.PumpUntil(() => headers.Items.Count == 1, 3000));
                    Assert.True(GetHeader(headers, 0).IsSelected);

                    pivot.Items.Add(new PivotItem { Header = "later" });
                    Assert.True(WpfTestHelper.PumpUntil(() => headers.Items.Count == 2, 3000));
                    Assert.Equal(0, pivot.SelectedIndex);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void Pivot_OutOfRangeSelectionIsRejected()
        {
            StaTest.Run(() =>
            {
                Window window;
                Pivot pivot = CreatePivot(2, out window);
                try
                {
                    Assert.Throws<ArgumentException>(() => pivot.SelectedIndex = 5);
                    Assert.Equal(0, pivot.SelectedIndex);

                    Assert.Throws<ArgumentException>(() => pivot.SelectedItem = null);
                    Assert.Same(pivot.Items[0], pivot.SelectedItem);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void Pivot_RendersTheTitleHeadersAndContent()
        {
            StaTest.Run(() =>
            {
                Window window;
                Pivot pivot = CreatePivot(3, out window);
                try
                {
                    pivot.Background = Brushes.Black;
                    pivot.Foreground = Brushes.White;
                    window.UpdateLayout();
                    WpfTestHelper.PumpDispatcher();

                    PivotHeadersControl headers = FindHeaders(pivot);
                    Point headerOrigin = headers.TranslatePoint(new Point(0, 0), pivot);
                    int headerBottom = (int)Math.Ceiling(headerOrigin.Y + headers.ActualHeight);

                    PivotItem first = (PivotItem)pivot.Items[0];
                    Point itemOrigin = first.TranslatePoint(new Point(0, 0), pivot);
                    int contentTop = (int)Math.Floor(itemOrigin.Y);

                    RenderTargetBitmap bitmap = Render(pivot);
                    Assert.True(bitmap.PixelWidth > 400 && bitmap.PixelHeight > 400, "The pivot has to be laid out at the sample size.");

                    Assert.True(
                        CountLightPixels(bitmap, 0, 0, bitmap.PixelWidth, headerBottom) > 50,
                        "The title and the selected header have to be drawn in the header band (0.." + headerBottom + ").");
                    Assert.True(
                        CountLightPixels(bitmap, 0, contentTop, bitmap.PixelWidth, contentTop + 80) > 20,
                        "The selected item's content has to be drawn below the headers (from " + contentTop + ").");
                }
                finally
                {
                    window.Close();
                }
            });
        }

        private static Pivot CreatePivot(int count, out Window window)
        {
            Pivot pivot = new Pivot { Title = "SAMPLE" };
            for (int i = 0; i < count; i++)
            {
                pivot.Items.Add(new PivotItem
                {
                    Header = "item " + i,
                    Content = "content " + i
                });
            }

            WpfTestHelper.ApplyDefaultStyle(pivot);

            window = new Window { Width = 480, Height = 800, Content = pivot };
            window.Show();
            WpfTestHelper.PumpDispatcher();
            window.UpdateLayout();
            WpfTestHelper.PumpDispatcher();

            return pivot;
        }

        private static PivotHeadersControl FindHeaders(Pivot pivot)
        {
            return WpfTestHelper.FindDescendants<PivotHeadersControl>(pivot).FirstOrDefault();
        }

        private static PivotHeaderItem GetHeader(PivotHeadersControl headers, int index)
        {
            PivotHeaderItem header = headers.ItemContainerGenerator.ContainerFromIndex(index) as PivotHeaderItem;
            Assert.NotNull(header);
            return header;
        }

        /// <summary>
        /// The port animates the container through the shared transform
        /// animator, which keeps an existing render transform (WPF reports an
        /// identity <see cref="MatrixTransform"/> for a default control) and adds
        /// the translate to the same group.
        /// </summary>
        private static TranslateTransform GetTranslation(UIElement element)
        {
            return TransformAnimator.GetTranslateTransform(element);
        }

        private static RenderTargetBitmap Render(FrameworkElement element)
        {
            RenderTargetBitmap bitmap = new RenderTargetBitmap(
                Math.Max(1, (int)Math.Ceiling(element.ActualWidth)),
                Math.Max(1, (int)Math.Ceiling(element.ActualHeight)),
                96,
                96,
                PixelFormats.Pbgra32);
            bitmap.Render(element);
            return bitmap;
        }

        private static int CountLightPixels(RenderTargetBitmap bitmap, int left, int top, int right, int bottom)
        {
            int stride = bitmap.PixelWidth * 4;
            byte[] pixels = new byte[stride * bitmap.PixelHeight];
            bitmap.CopyPixels(pixels, stride, 0);

            int count = 0;
            for (int y = Math.Max(0, top); y < Math.Min(bitmap.PixelHeight, bottom); y++)
            {
                for (int x = Math.Max(0, left); x < Math.Min(bitmap.PixelWidth, right); x++)
                {
                    int offset = (y * stride) + (x * 4);
                    if (pixels[offset] > 128 && pixels[offset + 1] > 128 && pixels[offset + 2] > 128)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private static void RaiseKey(Pivot pivot, Window window, Key key)
        {
            PresentationSource source = PresentationSource.FromVisual(window);
            Assert.NotNull(source);

            pivot.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, source, 0, key)
            {
                RoutedEvent = Keyboard.KeyDownEvent
            });
            WpfTestHelper.PumpDispatcher();
        }
    }
}
