﻿using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ICSViewer.Helpers
{
    internal static class Scaling
    {
        public static float GetScalingFactor()
        {
            var graphics = Graphics.FromHwnd(IntPtr.Zero);
            var desktop = graphics.GetHdc();
            var logicalScreenHeight = NativeMethods.GetDeviceCaps(desktop, (int) DeviceCap.Vertres);
            var physicalScreenHeight = NativeMethods.GetDeviceCaps(desktop, (int) DeviceCap.Desktopvertres);

            var screenScalingFactor = physicalScreenHeight/(float) logicalScreenHeight;

            return screenScalingFactor; // 1.25 = 125%
        }

        internal enum DeviceCap
        {
            Vertres = 10,
            Desktopvertres = 117,
            // http://pinvoke.net/default.aspx/gdi32/GetDeviceCaps.html
        }

        internal static class NativeMethods
        {
            [DllImport("gdi32.dll")]
            public static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
        }
    }
}