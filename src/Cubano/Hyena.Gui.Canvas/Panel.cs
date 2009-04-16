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

        public override Size Measure ()
        {
            Size size = new Size (0, 0);
            
            foreach (var child in Children) {
                if (child.Visible) {
                    var child_size = child.Measure ();
                    if (child_size.Width > size.Width) size.Width = child_size.Width;
                    if (child_size.Height > size.Height) size.Height = child_size.Height;
                }
            }
            
            Console.WriteLine ("MAX CHILD SIZE: {0}", size);
            
            size.Width += Margin.Left + Margin.Right;
            size.Height += Margin.Top + Margin.Bottom;
            
            Console.WriteLine ("DESIRED PARENT SIZE: {0}", size);
            
            DesiredSize = size;
            return DesiredSize;
        }
        
        public override void Arrange ()
        {
            foreach (var child in Children) {
                if (!child.Visible) {
                    continue;
                }
                
                double max_width = Math.Max (0, Allocation.Width - Margin.Left - Margin.Right);
                double max_height = Math.Max (0, Allocation.Width - Margin.Left - Margin.Right);
                
                Size size = new Size (0, 0);
                size.Width = Math.Min (max_width, child.DesiredSize.Width);
                size.Height = Math.Min (max_height, child.DesiredSize.Height);
                child.ActualSize = size;
                
                Console.WriteLine ("DESIRED CHILD: {0}x{1}", child.DesiredSize.Width, child.DesiredSize.Height);
                
                child.Allocation = new Rect (0, 0, size.Width, size.Height);
                child.Arrange ();
            }
        }

        protected override void ClippedRender (Cairo.Context cr)
        {
            cr.Color = new Cairo.Color (1, 0, 0, 0.5);
            cr.Rectangle (0, 0, RenderWidth, RenderHeight);
            cr.Fill ();
        
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
