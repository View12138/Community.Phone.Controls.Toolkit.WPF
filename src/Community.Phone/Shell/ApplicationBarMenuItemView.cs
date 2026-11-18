// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Windows;

namespace Microsoft.Phone.Shell
{
    /// <summary>
    /// Renders an <see cref="ApplicationBarMenuItem"/>: a 48 pixel high text row.
    /// The visuals are the control template in Themes/Controls/ApplicationBar.xaml;
    /// this class only supplies the text.
    /// </summary>
    internal class ApplicationBarMenuItemView : ApplicationBarItemView
    {
        /// <summary>
        /// The height of one menu row.
        /// </summary>
        internal const double RowHeight = 48;

        /// <summary>
        /// The font size of the menu text.
        /// </summary>
        internal const double FontSize = 20;

        /// <summary>
        /// Identifies the <see cref="Text"/> property.
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                "Text",
                typeof(string),
                typeof(ApplicationBarMenuItemView),
                new PropertyMetadata(null));

        static ApplicationBarMenuItemView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ApplicationBarMenuItemView),
                new FrameworkPropertyMetadata(typeof(ApplicationBarMenuItemView)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationBarMenuItemView"/> class.
        /// </summary>
        /// <param name="container">The container that tracks the menu item.</param>
        internal ApplicationBarMenuItemView(ApplicationBarItemContainer container)
            : base(container)
        {
        }

        /// <summary>
        /// Gets or sets the text of the row.
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <inheritdoc />
        protected override void UpdateContent()
        {
            Text = Container.Text;
        }
    }
}
