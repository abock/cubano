// 
// Rectangle.cs
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
    public class Rectangle : Actor
    {
        [DllImport ("clutter")]
        private static extern IntPtr clutter_rectangle_get_type ();
        
        public static new GLib.GType GType {
            get { var p = Actor.GType; return new GLib.GType (clutter_rectangle_get_type ()); }
        }
        
        public Rectangle (IntPtr raw) : base (raw)
        {
        }
        
        public Rectangle () : base (IntPtr.Zero)
        {
            CreateNativeObject (new string[0], new GLib.Value[0]);
        }
        
        public Rectangle (Color color)
        {
            Color = color;
        }
        
        [DllImport ("clutter")]
        private static extern void clutter_rectangle_set_color (IntPtr handle, ref Color color);
        
        [DllImport ("clutter")]
        private static extern void clutter_rectangle_get_color (IntPtr handle, out Color color);
        
        [GLib.Property ("color")]
        public Color Color {
            get {
                Color color = Color.Zero;
                clutter_rectangle_get_color (Handle, out color);
                return color;
            }
            
            set { clutter_rectangle_set_color (Handle, ref value); }
        }
    }
}
