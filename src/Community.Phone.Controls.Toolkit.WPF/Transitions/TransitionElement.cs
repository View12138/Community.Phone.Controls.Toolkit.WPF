// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Windows;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Transition factory for a particular transition family.
    /// </summary>
    public abstract class TransitionElement : DependencyObject
    {
        /// <summary>
        /// Creates a new <see cref="ITransition"/> for a
        /// <see cref="UIElement"/>.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The transition.</returns>
        public abstract ITransition GetTransition(UIElement element);
    }
}
