// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;

namespace PhoneToolkitSample.Placement
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    // RECT structure required by WINDOWPLACEMENT structure
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public Rect(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
        public static bool operator ==(Rect left, Rect right)
        {
            return left.Left == right.Left &&
                   left.Top == right.Top &&
                   left.Right == right.Right &&
                   left.Bottom == right.Bottom;
        }

        public static bool operator !=(Rect left, Rect right)
        {
            return !(left == right);
        }
    }
}
