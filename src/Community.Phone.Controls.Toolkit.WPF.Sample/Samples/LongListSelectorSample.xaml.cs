using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;

namespace PhoneToolkitSample.Samples
{
    public partial class LongListSelectorSample : PhoneApplicationPage
    {
        public LongListSelectorSample()
        {
            InitializeComponent();
            SampleList.ItemsSource = CreateGroups();
            UpdateStatus();
        }

        private static List<SampleGroup> CreateGroups()
        {
            List<SampleGroup> groups = new List<SampleGroup>
            {
                new SampleGroup("A", "Amsterdam", "Athens", "Auckland"),
                new SampleGroup("B", "Berlin", "Boston", "Brussels", "Budapest"),
                // Deliberately empty: HideEmptyGroups removes it from the list
                // and from the jump list.
                new SampleGroup("C"),
                new SampleGroup("D", "Delhi", "Dublin"),
                new SampleGroup("E", "Edinburgh", "El Paso"),
                new SampleGroup("F", "Florence", "Frankfurt"),
            };

            return groups;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateStatus();
        }

        private void OnJumpListOpening(object sender, EventArgs e)
        {
            StatusText.Text = "Jump list opening";
        }

        private void OnJumpListClosed(object sender, EventArgs e)
        {
            UpdateStatus();
        }

        private void OnToggleLayout(object sender, RoutedEventArgs e)
        {
            SampleList.LayoutMode = SampleList.LayoutMode == LongListSelectorLayoutMode.List
                ? LongListSelectorLayoutMode.Grid
                : LongListSelectorLayoutMode.List;
            SampleList.GridCellSize = new Size(200, 64);
            UpdateStatus();
        }

        private void OnGoBack(object sender, RoutedEventArgs e)
        {
            SampleNavigator.GoBack(this, TransitionKind.Turnstile);
        }

        private void UpdateStatus()
        {
            if (StatusText == null || SampleList == null)
            {
                return;
            }

            int groupCount = SampleList.ItemsSource == null ? 0 : SampleList.ItemsSource.Count;
            StatusText.Text = string.Format(
                "{0} · {1} groups · selected: {2}",
                SampleList.LayoutMode == LongListSelectorLayoutMode.Grid ? "grid" : "list",
                groupCount,
                SampleList.SelectedItem ?? "none");
        }

        private sealed class SampleGroup : List<string>
        {
            public SampleGroup(string key, params string[] items)
                : base(items)
            {
                Key = key;
            }

            public string Key { get; }
        }
    }
}
