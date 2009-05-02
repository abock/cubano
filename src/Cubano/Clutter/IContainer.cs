// 
// IContainer.cs
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
    public interface IContainer
    {
        void Add (Actor actor);
        void Add (params Actor [] actors);
        
        void Remove (Actor actor);
        void Remove (params Actor [] actors);
    }
    
    public static class IContainerImpl
    {
        [DllImport ("clutter")]
        private static extern void clutter_container_add_actor (IntPtr handle, IntPtr actor);
        
        public static void Add<T> (T container, Actor actor) where T : Actor, IContainer
        {
            clutter_container_add_actor (container.Handle, actor.Handle);
        }
        
        public static void Add<T> (T container, params Actor [] actors) where T : Actor, IContainer
        {
            foreach (var actor in actors) {
                clutter_container_add_actor (container.Handle, actor.Handle);
            }
        }
        
        [DllImport ("clutter")]
        private static extern void clutter_container_remove_actor (IntPtr handle, IntPtr actor);
        
        public static void Remove<T> (T container, Actor actor) where T : Actor, IContainer
        {
            clutter_container_remove_actor (container.Handle, actor.Handle);
        }
        
        public static void Remove<T> (T container, params Actor [] actors) where T : Actor, IContainer
        {
            foreach (var actor in actors) {
                clutter_container_remove_actor (container.Handle, actor.Handle);
            }
        }
    }
}
