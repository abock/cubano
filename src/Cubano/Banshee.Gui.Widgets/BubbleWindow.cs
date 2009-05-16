// 
// BubbleWindow.cs
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
    public class BubbleWindow : ShapedPopupWindow
    {
        private int arrow_size;
        private int corner_radius;
    
        public BubbleWindow (Widget context) : base (context)
        {
            ConfigureDefault ();
        }
        
        private void ConfigureDefault ()
        {
            ArrowSize = 20;
            CornerRadius = 5;
        }

        protected override void DrawShape (Cairo.Context cr)
        {
            cr.SetSourceRGBA (0, 0, 0, 0);
            cr.Operator = Cairo.Operator.Source;
            cr.Paint ();
            
            var box_height = Allocation.Height - ArrowSize;
            var arrow_x = (Allocation.Width - ArrowSize) / 2;
            
            cr.NewPath ();
            
            CairoExtensions.RoundedRectangle (cr, 0, 0, 
                Allocation.Width, box_height, CornerRadius);
                
            cr.MoveTo (arrow_x, box_height);
            cr.LineTo (Allocation.Width / 2, Allocation.Height);
            cr.LineTo (arrow_x + ArrowSize, box_height);
            
            cr.ClosePath ();

            var grad = new Cairo.LinearGradient (0, 0, 0, Allocation.Height);
            grad.AddColorStop (0, new Cairo.Color (0.3, 0.3, 0.3, 0.9));
            grad.AddColorStop (1, new Cairo.Color (0.1, 0.1, 0.1, 0.9));
            
            cr.Operator = Cairo.Operator.Over;
            cr.Pattern = grad;
            cr.Fill ();
            
            grad.Destroy ();
        }
        
        public int ArrowSize {
            get { return arrow_size; }
            set {
                arrow_size = Math.Max (0, value);
                MarginBottom = arrow_size + CornerRadius;
                if (IsRealized) {
                    ShapeWindow ();
                }
            }
        }
        
        public int CornerRadius {
            get { return corner_radius; }
            set {
                corner_radius = Math.Max (0, value);
                MarginLeft 
                    = MarginRight 
                    = MarginTop
                    = corner_radius;
                MarginBottom = corner_radius + ArrowSize;
            }
        }
    }
}
