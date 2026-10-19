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

            Save();
            _begun = true;
            _completed = false;
            _restored = false;
            _started = true;
            Storyboard.Begin();
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
                return Storyboard.GetCurrentTime();
            }
            catch (InvalidOperationException)
            {
                return TimeSpan.Zero;
            }
        }

        /// <inheritdoc />
        public void Pause()
        {
            Storyboard.Pause();
        }

        /// <inheritdoc />
        public void Resume()
        {
            Storyboard.Resume();
        }

        /// <inheritdoc />
        public void Seek(TimeSpan offset)
        {
            Storyboard.Seek(offset);
        }

        /// <inheritdoc />
        public void SeekAlignedToLastTick(TimeSpan offset)
        {
            Storyboard.SeekAlignedToLastTick(offset);
        }

        /// <inheritdoc />
        public void SkipToFill()
        {
            Storyboard.SkipToFill();
        }

        /// <inheritdoc />
        public virtual void Stop()
        {
            if (_started)
            {
                Storyboard.Stop();
                Storyboard.Remove();
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
