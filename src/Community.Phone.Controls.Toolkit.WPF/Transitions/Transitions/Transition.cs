// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Mirrors the
    /// <see cref="T:System.Windows.Media.Animation.Storyboard"/>
    /// interface to control an
    /// <see cref="T:Microsoft.Phone.Controls.ITransition"/>
    /// for a
    /// <see cref="T:System.Windows.UIElement"/>.
    /// Saves and restores the
    /// <see cref="P:System.Windows.UIElement.CacheMode"/>
    /// and
    /// <see cref="P:System.Windows.UIElement.IsHitTestVisible"/>
    /// values for the
    /// <see cref="T:System.Windows.UIElement"/>.
    /// </summary>
    public class Transition : ITransition
    {
        /// <summary>
        /// The original
        /// <see cref="P:System.Windows.UIElement.CacheMode"/>
        /// of the
        /// <see cref="T:System.Windows.UIElement"/>.
        /// </summary>
        private CacheMode _cacheMode;

        /// <summary>
        /// The <see cref="T:System.Windows.UIElement"/>
        /// that the transition will be applied to.
        /// </summary>
        protected UIElement _element;

        /// <summary>
        /// The original
        /// <see cref="P:System.Windows.UIElement.IsHitTestVisible"/>
        /// of the
        /// <see cref="T:System.Windows.UIElement"/>.
        /// </summary>
        private bool _isHitTestVisible;

        /// <summary>
        /// A value indicating whether the storyboard has been started and has
        /// not completed or been stopped yet.
        /// </summary>
        private bool _isBegun;

        /// <summary>
        /// A value indicating whether the
        /// <see cref="E:Microsoft.Phone.Controls.Transition.Completed"/>
        /// event has already been raised for the current run.
        /// </summary>
        private bool _isCompleted;

        /// <summary>
        /// The
        /// <see cref="T:System.Windows.Media.Animation.Storyboard"/>
        /// for the
        /// <see cref="T:System.Windows.UIElement"/>.
        /// </summary>
        private Storyboard _storyboard;

        /// <summary>
        /// The property that identifies the
        /// <see cref="T:System.Windows.Media.Animation.Storyboard"/>
        /// for the
        /// <see cref="T:System.Windows.UIElement"/>.
        /// </summary>
        protected Storyboard Storyboard
        {
            get { return _storyboard; }
            set
            {
                if (value != _storyboard)
                {
                    if (_storyboard != null)
                    {
                        _storyboard.Completed -= OnCompleted;
                    }

                    _storyboard = value;

                    if (_storyboard != null)
                    {
                        _storyboard.Completed += OnCompleted;
                    }
                }
            }
        }

        /// <summary>
        /// Mirrors <see cref="E:System.Windows.Media.Animation.Storyboard.Completed"/>.
        /// </summary>
        public event EventHandler Completed;

        /// <summary>
        /// Constructs a
        /// <see cref="T:Microsoft.Phone.Controls.Transition"/>
        /// for a
        /// <see cref="T:System.Windows.UIElement"/>
        /// and a
        /// <see cref="T:System.Windows.Media.Animation.Storyboard"/>.
        /// </summary>
        /// <param name="element">The <see cref="T:System.Windows.UIElement"/>.</param>
        /// <param name="storyboard">The <see cref="T:System.Windows.Media.Animation.Storyboard"/>.</param>
        public Transition(UIElement element, Storyboard storyboard)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (storyboard == null)
            {
                throw new ArgumentNullException("storyboard");
            }
            _element = element;
            Storyboard = storyboard;
            Storyboard.Completed += OnCompletedRestore;
        }

        /// <summary>
        /// The element as the containing object of the storyboard, which WPF
        /// needs for controllable clocks. Silverlight needed none; an element
        /// that is not a framework element falls back to the parameterless
        /// storyboard methods.
        /// </summary>
        private FrameworkElement ContainingObject
        {
            get { return _element as FrameworkElement; }
        }

        /// <summary>
        /// Mirrors <see cref="M:System.Windows.Media.Animation.Storyboard.Begin"/>.
        /// </summary>
        public virtual void Begin()
        {
            if (_isBegun)
            {
                // A restart must not report the previous run as completed.
                _isCompleted = true;
                StopStoryboard();
            }

            Save();

            _isCompleted = false;

            // Started as a controllable clock so that Pause/Resume/Seek/
            // SkipToFill keep working, which Silverlight allowed after a plain
            // Begin().
            FrameworkElement containingObject = ContainingObject;
            if (containingObject != null)
            {
                Storyboard.Begin(containingObject, true);
            }
            else
            {
                Storyboard.Begin();
            }

            _isBegun = true;
        }

        /// <summary>
        /// Restores the settings for the transition.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCompletedRestore(object sender, EventArgs e)
        {
            _isBegun = false;
            Restore();
        }

        /// <summary>
        /// Mirrors <see cref="M:System.Windows.Media.Animation.Storyboard.GetCurrentState"/>.
        /// </summary>
        public ClockState GetCurrentState()
        {
            // WPF throws "Cannot perform this operation because the specified
            // Storyboard was not applied to this object for interactive
            // control" for a storyboard that has no clock yet, and reports
            // Stopped until the clock's first tick. Silverlight returned
            // Stopped before Begin and Active as soon as it was running, so
            // the begun/completed state is tracked here instead of querying a
            // clock that may not exist.
            return _isBegun ? ClockState.Active : ClockState.Stopped;
        }

        /// <summary>
        /// Mirrors <see cref="M:System.Windows.Media.Animation.Storyboard.GetCurrentTime"/>.
        /// </summary>
        public TimeSpan GetCurrentTime()
        {
            if (!_isBegun)
            {
                return TimeSpan.Zero;
            }

            FrameworkElement containingObject = ContainingObject;
            TimeSpan? currentTime = containingObject == null
                ? Storyboard.GetCurrentTime()
                : Storyboard.GetCurrentTime(containingObject);
            return currentTime ?? TimeSpan.Zero;
        }

        /// <summary>
        /// Mirrors <see cref="M:System.Windows.Media.Animation.Storyboard.Pause"/>.
        /// </summary>
        public void Pause()
        {
            if (!_isBegun)
            {
                return;
            }

            FrameworkElement containingObject = ContainingObject;
            if (containingObject == null)
            {
                Storyboard.Pause();
            }
            else
            {
                Storyboard.Pause(containingObject);
            }
        }

        /// <summary>
        /// Restores the saved
        /// <see cref="P:System.Windows.UIElement.CacheMode"/>
        /// and
        /// <see cref="P:System.Windows.UIElement.IsHitTestVisible"/>
        /// values for the
        /// <see cref="T:System.Windows.UIElement"/>.
        /// </summary>
        private void Restore()
        {
            if (!(_cacheMode is BitmapCache))
            {
                _element.CacheMode = _cacheMode;
            }
            if (_isHitTestVisible)
            {
                _element.IsHitTestVisible = _isHitTestVisible;
            }
            else
            {
                // This is resolving a bug where the new page cannot be used.
                // This may regress some scenarios for unsupported uses of the
                // transitions.
                _element.IsHitTestVisible = true;
            }
        }

        /// <summary>
        /// Mirrors <see cref="M:System.Windows.Media.Animation.Storyboard.Resume"/>.
        /// </summary>
        public void Resume()
        {
            if (!_isBegun)
            {
                return;
            }

            FrameworkElement containingObject = ContainingObject;
            if (containingObject == null)
            {
                Storyboard.Resume();
            }
            else
            {
                Storyboard.Resume(containingObject);
            }
        }

        /// <summary>
        /// Saves the
        /// <see cref="P:System.Windows.UIElement.CacheMode"/>
        /// and
        /// <see cref="P:System.Windows.UIElement.IsHitTestVisible"/>
        /// values for the
        /// <see cref="T:System.Windows.UIElement"/>.
        /// </summary>
        private void Save()
        {
            _cacheMode = _element.CacheMode;
            if (!(_cacheMode is BitmapCache))
            {
                // A Freezable belongs to the dispatcher that created it, so a
                // transition creates its own cache instead of sharing a
                // static instance across threads and elements.
                _element.CacheMode = new BitmapCache();
            }
            _isHitTestVisible = _element.IsHitTestVisible;
            if (_isHitTestVisible)
            {
                _element.IsHitTestVisible = false;
            }
        }

        /// <summary>
        /// Mirrors <see cref="E:System.Windows.Media.Animation.Storyboard.Completed"/>.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnCompleted(object sender, EventArgs e)
        {
            // WPF raises Completed again when a controllable clock is stopped
            // or removed, so a run reports completion at most once: the frame
            // completes the navigation from this event.
            if (_isCompleted)
            {
                return;
            }

            _isCompleted = true;

            EventHandler handler = Completed;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Mirrors <see cref="M:System.Windows.Media.Animation.Storyboard.Seek"/>.
        /// </summary>
        /// <param name="offset">The time offset.</param>
        public void Seek(TimeSpan offset)
        {
            if (!_isBegun)
            {
                return;
            }

            FrameworkElement containingObject = ContainingObject;
            if (containingObject == null)
            {
                Storyboard.Seek(offset);
            }
            else
            {
                Storyboard.Seek(containingObject, offset, TimeSeekOrigin.BeginTime);
            }
        }

        /// <summary>
        /// Mirrors <see cref="M:System.Windows.Media.Animation.Storyboard.SeekAlignedToLastTick"/>.
        /// </summary>
        /// <param name="offset">The time offset.</param>
        public void SeekAlignedToLastTick(TimeSpan offset)
        {
            if (!_isBegun)
            {
                return;
            }

            FrameworkElement containingObject = ContainingObject;
            if (containingObject == null)
            {
                Storyboard.SeekAlignedToLastTick(offset);
            }
            else
            {
                Storyboard.SeekAlignedToLastTick(containingObject, offset, TimeSeekOrigin.BeginTime);
            }
        }

        /// <summary>
        /// Mirrors <see cref="M:System.Windows.Media.Animation.Storyboard.SkipToFill"/>.
        /// </summary>
        public void SkipToFill()
        {
            if (!_isBegun)
            {
                return;
            }

            FrameworkElement containingObject = ContainingObject;
            if (containingObject == null)
            {
                Storyboard.SkipToFill();
            }
            else
            {
                Storyboard.SkipToFill(containingObject);
            }
        }

        /// <summary>
        /// Mirrors <see cref="M:System.Windows.Media.Animation.Storyboard.Stop"/>.
        /// </summary>
        public virtual void Stop()
        {
            // Stopping cancels any pending completion: the run is over, so the
            // Completed event must not fire afterwards.
            _isCompleted = true;

            StopStoryboard();
            Restore();
        }

        /// <summary>
        /// Stops and removes the storyboard clock, which returns the animated
        /// properties to their base values.
        /// </summary>
        private void StopStoryboard()
        {
            // WPF throws when a storyboard that was never started is stopped,
            // and it keeps holding the animated values unless the clock is
            // removed, so both steps are guarded by the begun flag.
            if (!_isBegun)
            {
                return;
            }

            FrameworkElement containingObject = ContainingObject;
            if (containingObject == null)
            {
                Storyboard.Stop();
                Storyboard.Remove();
            }
            else
            {
                Storyboard.Stop(containingObject);
                Storyboard.Remove(containingObject);
            }

            _isBegun = false;
        }
    }
}
