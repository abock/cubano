// 
// TextBlock.cs
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

namespace Hyena.Gui.Canvas
{
    public class TextBlock : CanvasItem
    {
        private string markup;
        private Pango.Layout layout;
        private bool remeasure = true;
    
        public TextBlock ()
        {
        }
        
        private bool EnsureLayout ()
        {
            if (layout != null) {
                return true;
            }
        
            Gtk.Widget widget = Manager == null ? null : Manager.Host as Gtk.Widget;
            if (widget == null || widget.GdkWindow == null || !widget.IsRealized) {
                return false;
            }
            
            using (var cr = Gdk.CairoHelper.Create (widget.GdkWindow)) {
                layout = CairoExtensions.CreateLayout (widget, cr);
            }
            
            if (remeasure) {
                remeasure = false;
                Measure (ContentSize);
            }
                        
            return layout != null;
        }
        
        public override Size Measure (Size available)
        {
            if (!EnsureLayout ()) {
                return new Size (0, 0);
            }
            
            available = base.Measure (available);
            
            int text_w, text_h;
            layout.Width = (int)(Pango.Scale.PangoScale * available.Width);
            layout.SetMarkup (Markup);
            layout.GetPixelSize (out text_w, out text_h);
            
            DesiredSize = new Size (available.Width,
                text_h + Margin.Top + Margin.Bottom);

            // Hack, as this prevents the TextBlock from 
            // being flexible in a Vertical StackPanel 
            Height = DesiredSize.Height;
            
            return DesiredSize;
        }
        
        protected override void ClippedRender (Cairo.Context cr)
        {
            if (!EnsureLayout ()) {
                return;
            }
            
            int layout_width = (int)(Pango.Scale.PangoScale * ContentAllocation.Width);
            if (layout.Width != layout_width) {
                layout.Width = layout_width;
            }
            
            cr.Color = new Cairo.Color (1, 0, 0);
            Pango.CairoHelper.ShowLayout (cr, layout);
            cr.Fill ();
        }
        
        public string Markup {
            get { return markup; }
            set { markup = value; }
        }
    }
}
