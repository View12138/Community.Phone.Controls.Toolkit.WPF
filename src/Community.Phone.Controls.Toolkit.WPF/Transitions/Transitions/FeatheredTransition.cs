// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Controls a transition that produces a feathered animation on a set of
    /// elements.
    /// </summary>
    public class FeatheredTransition : Transition
    {
        private readonly TurnstileFeatherTransitionMode _mode;
        private readonly TimeSpan? _beginTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatheredTransition"/> class.
        /// </summary>
        public FeatheredTransition(UIElement element, Storyboard storyboard, TurnstileFeatherTransitionMode mode, TimeSpan? beginTime)
            : base(element, storyboard)
        {
            _mode = mode;
            _beginTime = beginTime;
        }

        /// <inheritdoc />
        public override void Begin()
        {
            TurnstileFeatherEffect.ComposeStoryboard(Storyboard, _beginTime, _mode, Element);
            base.Begin();
        }

        /// <inheritdoc />
        public override void Stop()
        {
            TurnstileFeatherEffect.Cleanup();
            base.Stop();
        }
    }
}
