// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Provides attached-property access to a <see cref="Projection"/> for
    /// elements hosted in a <see cref="ProjectionHost"/>.
    /// </summary>
    /// <remarks>
    /// Setting this property on a <see cref="ProjectionHost"/> applies the
    /// projection directly. Setting it on a descendant applies the projection
    /// to the nearest <see cref="ProjectionHost"/> ancestor, which enables
    /// storyboard animation using the familiar
    /// <c>(UIElement.Projection).(PlaneProjection.RotationY)</c> style
    /// property path.
    /// </remarks>
    public static class ProjectionService
    {
        #region DependencyProperty Projection
        /// <summary>
        /// Gets the projection attached to an element.
        /// </summary>
        public static Projection GetProjection(DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            if (element is ProjectionHost host)
            {
                if (host.Projection != null)
                {
                    return host.Projection;
                }
            }

            return (Projection)element.GetValue(ProjectionProperty);
        }

        /// <summary>
        /// Sets the projection attached to an element.
        /// </summary>
        public static void SetProjection(DependencyObject element, Projection value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.SetValue(ProjectionProperty, value);
        }

        /// <summary>
        /// Identifies the Projection attached property.
        /// </summary>
        public static readonly DependencyProperty ProjectionProperty =
            DependencyProperty.RegisterAttached("Projection", typeof(Projection), typeof(ProjectionService), new PropertyMetadata(null, OnProjectionChanged));

        private static void OnProjectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ProjectionHost host = GetProjectionHost(d);
            if (host != null)
            {
                host.Projection = (Projection)e.NewValue;
                return;
            }

            // The element may not be connected to the tree yet. Retry once it
            // is loaded so that XAML declarations on page content still work.
            if (d is FrameworkElement element)
            {
                element.Loaded -= OnElementLoaded;
                element.Loaded += OnElementLoaded;
            }
        }
        #endregion

        private static void OnElementLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                element.Loaded -= OnElementLoaded;

                ProjectionHost host = GetProjectionHost(element);
                host?.Projection = (Projection)element.GetValue(ProjectionProperty);
            }
        }

        /// <summary>
        /// Finds the nearest <see cref="ProjectionHost"/> ancestor of an
        /// element, or the element itself when it is a host.
        /// </summary>
        public static ProjectionHost GetProjectionHost(DependencyObject element)
        {
            if (element is ProjectionHost host)
            {
                return host;
            }

            DependencyObject current = element;
            while (current != null)
            {
                if (current is ProjectionHost _host)
                {
                    return _host;
                }

                current = GetParent(current);
            }

            return null;
        }

        private static DependencyObject GetParent(DependencyObject element)
        {
            if (element is FrameworkElement frameworkElement)
            {
                return frameworkElement.Parent ?? frameworkElement.TemplatedParent;
            }

            return VisualTreeHelper.GetParent(element);
        }

        private static readonly ConditionalWeakTable<FrameworkElement, HostInfo> HostInfos =
            new ConditionalWeakTable<FrameworkElement, HostInfo>();

        /// <summary>
        /// Ensures that an element is hosted in a <see cref="ProjectionHost"/>.
        /// If the element is already hosted, the existing host is returned. If
        /// the element is a direct child of a panel, it is temporarily wrapped
        /// in a host, preserving layout properties.
        /// </summary>
        /// <remarks>
        /// Item containers (such as <see cref="ListBoxItem"/>) are not wrapped
        /// because replacing them can break item container generation and
        /// virtualization. Callers should place such content inside a
        /// <see cref="ProjectionHost"/> explicitly.
        /// </remarks>
        public static ProjectionHost EnsureProjectionHost(FrameworkElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            // Only a host that directly contains the element can project it.
            // Falling back to the nearest ancestor host would apply an
            // element effect (tilt, feathering) to the whole page, because
            // pages are hosted by the transition frame's presenters.
            ProjectionHost existing = FindDirectHost(element);
            if (existing != null)
            {
                return existing;
            }

            if (element is ListBoxItem || element is ContentPresenter)
            {
                return null;
            }

            if (element.Parent is not Panel panel)
            {
                return null;
            }

            int index = panel.Children.IndexOf(element);
            if (index < 0)
            {
                return null;
            }

            HostInfo info = new HostInfo
            {
                Parent = panel,
                Index = index,
                Width = element.Width,
                Height = element.Height,
                Margin = element.Margin,
                HorizontalAlignment = element.HorizontalAlignment,
                VerticalAlignment = element.VerticalAlignment,
                MinWidth = element.MinWidth,
                MinHeight = element.MinHeight,
                MaxWidth = element.MaxWidth,
                MaxHeight = element.MaxHeight,
                GridRow = Grid.GetRow(element),
                GridColumn = Grid.GetColumn(element),
                GridRowSpan = Grid.GetRowSpan(element),
                GridColumnSpan = Grid.GetColumnSpan(element),
                ZIndex = Panel.GetZIndex(element),
                Dock = DockPanel.GetDock(element),
                CanvasLeft = Canvas.GetLeft(element),
                CanvasTop = Canvas.GetTop(element)
            };

            ProjectionHost host = new ProjectionHost
            {
                Width = info.Width,
                Height = info.Height,
                Margin = info.Margin,
                HorizontalAlignment = info.HorizontalAlignment,
                VerticalAlignment = info.VerticalAlignment,
                MinWidth = info.MinWidth,
                MinHeight = info.MinHeight,
                MaxWidth = info.MaxWidth,
                MaxHeight = info.MaxHeight
            };

            Grid.SetRow(host, info.GridRow);
            Grid.SetColumn(host, info.GridColumn);
            Grid.SetRowSpan(host, info.GridRowSpan);
            Grid.SetColumnSpan(host, info.GridColumnSpan);
            Panel.SetZIndex(host, info.ZIndex);
            DockPanel.SetDock(host, info.Dock);
            if (!double.IsNaN(info.CanvasLeft))
            {
                Canvas.SetLeft(host, info.CanvasLeft);
            }

            if (!double.IsNaN(info.CanvasTop))
            {
                Canvas.SetTop(host, info.CanvasTop);
            }

            element.Width = double.NaN;
            element.Height = double.NaN;
            element.Margin = new Thickness(0);
            element.HorizontalAlignment = HorizontalAlignment.Stretch;
            element.VerticalAlignment = VerticalAlignment.Stretch;
            element.MinWidth = 0;
            element.MinHeight = 0;
            element.MaxWidth = double.PositiveInfinity;
            element.MaxHeight = double.PositiveInfinity;

            panel.Children.RemoveAt(index);
            panel.Children.Insert(index, host);
            host.Child = element;
            info.Host = host;
            HostInfos.Add(element, info);
            return host;
        }

        private static ProjectionHost FindDirectHost(FrameworkElement element)
        {
            if (HostInfos.TryGetValue(element, out HostInfo info))
            {
                return info.Host;
            }

            DependencyObject parent = element.Parent;
            parent ??= VisualTreeHelper.GetParent(element);

            if (parent is ProjectionHost host && ReferenceEquals(host.Child, element))
            {
                return host;
            }

            return null;
        }

        /// <summary>
        /// Removes a host inserted by <see cref="EnsureProjectionHost"/> and
        /// restores the original layout properties of the element.
        /// </summary>
        public static void ReleaseProjectionHost(FrameworkElement element)
        {
            if (element == null)
            {
                return;
            }

            if (!HostInfos.TryGetValue(element, out HostInfo info))
            {
                return;
            }

            HostInfos.Remove(element);

            ProjectionHost host = info.Host;
            host.Child = null;

            int currentIndex = info.Parent.Children.IndexOf(host);
            if (currentIndex >= 0)
            {
                info.Parent.Children.RemoveAt(currentIndex);
            }

            element.Width = info.Width;
            element.Height = info.Height;
            element.Margin = info.Margin;
            element.HorizontalAlignment = info.HorizontalAlignment;
            element.VerticalAlignment = info.VerticalAlignment;
            element.MinWidth = info.MinWidth;
            element.MinHeight = info.MinHeight;
            element.MaxWidth = info.MaxWidth;
            element.MaxHeight = info.MaxHeight;
            Grid.SetRow(element, info.GridRow);
            Grid.SetColumn(element, info.GridColumn);
            Grid.SetRowSpan(element, info.GridRowSpan);
            Grid.SetColumnSpan(element, info.GridColumnSpan);
            Panel.SetZIndex(element, info.ZIndex);
            DockPanel.SetDock(element, info.Dock);
            Canvas.SetLeft(element, info.CanvasLeft);
            Canvas.SetTop(element, info.CanvasTop);

            info.Parent.Children.Insert(Math.Min(info.Index, info.Parent.Children.Count), element);
        }

        private sealed class HostInfo
        {
            public Panel Parent;
            public int Index;
            public double Width;
            public double Height;
            public Thickness Margin;
            public HorizontalAlignment HorizontalAlignment;
            public VerticalAlignment VerticalAlignment;
            public double MinWidth;
            public double MinHeight;
            public double MaxWidth;
            public double MaxHeight;
            public int GridRow;
            public int GridColumn;
            public int GridRowSpan;
            public int GridColumnSpan;
            public int ZIndex;
            public Dock Dock;
            public double CanvasLeft;
            public double CanvasTop;
            public ProjectionHost Host;
        }
    }
}
