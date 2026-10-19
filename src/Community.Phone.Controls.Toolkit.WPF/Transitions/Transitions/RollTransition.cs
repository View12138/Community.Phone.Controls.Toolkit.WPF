// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Windows;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Provides roll transitions.
    /// </summary>
    public class RollTransition : TransitionElement
    {
        /// <inheritdoc />
        public override ITransition GetTransition(UIElement element)
        {
            return Transitions.Roll(element);
        }
    }
}
