// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows.Media.Animation;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Controls the behavior of transitions.
    /// </summary>
    public interface ITransition
    {
        /// <summary>
        /// Occurs when the transition has completed playing.
        /// </summary>
        event EventHandler Completed;

        /// <summary>
        /// Gets the <see cref="ClockState"/> of the transition.
        /// </summary>
        ClockState GetCurrentState();

        /// <summary>
        /// Gets the current time of the transition.
        /// </summary>
        TimeSpan GetCurrentTime();

        /// <summary>
        /// Pauses the animation clock.
        /// </summary>
        void Pause();

        /// <summary>
        /// Resumes the animation clock.
        /// </summary>
        void Resume();

        /// <summary>
        /// Moves the transition to the specified animation position.
        /// </summary>
        void Seek(TimeSpan offset);

        /// <summary>
        /// Moves the transition to the specified animation position
        /// synchronously.
        /// </summary>
        void SeekAlignedToLastTick(TimeSpan offset);

        /// <summary>
        /// Advances the current time to the end of the active period.
        /// </summary>
        void SkipToFill();

        /// <summary>
        /// Starts the transition.
        /// </summary>
        void Begin();

        /// <summary>
        /// Stops the transition and restores the animated element state.
        /// </summary>
        void Stop();
    }
}
