//
// Texture.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//
// Copyright (C) 2009 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Runtime.InteropServices;

namespace Clutter
{
    public class Texture : Actor
    {
        [DllImport ("clutter")]
        private static extern IntPtr clutter_texture_get_type ();
        
        public static new GLib.GType GType {
            get { return new GLib.GType (clutter_texture_get_type ()); }
        }
        
        public Texture (IntPtr raw) : base (raw) 
        {
        }

        public Texture () : base (IntPtr.Zero)
        {
            CreateNativeObject (new string[0], new GLib.Value[0]);
        }
        
        [GLib.Signal ("size-change")]
        public event EventHandler<SizeChangeArgs> SizeChange {
            add { GLib.Signal.Lookup (this, "size-change", typeof (SizeChangeArgs)).AddDelegate (value); }
            remove { GLib.Signal.Lookup (this, "size-change", typeof (SizeChangeArgs)).RemoveDelegate (value); }
        }
    }

    public class SizeChangeArgs : GLib.SignalArgs 
    {
        public int Width {
            get { return (int)Args[0]; }
        }
        
        public int Height {
            get { return (int)Args[1]; }
        }
    }
}
