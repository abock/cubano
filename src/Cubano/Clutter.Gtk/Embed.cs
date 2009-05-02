//
// Embed.cs
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

using Gtk;
using Clutter;

namespace Clutter.Gtk
{
    public class Embed : Widget
    {
        public Embed (IntPtr raw) : base (raw)
        {
        }
        
        public Embed () : base (IntPtr.Zero)
        {
            CreateNativeObject (new string[0], new GLib.Value[0]);
        }
        
        [DllImport ("clutter-gtk")]
        private static extern IntPtr gtk_clutter_embed_get_stage (IntPtr handle);
        
        public Stage Stage {
            get { return new Stage (gtk_clutter_embed_get_stage (Handle)); }
        }
        
        [DllImport ("clutter-gtk")]
        private static extern IntPtr gtk_clutter_embed_get_type ();
        
        public static new GLib.GType GType {
            get { return new GLib.GType (gtk_clutter_embed_get_type ()); }
        }
    }
}
