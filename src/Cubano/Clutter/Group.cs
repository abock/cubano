//
// Group.cs
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
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Clutter
{
    public class Group : Actor, IContainer, IEnumerable<Actor>
    {
        [DllImport ("clutter")]
        private static extern IntPtr clutter_group_get_type ();
        
        public static new GLib.GType GType {
            get { return new GLib.GType (clutter_group_get_type ()); }
        }
        
        public Group (IntPtr raw) : base (raw) 
        {
        }

        public Group () : base (IntPtr.Zero)
        {
            CreateNativeObject (new string[0], new GLib.Value[0]);
        }
        
        [DllImport ("clutter")]
        private static extern void clutter_group_remove_all (IntPtr self);
        
        public void RemoveAll ()
        {
            lock (SelfMutex) { clutter_group_remove_all (Handle); }
        }
        
        [DllImport ("clutter")]
        private static extern IntPtr clutter_group_get_nth_child (IntPtr self, int index);
        
        public Actor ChildAt (int index)
        {
            return new Actor (clutter_group_get_nth_child (Handle, index));
        }
        
        [DllImport ("clutter")]
        private static extern int clutter_group_get_n_children (IntPtr self);
        
        public int ChildCount {
            get { return clutter_group_get_n_children (Handle); }
        }
        
#region IEnumerable<Actor> Implementation

        public IEnumerator<Actor> GetEnumerator ()
        {
            lock (SelfMutex) {
                for (int i = 0, n = ChildCount; i < n; i++) {
                    yield return ChildAt (i);
                }
            }
        }
        
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            return GetEnumerator ();
        }

#endregion
        
#region IContainer Implementation

        public void Add (Actor actor)
        {
            lock (SelfMutex) { IContainerImpl.Add (this, actor); }
        }
        
        public void Add (params Actor [] actors)
        {
            lock (SelfMutex) { IContainerImpl.Add (this, actors); }
        }

        public void Remove (Actor actor)
        {
            lock (SelfMutex) { IContainerImpl.Remove (this, actor); }
        }
        
        public void Remove (params Actor [] actors)
        {
            lock (SelfMutex) { IContainerImpl.Remove (this, actors); }
        }

#endregion

    }
}
