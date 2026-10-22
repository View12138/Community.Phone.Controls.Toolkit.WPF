// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace Microsoft.Phone.Shell
{
    /// <summary>
    /// Tracks an <see cref="ApplicationBarIconButton"/> that is rendered as an
    /// icon button in the application bar.
    /// </summary>
    internal class ApplicationBarIconButtonContainer : ApplicationBarItemContainer
    {
        private readonly ApplicationBarIconButton _item;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationBarIconButtonContainer"/> class.
        /// </summary>
        /// <param name="item">The icon button.</param>
        internal ApplicationBarIconButtonContainer(ApplicationBarIconButton item)
            : base(item, ApplicationBarItemType.ImageButton)
        {
            _item = item;
        }

        /// <summary>
        /// Gets the icon button being tracked.
        /// </summary>
        internal ApplicationBarIconButton IconButton
        {
            get { return _item; }
        }

        /// <inheritdoc />
        internal override void ClickEvent()
        {
            _item.ClickEvent();
        }

        /// <inheritdoc />
        protected override void UpdateProperties(object sender, BoolEventArgs args)
        {
            IconUri = _item.IconUri;
            base.UpdateProperties(sender, args);
        }

        /// <inheritdoc />
        protected override void Subscribe()
        {
            _item.Changed += HandleItemChanged;
        }

        /// <inheritdoc />
        protected override void Unsubscribe()
        {
            _item.Changed -= HandleItemChanged;
        }
    }
}
