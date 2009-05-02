// 
// Application.cs
//  
// Author:
//       Aaron Bockover <abockover@novell.com>
// 
// Copyright 2009 Aaron Bockover
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Runtime.InteropServices;

namespace Clutter
{
    public static class Application
    {
        public enum InitError {
            Success = 1,
            ErrorUnknown = 0,
            ErrorThreads = -1,
            ErrorBackend = -2,
            ErrorInternal = -3
        }
        
        private class ClutterInitException : ApplicationException
        {
            private InitError clutter_error;
            
            public ClutterInitException (InitError error)
                : base (String.Format ("Error from clutter_init error: {0}", error))
            {
                clutter_error = error;
            }
            
            public InitError ClutterError {
                get { return clutter_error; }
            }
        }
    
        [DllImport ("clutter")]
        private static extern InitError clutter_init (IntPtr argc, IntPtr argv);
        
        public static void Init ()
        {
            InitError error = clutter_init (IntPtr.Zero, IntPtr.Zero);
            if (error != InitError.Success) {
                throw new ClutterInitException (error);
            }
        }
        
        public static void InitForToolkit ()
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix) {
                X11.SetDisplay (Gdk.Display.Default);
                X11.DisableEventRetrieval ();
            } else {
                Win32.DisableEventRetrieval ();
            }
            
            Init ();
        }
        
        public static class X11
        {
            [DllImport ("gdk-x11")]
            private static extern IntPtr gdk_x11_display_get_xdisplay (IntPtr display);
        
            [DllImport ("clutter")]
            private static extern void clutter_x11_set_display (IntPtr xdisplay);
            
            public static void SetDisplay (Gdk.Display display)
            {
                clutter_x11_set_display (display == null 
                    ? IntPtr.Zero 
                    : gdk_x11_display_get_xdisplay (display.Handle));
            }
            
            [DllImport ("clutter")]
            private static extern void clutter_x11_disable_event_retrieval ();
            
            public static void DisableEventRetrieval ()
            {
                clutter_x11_disable_event_retrieval ();
            }
        }
        
        public static class Win32
        {
            [DllImport ("clutter")]
            private static extern void clutter_win32_disable_event_retrieval ();
            
            public static void DisableEventRetrieval ()
            {
                clutter_win32_disable_event_retrieval ();
            }
        }
    }
}
