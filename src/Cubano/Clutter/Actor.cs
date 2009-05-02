//
// Actor.cs
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
    public enum RotateAxis : int { X, Y, Z }

    public class Actor : GLib.InitiallyUnowned
    {
        private object self_mutex;
        protected object SelfMutex {
            get { return self_mutex ?? (self_mutex = new object ()); }
        }
    
        [DllImport ("clutter")]
        private static extern IntPtr clutter_actor_get_type ();
        
        public static new GLib.GType GType {
            get { return new GLib.GType (clutter_actor_get_type ()); }
        }
    
        public Actor (IntPtr raw) : base (raw) 
        {
        }

        protected Actor () : base (IntPtr.Zero)
        {
            CreateNativeObject (new string[0], new GLib.Value[0]);
        }
        
        [DllImport ("clutter")]
        private static extern void clutter_actor_hide (IntPtr self);
        
        public void Hide ()
        {
            clutter_actor_hide (Handle);
        }
        
        [DllImport ("clutter")]
        private static extern void clutter_actor_show (IntPtr self);
        
        public void Show ()
        {
            clutter_actor_show (Handle);
        }
        
        [DllImport ("clutter")]
        private static extern void clutter_actor_show_all (IntPtr self);
        
        public void ShowAll ()
        {
            clutter_actor_show_all (Handle);
        }
        
        [DllImport ("clutter")]
        private static extern void clutter_actor_hide_all (IntPtr self);
        
        public void HideAll ()
        {
            clutter_actor_hide_all (Handle);
        }
        
        [DllImport ("clutter")]
        private static extern void clutter_actor_paint (IntPtr self);
        
        public void Paint ()
        {
            clutter_actor_paint (Handle);
        }
        
        [DllImport ("clutter")]
        private static extern void clutter_actor_queue_redraw (IntPtr self);
        
        public void QueueRedraw ()
        {
            clutter_actor_queue_redraw (Handle);
        }
        
        [DllImport ("clutter")]
        private static extern void clutter_actor_queue_relayout (IntPtr self);
        
        public void QueueRelayout ()
        {
            clutter_actor_queue_relayout (Handle);
        }
        
        [DllImport ("clutter")]
        private static extern void clutter_actor_set_position (IntPtr handle, int x, int y);
        
        public void SetPosition (int x, int y)
        {
            clutter_actor_set_position (Handle, x, y);
        }
        
        [DllImport ("clutter")]
        private static extern void clutter_actor_get_position (IntPtr handle, out int x, out int y);
        
        public void GetPosition (out int x, out int y)
        {
            clutter_actor_get_position (Handle, out x, out y);
        }
        
        [DllImport ("clutter")]
        private static extern void clutter_actor_set_size (IntPtr handle, int width, int height);
        
        public void SetSize (int width, int height)
        {
            clutter_actor_set_size (Handle, width, height);
        }
        
        [DllImport ("clutter")]
        private static extern void clutter_actor_get_size (IntPtr handle, out int width, out int height);
        
        public void GetSize (out int width, out int height)
        {
            clutter_actor_get_size (Handle, out width, out height);
        }
        
        [DllImport ("clutter")]
        private static extern void clutter_actor_set_rotation (IntPtr handle, RotateAxis axis, 
            double angle, int x, int y, int z);
            
        public void SetRotation (RotateAxis axis, double angle, int x, int y, int z)
        {
            clutter_actor_set_rotation (Handle, axis, angle, x, y, z);
        }
        
#region Events / Virtual Methods

        private void ConnectSignal (string signalName, EventHandler<EventArgs> handler)
        {
            ConnectSignal<EventArgs> (signalName, handler);
        }

        private void ConnectSignal<T> (string signalName, EventHandler<T> handler) where T : EventArgs
        {
            GLib.Signal.Lookup (this, signalName, typeof (T)).AddDelegate (handler);
        }
        
        private void DisconnectSignal<T> (string signalName, EventHandler<T> handler) where T : EventArgs
        {
             GLib.Signal.Lookup (this, signalName, typeof (T)).RemoveDelegate (handler);
        }
        
        public event EventHandler<EventArgs> PaintEvent {
            add { ConnectSignal ("paint", value); }
            remove { DisconnectSignal ("paint", value); }
        }

#endregion

#region Properties
                                
        [GLib.Property ("name")]
        public string Name {
            get { using (GLib.Value val = GetProperty ("name")) return (string)val; }
            set { using (GLib.Value val = new GLib.Value (value)) SetProperty ("name", val); }
        }
        
        [GLib.Property ("anchor-x")]
        public int AnchorX {
            get { using (GLib.Value val = GetProperty ("anchor-x")) return (int)val; }
            set { using (GLib.Value val = new GLib.Value (value)) SetProperty ("anchor-x", val); }
        }
        
        [GLib.Property ("anchor-y")]
        public int AnchorY {
            get { using (GLib.Value val = GetProperty ("anchor-y")) return (int)val; }
            set { using (GLib.Value val = new GLib.Value (value)) SetProperty ("anchor-y", val); }
        }
        
        [GLib.Property ("x")]
        public int X {
            get { using (GLib.Value val = GetProperty ("x")) return (int)val; }
            set { using (GLib.Value val = new GLib.Value (value)) SetProperty ("x", val); }
        }
        
        [GLib.Property ("y")]
        public int Y {
            get { using (GLib.Value val = GetProperty ("y")) return (int)val; }
            set { using (GLib.Value val = new GLib.Value (value)) SetProperty ("y", val); }
        }
        
        [GLib.Property ("width")]
        public int Width {
            get { using (GLib.Value val = GetProperty ("width")) return (int)val; }
            set { using (GLib.Value val = new GLib.Value (value)) SetProperty ("width", val); }
        }
        
        [GLib.Property ("height")]
        public int Height {
            get { using (GLib.Value val = GetProperty ("height")) return (int)val; }
            set { using (GLib.Value val = new GLib.Value (value)) SetProperty ("height", val); }
        }
        
        [GLib.Property ("depth")]
        public int Depth {
            get { using (GLib.Value val = GetProperty ("depth")) return (int)val; }
            set { using (GLib.Value val = new GLib.Value (value)) SetProperty ("depth", val); }
        }
        
        [GLib.Property ("opacity")]
        public byte OpacityAsByte {
            get { using (GLib.Value val = GetProperty ("opacity")) return (byte)val; }
            set { using (GLib.Value val = new GLib.Value (value)) SetProperty ("opacity", val); }
        }
        
        public double Opacity {
            get { return OpacityAsByte / 255.0; }
            set { OpacityAsByte = (byte)Math.Round (value * 255.0); }
        }
        
        [GLib.Property ("rotation-angle-x")]
        public double RotationAngleX {
            get { using (GLib.Value val = GetProperty ("rotation-angle-x")) return (double)val; }
            set { using (GLib.Value val = new GLib.Value (value)) SetProperty ("rotation-angle-x", val); }
        }
        
        [GLib.Property ("rotation-angle-y")]
        public double RotationAngleY {
            get { using (GLib.Value val = GetProperty ("rotation-angle-y")) return (double)val; }
            set { using (GLib.Value val = new GLib.Value (value)) SetProperty ("rotation-angle-y", val); }
        }
        
        [GLib.Property ("rotation-angle-z")]
        public double RotationAngleZ {
            get { using (GLib.Value val = GetProperty ("rotation-angle-z")) return (double)val; }
            set { using (GLib.Value val = new GLib.Value (value)) SetProperty ("rotation-angle-z", val); }
        }
        
        [GLib.Property ("scale-x")]
        public double ScaleX {
            get { using (GLib.Value val = GetProperty ("scale-x")) return (double)val; }
            set { using (GLib.Value val = new GLib.Value (value)) SetProperty ("scale-x", val); }
        }
        
        [GLib.Property ("scale-y")]
        public double ScaleY {
            get { using (GLib.Value val = GetProperty ("scale-y")) return (double)val; }
            set { using (GLib.Value val = new GLib.Value (value)) SetProperty ("scale-y", val); }
        }
        
        [GLib.Property ("scale-center-x")]
        public int ScaleCenterX {
            get { using (GLib.Value val = GetProperty ("scale-center-x")) return (int)val; }
            set { using (GLib.Value val = new GLib.Value (value)) SetProperty ("scale-center-x", val); }
        }
        
        [GLib.Property ("scale-center-y")]
        public int ScaleCenterY {
            get { using (GLib.Value val = GetProperty ("scale-center-y")) return (int)val; }
            set { using (GLib.Value val = new GLib.Value (value)) SetProperty ("scale-center-y", val); }
        }
        
        [GLib.Property ("show-on-set-parent")]
        public bool ShowOnSetParent {
            get { using (GLib.Value val = GetProperty ("show-on-set-parent")) return (bool)val; }
            set { using (GLib.Value val = new GLib.Value (value)) SetProperty ("show-on-set-parent", val); }
        }
        
        [GLib.Property ("visible")]
        public bool Visible {
            get { using (GLib.Value val = GetProperty ("visible")) return (bool)val; }
            set { using (GLib.Value val = new GLib.Value (value)) SetProperty ("visible", val); }
        }
        
#endregion

    }
}
