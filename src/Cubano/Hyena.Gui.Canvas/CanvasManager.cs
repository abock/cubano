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

namespace Hyena.Gui.Canvas
{
    public class CanvasManager
    {
        private object host;
    
        public CanvasManager (object host)
        {
            this.host = host;
        }
        
        public void QueueArrange (CanvasItem item)
        {
            item.Arrange ();
        }
        
        public void QueueMeasure (CanvasItem item)
        {
            item.Measure (item.ContentSize);
        }
        
        public void QueueRender (CanvasItem item, Rect rect)
        {
            Gtk.Widget widget = Host as Gtk.Widget;
            if (widget == null) {
                return;
            }
            
            if (rect == Rect.Empty) {
                widget.QueueDraw ();
                return;
            }
            
            double x = 0, y = 0;
            CanvasItem parent = item;
            
            while (parent != null) {
                x += parent.ContentAllocation.X;
                y += parent.ContentAllocation.Y;
                parent = parent.Parent;
            }
            
            widget.QueueDrawArea (
                (int)Math.Floor (x),
                (int)Math.Floor (y),
                (int)Math.Ceiling (rect.Width),
                (int)Math.Ceiling (rect.Height)
            );
        }
        
        public object Host {
            get { return host; }
        }
    }
}
