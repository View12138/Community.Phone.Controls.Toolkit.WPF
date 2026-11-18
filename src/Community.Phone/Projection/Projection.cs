// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Base class for a 3D projection that can be applied to a
    /// <see cref="ProjectionHost"/>.
    /// </summary>
    public abstract class Projection : Animatable
    {
        /// <summary>
        /// Occurs when a projection property changes and the host must update
        /// its visual representation.
        /// </summary>
        public event EventHandler ProjectionChanged;

        /// <summary>
        /// Gets a value indicating whether the projection leaves the element
        /// visually unchanged.
        /// </summary>
        public virtual bool IsIdentity => false;

        /// <summary>
        /// Creates the pixel-space transform for an element of the supplied
        /// size. The returned matrix is applied to a plane whose origin is the
        /// center of the element and whose positive Y axis points up.
        /// </summary>
        /// <param name="elementSize">The arranged size of the element.</param>
        /// <returns>The transform matrix.</returns>
        public abstract Matrix3D ToMatrix3D(Size elementSize);

        /// <summary>
        /// Raises the <see cref="ProjectionChanged"/> event.
        /// </summary>
        protected void OnProjectionChanged() => ProjectionChanged?.Invoke(this, EventArgs.Empty);
    }
}
