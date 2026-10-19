// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Utilities for working with collections of weak references.
    /// </summary>
    public static class WeakReferenceHelper
    {
        /// <summary>
        /// Determines whether the collection contains a reference to the target.
        /// </summary>
        public static bool ContainsTarget(IEnumerable<WeakReference> references, object target)
        {
            if (references == null)
            {
                return false;
            }

            foreach (WeakReference reference in references)
            {
                if (ReferenceEquals(target, reference.Target))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes a reference to the target from the list.
        /// </summary>
        public static bool TryRemoveTarget(IList<WeakReference> references, object target)
        {
            if (references == null)
            {
                return false;
            }

            for (int i = 0; i < references.Count; i++)
            {
                if (ReferenceEquals(references[i].Target, target))
                {
                    references.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes references whose target has been collected.
        /// </summary>
        public static void RemoveNullTargetReferences(IList<WeakReference> references)
        {
            if (references == null)
            {
                return;
            }

            for (int i = references.Count - 1; i >= 0; i--)
            {
                if (references[i].Target == null)
                {
                    references.RemoveAt(i);
                }
            }
        }
    }
}
