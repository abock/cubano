// 
// ShapedPopupWindow.cs
//  
// Author:
//     Aaron Bockover <abockover@novell.com>
// 
// Copyright 2009 Novell, Inc.
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

using Hyena.Gui;

namespace Banshee.Gui.Widgets
{
    public class ShapedPopupWindow : Window
    {
        public ShapedPopupWindow () : base (WindowType.Toplevel)
        {
            DestroyWithParent = true;
            Decorated = false;
            SkipPagerHint = true;
            SkipTaskbarHint = true;
            
            Screen.CompositedChanged += OnCompositedChanged;
        }
        
#region Window Shaping

        private bool use_shape_extension;
        
        private void OnCompositedChanged (object o, EventArgs args)
        {
            // FIXME: Try to actually make the colormap switching work
        }
        
        protected override void OnRealized ()
        {
            ProbeComposited ();
            base.OnRealized ();
            ShapeWindow ();
        }
        
        private void ProbeComposited ()
        {
            use_shape_extension = !CompositeUtils.IsComposited (Screen) || 
                !CompositeUtils.SetRgbaColormap (this);
        }
        
        protected void ShapeWindow ()
        {
            using (var bitmap = new Gdk.Pixmap (GdkWindow,
                Allocation.Width, Allocation.Height, 1)) {
            
                using (var cr = Gdk.CairoHelper.Create (bitmap)) {
                    DrawShape (cr);
                }
                
                if (use_shape_extension) {
                    ShapeCombineMask (null, 0, 0);
                    ShapeCombineMask (bitmap, 0, 0);
                } else {
                    using (var cr = Gdk.CairoHelper.Create (GdkWindow)) {
                        DrawShape (cr);
                    }
                    
                    try {
                        CompositeUtils.InputShapeCombineMask (this, null, 0, 0);
                        CompositeUtils.InputShapeCombineMask (this, bitmap, 0, 0);
                    } catch {
                        Hyena.Log.Warning ("This GTK+ version does not support input shapes");
                    }
                }
            }
        }
        
        protected virtual void DrawShape (Cairo.Context cr)
        {
            CairoExtensions.RoundedRectangle (cr, 0, 0, 
                Allocation.Width, Allocation.Height, 5);
        }
        
#endregion

#region Container/Margins

        private Widget child;
        private Gdk.Rectangle child_allocation;
        
        private int margin_top;
        public int MarginTop {
            get { return margin_top; }
            set { margin_top = value; QueueResize (); }
        }
        
        private int margin_left;
        public int MarginLeft {
            get { return margin_left; }
            set { margin_left = value; QueueResize (); }
        }
        
        private int margin_bottom;
        public int MarginBottom {
            get { return margin_bottom; }
            set { margin_bottom = value; QueueResize (); }
        }    

        private int margin_right;
        public int MarginRight {
            get { return margin_right; }
            set { margin_right = value; QueueResize (); }
        }
        
        protected override void OnSizeRequested (ref Requisition requisition)
        {
            if (child != null && child.Visible) {
                // Add the child's width/height        
                Requisition child_requisition = child.SizeRequest ();
                requisition.Width = Math.Max (0, child_requisition.Width);
                requisition.Height = child_requisition.Height;
            } else {
                requisition.Width = 0;
                requisition.Height = 0;
            }
            
            // Add the frame border
            requisition.Width += (int)BorderWidth * 2 + MarginLeft + MarginRight;
            requisition.Height += (int)BorderWidth * 2 + MarginTop + MarginBottom;
        }

        protected override void OnSizeAllocated (Gdk.Rectangle allocation)
        {
            if (IsRealized) {
                ShapeWindow ();
            }
            
            base.OnSizeAllocated (allocation);
            
            child_allocation = new Gdk.Rectangle ();
            
            if (child == null || !child.Visible) {
                return;
            }
            
            child_allocation.X = (int)BorderWidth + MarginTop;
            child_allocation.Y = (int)BorderWidth + MarginLeft;
            child_allocation.Width = (int)Math.Max (1, 
                Allocation.Width - 2 * (int)BorderWidth - MarginLeft - MarginRight);
            child_allocation.Height = (int)Math.Max (1, 
                Allocation.Height - 2 * (int)BorderWidth - MarginTop - MarginBottom);
                
            child_allocation.X += Allocation.X;
            child_allocation.Y += Allocation.Y;
            
            child.SizeAllocate (child_allocation);
        }
        
        protected override void OnAdded (Widget widget)
        {
            child = widget;
            base.OnAdded (widget);
        }

        protected override void OnRemoved (Widget widget)
        {
            if (child == widget) {
                child = null;
            }

            base.OnRemoved (widget);
        }

#endregion

#region Window Popup/Positioning

        public double PopupXAlign { get; set; }
        public double PopupYAlign { get; set; }
        
        public void Popup (Widget widget)
        {
            Popup (widget, 0.5, 0.5);
        }
        
        public void Popup (Widget context, double contextXAlign, double contextYAlign)
        {
            Realize ();
            QueueResize ();
        
            int x, y;
            context.GdkWindow.GetOrigin (out x, out y);
            x += context.Allocation.X;
            y += context.Allocation.Y;
            
            x -= (int)(Allocation.Width * PopupXAlign);
            y -= (int)(Allocation.Height * PopupYAlign);
            
            x += (int)(context.Allocation.Width * contextXAlign);
            y += (int)(context.Allocation.Height * contextYAlign);
            
            Move (x, y);
            Show ();
        }
        
        protected override bool OnButtonPressEvent (Gdk.EventButton evnt)
        {
            return base.OnButtonPressEvent (evnt);
        }


#endregion

    }
}
