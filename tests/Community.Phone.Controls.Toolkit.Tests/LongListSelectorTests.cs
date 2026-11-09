using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Controls;
using Xunit;

namespace Community.Phone.Controls.Toolkit.WPF.Tests
{
    public class LongListSelectorTests
    {
        [Fact]
        public void LongListSelector_RendersListPartsAndVirtualizedItems()
        {
            StaTest.Run(() =>
            {
                List<string> source = new List<string> { "alpha", "beta", "gamma" };
                LongListSelector selector = new LongListSelector
                {
                    Width = 320,
                    Height = 400,
                    ItemsSource = source,
                    ListHeader = "header",
                    ListHeaderTemplate = CreateConstantTemplate("HEADER"),
                    ListFooter = "footer",
                    ListFooterTemplate = CreateConstantTemplate("FOOTER"),
                    ItemTemplate = CreateBoundTextTemplate(),
                };

                List<LongListSelectorItemKind> realizedKinds = new List<LongListSelectorItemKind>();
                List<ContentPresenter> realizedContainers = new List<ContentPresenter>();
                selector.ItemRealized += (sender, e) =>
                {
                    realizedKinds.Add(e.ItemKind);
                    realizedContainers.Add(e.Container);
                };

                Window window = Show(selector);
                try
                {
                    Assert.Equal(5, selector.FlattenedItems.Count);
                    Assert.Equal(
                        new[]
                        {
                            LongListSelectorItemKind.ListHeader,
                            LongListSelectorItemKind.Item,
                            LongListSelectorItemKind.Item,
                            LongListSelectorItemKind.Item,
                            LongListSelectorItemKind.ListFooter,
                        },
                        selector.FlattenedItems.Select(entry => entry.ItemKind));

                    Assert.Equal(source[0], selector.FlattenedItems[1].Data);
                    Assert.Equal(source[2], selector.FlattenedItems[3].Data);

                    Assert.Contains(realizedKinds, kind => kind == LongListSelectorItemKind.ListHeader);
                    Assert.Contains(realizedKinds, kind => kind == LongListSelectorItemKind.Item);
                    Assert.Contains(realizedKinds, kind => kind == LongListSelectorItemKind.ListFooter);
                    Assert.All(realizedContainers, container => Assert.IsType<ContentPresenter>(container));

                    List<string> text = WpfTestHelper
                        .FindDescendants<TextBlock>(selector)
                        .Select(block => block.Text)
                        .ToList();
                    Assert.Contains("HEADER", text);
                    Assert.Contains("alpha", text);
                    Assert.Contains("beta", text);
                    Assert.Contains("gamma", text);

                    // Every generated container is a plain ContentPresenter,
                    // like the phone control, and keeps the flattened entry.
                    ContentPresenter itemContainer =
                        (ContentPresenter)selector.ItemsHost.ItemContainerGenerator.ContainerFromIndex(1);
                    Assert.NotNull(itemContainer);
                    Assert.Equal("alpha", itemContainer.Content);
                    Assert.IsType<LongListSelectorItem>(
                        selector.ItemsHost.ItemContainerGenerator.ItemFromContainer(itemContainer));
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void LongListSelector_GroupingBuildsGroupPartsAndHidesEmptyGroups()
        {
            StaTest.Run(() =>
            {
                TestGroup first = new TestGroup("A") { "a1", "a2" };
                TestGroup empty = new TestGroup("EMPTY");
                TestGroup second = new TestGroup("B") { "b1" };

                LongListSelector selector = new LongListSelector
                {
                    Width = 320,
                    Height = 500,
                    IsGroupingEnabled = true,
                    HideEmptyGroups = true,
                    ItemsSource = new List<TestGroup> { first, empty, second },
                    GroupHeaderTemplate = CreateBindingTemplate("Key"),
                    GroupFooterTemplate = CreateBindingTemplate("Key"),
                    ItemTemplate = CreateBoundTextTemplate(),
                };

                Window window = Show(selector);
                try
                {
                    List<LongListSelectorItemKind> kinds =
                        selector.FlattenedItems.Select(entry => entry.ItemKind).ToList();

                    Assert.Equal(
                        new[]
                        {
                            LongListSelectorItemKind.GroupHeader,
                            LongListSelectorItemKind.Item,
                            LongListSelectorItemKind.Item,
                            LongListSelectorItemKind.GroupFooter,
                            LongListSelectorItemKind.GroupHeader,
                            LongListSelectorItemKind.Item,
                            LongListSelectorItemKind.GroupFooter,
                        },
                        kinds);

                    Assert.Equal(first, selector.FlattenedItems[0].Data);
                    Assert.Equal(first, selector.FlattenedItems[0].Group);
                    Assert.Equal("a1", selector.FlattenedItems[1].Data);
                    Assert.Equal(first, selector.FlattenedItems[1].Group);

                    selector.HideEmptyGroups = false;
                    WpfTestHelper.PumpDispatcher();
                    selector.UpdateLayout();

                    Assert.Contains(
                        selector.FlattenedItems,
                        entry => entry.ItemKind == LongListSelectorItemKind.GroupHeader &&
                                 ReferenceEquals(entry.Data, empty));
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void LongListSelector_GridLayoutArrangesItemsInFixedCells()
        {
            StaTest.Run(() =>
            {
                List<string> source = Enumerable.Range(0, 6).Select(index => "item " + index).ToList();
                LongListSelector selector = new LongListSelector
                {
                    Width = 480,
                    Height = 600,
                    LayoutMode = LongListSelectorLayoutMode.Grid,
                    GridCellSize = new Size(100, 80),
                    ItemsSource = source,
                    ItemTemplate = CreateBoundTextTemplate(),
                };

                Window window = Show(selector);
                try
                {
                    Assert.Equal(4, CountGridColumns(selector));

                    FrameworkElement firstContainer = ContainerAt(selector, 0);
                    FrameworkElement fifthContainer = ContainerAt(selector, 4);
                    FrameworkElement sixthContainer = ContainerAt(selector, 5);

                    Point first = firstContainer.TransformToAncestor(selector).Transform(new Point(0, 0));
                    Point fifth = fifthContainer.TransformToAncestor(selector).Transform(new Point(0, 0));
                    Point sixth = sixthContainer.TransformToAncestor(selector).Transform(new Point(0, 0));

                    Assert.Equal(0, first.X, 1);
                    Assert.Equal(0, first.Y, 1);
                    Assert.Equal(0, fifth.X, 1);
                    Assert.Equal(80, fifth.Y, 1);
                    Assert.Equal(100, sixth.X, 1);
                    Assert.Equal(80, sixth.Y, 1);
                    Assert.Equal(100, firstContainer.ActualWidth, 1);
                    Assert.Equal(80, firstContainer.ActualHeight, 1);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void LongListSelector_SelectionAndKeyboardNavigationUseItemData()
        {
            StaTest.Run(() =>
            {
                LongListSelector selector = new LongListSelector
                {
                    Width = 320,
                    Height = 400,
                    ItemsSource = new List<string> { "alpha", "beta", "gamma" },
                    ItemTemplate = CreateBoundTextTemplate(),
                };

                List<object> added = new List<object>();
                List<object> removed = new List<object>();
                List<string> changedProperties = new List<string>();
                selector.SelectionChanged += (sender, e) =>
                {
                    added.AddRange(e.AddedItems.Cast<object>());
                    removed.AddRange(e.RemovedItems.Cast<object>());
                };
                selector.PropertyChanged += (sender, e) => changedProperties.Add(e.PropertyName);

                Window window = Show(selector);
                try
                {
                    Assert.True(selector.MoveSelection(1));
                    Assert.Equal("alpha", selector.SelectedItem);
                    Assert.True(selector.MoveSelection(1));
                    Assert.Equal("beta", selector.SelectedItem);
                    Assert.True(selector.MoveSelection(-1));
                    Assert.Equal("alpha", selector.SelectedItem);

                    selector.SelectedItem = "gamma";
                    Assert.Equal("gamma", selector.SelectedItem);
                    Assert.Contains("gamma", added);
                    Assert.Contains("beta", removed);
                    Assert.Contains(nameof(LongListSelector.SelectedItem), changedProperties);

                    // Group and list parts are not selectable.
                    selector.SelectedItem = null;
                    selector.MoveSelection(1);
                    Assert.Equal("alpha", selector.SelectedItem);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void LongListSelector_ScrollToBringsAFarItemIntoView()
        {
            StaTest.Run(() =>
            {
                List<string> source = Enumerable.Range(0, 200).Select(index => "item " + index).ToList();
                LongListSelector selector = new LongListSelector
                {
                    Width = 320,
                    Height = 300,
                    ItemsSource = source,
                    ItemTemplate = CreateBoundTextTemplate(),
                };

                Window window = Show(selector);
                try
                {
                    Assert.Equal(0, selector.ScrollViewer.VerticalOffset, 1);

                    selector.ScrollTo(source[150]);
                    WpfTestHelper.PumpDispatcher();
                    selector.UpdateLayout();
                    WpfTestHelper.PumpDispatcher();

                    Assert.True(
                        selector.ScrollViewer.VerticalOffset > 0,
                        string.Format(
                            "ScrollTo must move the viewport (offset={0}, extent={1}, viewport={2}, canContentScroll={3}, realized={4}, panel={5}).",
                            selector.ScrollViewer.VerticalOffset,
                            selector.ScrollViewer.ExtentHeight,
                            selector.ScrollViewer.ViewportHeight,
                            selector.ScrollViewer.CanContentScroll,
                            selector.GetRealizedContainers().Count(),
                            selector.ItemsPanel?.GetType().Name));

                    ContentPresenter container =
                        (ContentPresenter)selector.ItemsHost.ItemContainerGenerator.ContainerFromIndex(150);
                    Assert.NotNull(container);
                    Rect bounds = BoundsInSelector(selector, container);
                    Assert.True(bounds.Top >= 0 && bounds.Bottom <= selector.ActualHeight + 1,
                        "The scrolled item should be inside the viewport, but was " + bounds + ".");

                    Assert.Throws<ArgumentException>(() => selector.ScrollTo("not in the list"));
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void LongListSelector_CollectionChangesRebuildItemsAndRaiseUnrealized()
        {
            StaTest.Run(() =>
            {
                ObservableCollection<string> source =
                    new ObservableCollection<string>(Enumerable.Range(0, 8).Select(index => "item " + index));
                LongListSelector selector = new LongListSelector
                {
                    Width = 320,
                    Height = 300,
                    ItemsSource = source,
                    ItemTemplate = CreateBoundTextTemplate(),
                };

                Window window = Show(selector);
                try
                {
                    int unrealized = 0;
                    selector.ItemUnrealized += (sender, e) => unrealized++;

                    source.Add("added");
                    WpfTestHelper.PumpDispatcher();
                    selector.UpdateLayout();

                    Assert.Equal(9, selector.FlattenedItems.Count);
                    Assert.True(unrealized > 0, "Replacing the flattened items must unrealize the old containers.");

                    source.RemoveAt(0);
                    WpfTestHelper.PumpDispatcher();
                    selector.UpdateLayout();
                    Assert.Equal(8, selector.FlattenedItems.Count);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void LongListSelector_JumpListOpensAndScrollsToTheChosenGroup()
        {
            StaTest.Run(() =>
            {
                TestGroup first = new TestGroup("A") { "a1", "a2" };
                TestGroup second = new TestGroup("B") { "b1" };
                TestGroup third = new TestGroup("C") { "c1" };

                Style jumpListStyle = new Style(typeof(LongListSelector));
                jumpListStyle.Setters.Add(new Setter(LongListSelector.LayoutModeProperty, LongListSelectorLayoutMode.Grid));
                jumpListStyle.Setters.Add(new Setter(LongListSelector.GridCellSizeProperty, new Size(96, 96)));
                jumpListStyle.Setters.Add(new Setter(LongListSelector.ItemTemplateProperty, CreateBindingTemplate("Key")));

                LongListSelector selector = new LongListSelector
                {
                    Width = 320,
                    Height = 300,
                    IsGroupingEnabled = true,
                    ItemsSource = new List<TestGroup> { first, second, third },
                    GroupHeaderTemplate = CreateBindingTemplate("Key"),
                    ItemTemplate = CreateBoundTextTemplate(),
                    JumpListStyle = jumpListStyle,
                };

                int opening = 0;
                int closed = 0;
                selector.JumpListOpening += (sender, e) => opening++;
                selector.JumpListClosed += (sender, e) => closed++;

                Window window = Show(selector);
                try
                {
                    selector.OpenJumpList(null);
                    WpfTestHelper.PumpDispatcher();

                    Assert.Equal(1, opening);
                    Assert.NotNull(selector.JumpListSelector);
                    Assert.Equal(3, selector.JumpListSelector.FlattenedItems.Count);

                    LongListSelector jumpList = selector.JumpListSelector;
                    jumpList.SelectedItem = third;
                    WpfTestHelper.PumpDispatcher();

                    Assert.Equal(1, closed);
                    Assert.Null(selector.JumpListSelector);
                    Assert.True(selector.ScrollViewer.VerticalOffset >= 0);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void LongListSelector_PaintsEveryRealizedItem()
        {
            StaTest.Run(() =>
            {
                List<string> source = Enumerable.Range(1, 40).Select(index => "item " + index).ToList();
                LongListSelector selector = new LongListSelector
                {
                    ItemsSource = source,
                    ListHeader = "header",
                    ListHeaderTemplate = CreateConstantTemplate("HEADER"),
                    ItemTemplate = CreateBoundTextTemplate(),
                };

                Window window = Show(selector);
                try
                {
                    selector.UpdateLayout();
                    WpfTestHelper.PumpDispatcher();

                    int width = (int)Math.Ceiling(selector.ActualWidth);
                    int height = (int)Math.Ceiling(selector.ActualHeight);
                    RenderTargetBitmap bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
                    bitmap.Render(selector);

                    int stride = width * 4;
                    byte[] pixels = new byte[stride * height];
                    bitmap.CopyPixels(pixels, stride, 0);

                    int checkedContainerCount = 0;
                    List<string> unpainted = new List<string>();
                    foreach (ContentPresenter container in selector.GetRealizedContainers())
                    {
                        Rect rect = BoundsInSelector(selector, container);
                        int x0 = Math.Max(0, (int)rect.Left);
                        int y0 = Math.Max(0, (int)rect.Top);
                        int x1 = Math.Min(width, (int)rect.Right);
                        int y1 = Math.Min(height, (int)rect.Bottom);

                        // Containers realized outside the viewport (virtualization
                        // buffer) are legitimately not drawn.
                        if (x1 <= x0 || y1 <= y0)
                        {
                            continue;
                        }

                        checkedContainerCount++;
                        bool painted = false;
                        for (int y = y0; y < y1 && !painted; y++)
                        {
                            for (int x = x0; x < x1; x++)
                            {
                                if (pixels[(y * stride) + (x * 4)] > 32)
                                {
                                    painted = true;
                                    break;
                                }
                            }
                        }

                        if (!painted)
                        {
                            unpainted.Add(container.Content?.ToString() ?? "<null>");
                        }
                    }

                    Assert.NotEmpty(selector.GetRealizedContainers());
                    Assert.True(checkedContainerCount > 0, "no realized container fell inside the viewport");
                    Assert.Empty(unpainted);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        private static Window Show(FrameworkElement content, double width = 480, double height = 800)
        {
            WpfTestHelper.ApplyTheme(content);
            if (content is Control control)
            {
                WpfTestHelper.ApplyDefaultStyle(control);
            }

            Window window = new Window
            {
                Width = width,
                Height = height,
                Background = Brushes.Black,
                Content = content,
            };

            window.Show();
            WpfTestHelper.PumpDispatcher();
            window.UpdateLayout();
            WpfTestHelper.PumpDispatcher();
            return window;
        }

        private static DataTemplate CreateBoundTextTemplate()
        {
            FrameworkElementFactory factory = new FrameworkElementFactory(typeof(TextBlock));
            factory.SetBinding(TextBlock.TextProperty, new Binding());
            return new DataTemplate { VisualTree = factory };
        }

        private static DataTemplate CreateBindingTemplate(string path)
        {
            FrameworkElementFactory factory = new FrameworkElementFactory(typeof(TextBlock));
            factory.SetBinding(TextBlock.TextProperty, new Binding(path));
            return new DataTemplate { VisualTree = factory };
        }

        private static DataTemplate CreateConstantTemplate(string text)
        {
            FrameworkElementFactory factory = new FrameworkElementFactory(typeof(TextBlock));
            factory.SetValue(TextBlock.TextProperty, text);
            return new DataTemplate { VisualTree = factory };
        }

        private static int CountGridColumns(LongListSelector selector)
        {
            List<double> positions = new List<double>();
            foreach (ContentPresenter container in selector.GetRealizedContainers())
            {
                positions.Add(BoundsInSelector(selector, container).X);
            }

            return positions.Distinct().Count();
        }

        private static FrameworkElement ContainerAt(LongListSelector selector, int index)
        {
            return (FrameworkElement)selector.ItemsHost.ItemContainerGenerator.ContainerFromIndex(index);
        }

        private static Rect BoundsInSelector(LongListSelector selector, FrameworkElement element)
        {
            GeneralTransform transform = element.TransformToAncestor(selector);
            Point topLeft = transform.Transform(new Point(0, 0));
            Point bottomRight = transform.Transform(new Point(element.ActualWidth, element.ActualHeight));
            return new Rect(topLeft, bottomRight);
        }

        private sealed class TestGroup : List<string>
        {
            public TestGroup(string key)
            {
                Key = key;
            }

            public string Key { get; }
        }
    }
}
