// 
// Panel.cs
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
    public class Panel : CanvasItem
    {
        private CanvasItemCollection children;
    
        public Panel ()
        {
            children = new CanvasItemCollection (this);
        }

        public override Size Measure (Size available)
        {
            Size size = new Size (0, 0);
            
            foreach (var child in Children) {
                if (child.Visible) {
                    var child_size = child.Measure (available);
                    if (child_size.Width > size.Width) size.Width = child_size.Width;
                    if (child_size.Height > size.Height) size.Height = child_size.Height;
                }
            }
            
            size.Width += Margin.Left + Margin.Right;
            size.Height += Margin.Top + Margin.Bottom;
            
            return DesiredSize = size;
        }
        
        public override void Arrange ()
        {
            foreach (var child in Children) {
                if (!child.Visible) {
                    continue;
                }
                                
                child.Allocation = new Rect (0, 0, 
                    Math.Min (ContentAllocation.Width, child.DesiredSize.Width), 
                    Math.Min (ContentAllocation.Height, child.DesiredSize.Height));
                    
                child.Arrange ();
            }
        }

        protected override void ClippedRender (Cairo.Context cr)
        {
            foreach (var child in Children) {
                if (child.Visible) {
                    child.Render (cr);
                }
            }
        }
        
        public CanvasItemCollection Children {
            get { return children; }
        }
    }
}
