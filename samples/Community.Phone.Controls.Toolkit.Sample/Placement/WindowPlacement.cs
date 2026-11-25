// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace PhoneToolkitSample.Placement
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct WindowPlacement
    {
        public int length;
        public int flags;
        public int showCmd;
        public Point minPosition;
        public Point maxPosition;
        public Rect normalPosition;

        public static bool operator ==(WindowPlacement left, WindowPlacement right)
        {
            return left.length == right.length &&
                   left.flags == right.flags &&
                   left.showCmd == right.showCmd &&
                   left.minPosition == right.minPosition &&
                   left.maxPosition == right.maxPosition &&
                   left.normalPosition == right.normalPosition;
        }

        public static bool operator !=(WindowPlacement left, WindowPlacement right)
        {
            return !(left == right);
        }
    }
}
