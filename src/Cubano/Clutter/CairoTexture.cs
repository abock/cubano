// 
// CairoTexture.cs
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
    public class CairoTexture : Texture
    {
        [DllImport ("clutter")]
        private static extern IntPtr clutter_cairo_texture_get_type ();
        
        public static new GLib.GType GType {
            get { return new GLib.GType (clutter_cairo_texture_get_type ()); }
        }
        
        public CairoTexture (IntPtr raw) : base (raw) 
        {
        }

        public CairoTexture (uint width, uint height) : base (IntPtr.Zero)
        {
            CreateNativeObject (new string [] {
                "surface-height",
                "surface-width"
            }, new GLib.Value [] {
                new GLib.Value (height),
                new GLib.Value (width)
            });
        }
        
        [DllImport ("clutter")]
        private static extern IntPtr clutter_cairo_texture_create (IntPtr self);
        
        public Cairo.Context CreateContext ()
        {
            return new Cairo.Context (clutter_cairo_texture_create (Handle));
        }
        
        [GLib.Property ("surface-height")]
        public uint SurfaceHeight {
            get { using (GLib.Value val = GetProperty ("surface-height")) return (uint)val; }
            set { using (GLib.Value val = new GLib.Value (value)) SetProperty ("surface-height", val); }
        }
        
        [GLib.Property ("surface-width")]
        public uint SurfaceWidth {
            get { using (GLib.Value val = GetProperty ("surface-width")) return (uint)val; }
            set { using (GLib.Value val = new GLib.Value (value)) SetProperty ("surface-width", val); }
        }
    }
}
