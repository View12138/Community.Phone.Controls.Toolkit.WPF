// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Mirrors the Silverlight <c>Storyboard</c>-based transition interface
    /// and restores the element's cache mode and hit-test visibility when the
    /// transition completes or is stopped.
    /// </summary>
    public class Transition : ITransition
    {
        private CacheMode _cacheMode;
        private readonly UIElement _element;
        private bool _isHitTestVisible;
        private bool _begun;
        private bool _completed;
        private bool _restored;
        private bool _started;
        private Storyboard _storyboard;

        /// <summary>
        /// Gets or sets the underlying storyboard.
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
                        _storyboard.Completed -= OnStoryboardCompleted;
                    }

                    _storyboard = value;

                    if (_storyboard != null)
                    {
                        _storyboard.Completed += OnStoryboardCompleted;
                    }
                }
            }
        }

        /// <summary>
        /// Occurs when the transition completes.
        /// </summary>
        public event EventHandler Completed;

        /// <summary>
        /// Initializes a new instance of the <see cref="Transition"/> class.
        /// </summary>
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
        }

        /// <summary>
        /// Gets the element animated by this transition.
        /// </summary>
        protected UIElement Element
        {
            get { return _element; }
        }

        /// <inheritdoc />
        public virtual void Begin()
        {
            if (_begun)
            {
                Stop();
            }

            _begun = true;
            _completed = false;
            _restored = false;

            if (Storyboard.Children.Count == 0)
            {
                _started = false;
                _element.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(CompleteWithoutAnimation));
                return;
            }

            Save();
            _started = true;

            if (_element is FrameworkElement containingObject)
            {
                Storyboard.Begin(containingObject, true);
            }
            else
            {
                Storyboard.Begin();
            }
        }

        private void CompleteWithoutAnimation()
        {
            if (!_begun)
            {
                return;
            }

            _completed = true;
            _begun = false;
            _restored = true;

            EventHandler handler = Completed;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        /// <inheritdoc />
        public ClockState GetCurrentState()
        {
            return _begun ? ClockState.Active : ClockState.Stopped;
        }

        /// <inheritdoc />
        public TimeSpan GetCurrentTime()
        {
            if (!_begun)
            {
                return TimeSpan.Zero;
            }

            try
            {
                FrameworkElement containingObject = _element as FrameworkElement;
                TimeSpan? currentTime = containingObject == null
                    ? Storyboard.GetCurrentTime()
                    : Storyboard.GetCurrentTime(containingObject);
                return currentTime ?? TimeSpan.Zero;
            }
            catch (InvalidOperationException)
            {
                return TimeSpan.Zero;
            }
        }

        /// <inheritdoc />
        public void Pause()
        {
            FrameworkElement containingObject = _element as FrameworkElement;
            if (containingObject == null)
            {
                Storyboard.Pause();
            }
            else
            {
                Storyboard.Pause(containingObject);
            }
        }

        /// <inheritdoc />
        public void Resume()
        {
            FrameworkElement containingObject = _element as FrameworkElement;
            if (containingObject == null)
            {
                Storyboard.Resume();
            }
            else
            {
                Storyboard.Resume(containingObject);
            }
        }

        /// <inheritdoc />
        public void Seek(TimeSpan offset)
        {
            FrameworkElement containingObject = _element as FrameworkElement;
            if (containingObject == null)
            {
                Storyboard.Seek(offset);
            }
            else
            {
                Storyboard.Seek(containingObject, offset, TimeSeekOrigin.BeginTime);
            }
        }

        /// <inheritdoc />
        public void SeekAlignedToLastTick(TimeSpan offset)
        {
            FrameworkElement containingObject = _element as FrameworkElement;
            if (containingObject == null)
            {
                Storyboard.SeekAlignedToLastTick(offset);
            }
            else
            {
                Storyboard.SeekAlignedToLastTick(containingObject, offset, TimeSeekOrigin.BeginTime);
            }
        }

        /// <inheritdoc />
        public void SkipToFill()
        {
            FrameworkElement containingObject = _element as FrameworkElement;
            if (containingObject == null)
            {
                Storyboard.SkipToFill();
            }
            else
            {
                Storyboard.SkipToFill(containingObject);
            }
        }

        /// <inheritdoc />
        public virtual void Stop()
        {
            if (_started)
            {
                FrameworkElement containingObject = _element as FrameworkElement;
                if (containingObject != null)
                {
                    Storyboard.Stop(containingObject);
                    Storyboard.Remove(containingObject);
                }
                else
                {
                    Storyboard.Stop();
                    Storyboard.Remove();
                }

                _started = false;
            }

            Restore();
            _begun = false;
            _completed = true;
        }

        private void OnStoryboardCompleted(object sender, EventArgs e)
        {
            if (_completed)
            {
                return;
            }

            _completed = true;
            _begun = false;
            Restore();

            EventHandler handler = Completed;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void Save()
        {
            if (_restored)
            {
                return;
            }

            _cacheMode = _element.CacheMode;
            if (!(_cacheMode is BitmapCache))
            {
                _element.CacheMode = new BitmapCache();
            }

            _isHitTestVisible = _element.IsHitTestVisible;
            if (_isHitTestVisible)
            {
                _element.IsHitTestVisible = false;
            }
        }

        private void Restore()
        {
            if (_restored)
            {
                return;
            }

            _restored = true;

            if (!(_cacheMode is BitmapCache))
            {
                _element.CacheMode = _cacheMode;
            }

            _element.IsHitTestVisible = _isHitTestVisible || !_begun;
        }
    }
}
