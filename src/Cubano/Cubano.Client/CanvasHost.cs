// 
// CanvasHost.cs
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
using Gtk;
using Gdk;

using Hyena.Gui.Theming;

namespace Hyena.Gui.Canvas2
{
    public class CanvasHost : Widget
    {
        private Gdk.Window event_window;
        private ICanvasItem canvas_child;
        private Theme theme;
    
        public CanvasHost ()
        {
            WidgetFlags |= WidgetFlags.NoWindow;
        }
        
        protected CanvasHost (IntPtr native) : base (native)
        {
        }
        
        protected override void OnRealized ()
        {
            base.OnRealized ();
            
            WindowAttr attributes = new WindowAttr ();
            attributes.WindowType = Gdk.WindowType.Child;
            attributes.X = Allocation.X;
            attributes.Y = Allocation.Y;
            attributes.Width = Allocation.Width;
            attributes.Height = Allocation.Height;
            attributes.Wclass = WindowClass.InputOnly;
            attributes.EventMask = (int)(
                EventMask.PointerMotionMask |
                EventMask.ButtonPressMask | 
                EventMask.ButtonReleaseMask |
                EventMask.EnterNotifyMask |
                EventMask.LeaveNotifyMask |
                EventMask.ExposureMask);
            
            WindowAttributesType attributes_mask =
                WindowAttributesType.X | WindowAttributesType.Y | WindowAttributesType.Wmclass;
            
            event_window = new Gdk.Window (GdkWindow, attributes, attributes_mask);
            event_window.UserData = Handle;
        }
        
        protected override void OnUnrealized ()
        {
            WidgetFlags ^= WidgetFlags.Realized;
            
            event_window.UserData = IntPtr.Zero;
            Hyena.Gui.GtkWorkarounds.WindowDestroy (event_window);
            event_window = null;
            
            base.OnUnrealized ();
        }
        
        protected override void OnMapped ()
        {
            event_window.Show ();
            base.OnMapped ();
        }

        protected override void OnUnmapped ()
        {
            event_window.Hide ();
            base.OnUnmapped ();
        }
        
        protected override void OnSizeAllocated (Gdk.Rectangle allocation)
        {
            base.OnSizeAllocated (allocation);
            
            if (IsRealized) {
                event_window.MoveResize (allocation);
            }
            
            if (canvas_child != null) {
                canvas_child.Left = Allocation.X;
                canvas_child.Top = Allocation.Y;
                canvas_child.Width = Allocation.Width;
                canvas_child.Height = Allocation.Height;
                canvas_child.Layout ();
            }
        }
        
        protected override void OnSizeRequested (ref Gtk.Requisition requisition)
        {
            if (canvas_child != null) {
                double w, h;
                canvas_child.SizeRequest (out w, out h);
                requisition.Width = (int)Math.Ceiling (w);
                requisition.Height = (int)Math.Ceiling (h);
            }
        }
        
        protected override bool OnExposeEvent (Gdk.EventExpose evnt)
        {
            if (canvas_child == null || !Visible || !IsMapped) {
                return true;
            }
            
            Cairo.Context cr = Gdk.CairoHelper.Create (evnt.Window);
            
            foreach (Gdk.Rectangle damage in evnt.Region.GetRectangles ()) {
                cr.Rectangle (damage.X, damage.Y, damage.Width, damage.Height);
                cr.Clip ();
                //cr.Rectangle (Allocation.X, Allocation.Y, Allocation.Width, Allocation.Height);
                //cr.SetSourceRGB (1, 0, 0);
                //cr.Fill ();
                canvas_child.Render (cr);
                cr.ResetClip ();
            }
            
            CairoExtensions.DisposeContext (cr);
            
            return true;
        }
        
        private bool changing_style = false;
        
        protected override void OnStyleSet (Style old_style)
        {
            if (changing_style) {
                return;
            }
            
            changing_style = true;
            
            theme = new GtkTheme (this);
            if (canvas_child != null) {
                canvas_child.Theme = theme;
            }
            
            changing_style = false;
            
            base.OnStyleSet (old_style);
        }
        
        protected override bool OnButtonPressEvent (Gdk.EventButton evnt)
        {
            if (canvas_child != null) {
                canvas_child.ButtonPress (evnt.X, evnt.Y, evnt.Button);
            }
            return true;
        }
        
        protected override bool OnButtonReleaseEvent (Gdk.EventButton evnt)
        {
            if (canvas_child != null) {
                canvas_child.ButtonRelease ();
            }
            return true;
        }
        
        protected override bool OnMotionNotifyEvent (EventMotion evnt)
        {
            if (canvas_child != null) {
                canvas_child.PointerMotion (evnt.X, evnt.Y);
            }
            return true;
        }
        
        public void Add (ICanvasItem child)
        {
            if (Child != null) {
                throw new InvalidOperationException ("Child is already set, remove it first");
            }
            
            Child = child;
        }
        
        public void Remove (ICanvasItem child)
        {
            if (Child != child) {
                throw new InvalidOperationException ("child does not already belong to host");
            }
            
            Child = null;
        }
        
        private void OnCanvasChildRerender (object o, EventArgs args)
        {
            QueueDraw ();
        }
        
        private void OnCanvasChildResize (object o, EventArgs args)
        {
            QueueResize ();
        }
        
        public ICanvasItem Child {
            get { return canvas_child; }
            set {
                if (canvas_child == value) {
                    return;
                } else if (canvas_child != null) {
                    canvas_child.Theme = null;
                    canvas_child.Rerender -= OnCanvasChildRerender;
                    canvas_child.Resize -= OnCanvasChildResize;
                }
            
                canvas_child = value;
                
                if (canvas_child != null) {
                    canvas_child.Theme = theme;
                    canvas_child.Rerender += OnCanvasChildRerender;
                    canvas_child.Resize += OnCanvasChildResize;
                }
                
                QueueDraw ();
                QueueResize ();
            }
        }
    }
}
