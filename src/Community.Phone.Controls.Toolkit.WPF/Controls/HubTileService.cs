// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Provides methods to freeze and unfreeze hub tiles.
    /// </summary>
    public static class HubTileService
    {
        private static readonly HashSet<string> FrozenGroups = new HashSet<string>(StringComparer.Ordinal);

        /// <summary>
        /// Freezes a hub tile.
        /// </summary>
        public static void FreezeHubTile(HubTile tile)
        {
            if (tile != null)
            {
                tile.IsFrozen = true;
            }
        }

        /// <summary>
        /// Unfreezes a hub tile.
        /// </summary>
        public static void UnfreezeHubTile(HubTile tile)
        {
            if (tile != null)
            {
                tile.IsFrozen = false;
            }
        }

        /// <summary>
        /// Freezes all tiles in a group.
        /// </summary>
        public static void FreezeGroup(string group)
        {
            if (!string.IsNullOrEmpty(group))
            {
                FrozenGroups.Add(group);
            }
        }

        /// <summary>
        /// Unfreezes all tiles in a group.
        /// </summary>
        public static void UnfreezeGroup(string group)
        {
            if (!string.IsNullOrEmpty(group))
            {
                FrozenGroups.Remove(group);
            }
        }

        internal static bool IsGroupFrozen(string group)
        {
            return !string.IsNullOrEmpty(group) && FrozenGroups.Contains(group);
        }
    }
}
