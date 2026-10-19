// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;

namespace Microsoft.Phone.Controls.Primitives
{
    /// <summary>
    /// Represents an interface used by date/time pickers to communicate with a
    /// picker page.
    /// </summary>
    public interface IDateTimePickerPage
    {
        /// <summary>
        /// Gets or sets the date/time value.
        /// </summary>
        DateTime? Value { get; set; }

        /// <summary>
        /// Sets the flow direction on child controls that need it.
        /// </summary>
        void SetFlowDirection(FlowDirection flowDirection);
    }
}
