// 
// CubanoWindowDecorator.cs
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
using Cairo;
using Gtk;
using Hyena.Gui;

namespace Cubano.Client
{
    public class CubanoWindowDecorator : WindowDecorator
    {
        public CubanoWindowDecorator (Gtk.Window window) : base (window)
        {
        }
        
        public override void Render (Context cr)
        {
            if (!CanResize) {
                return;
            }
        
            var selected_color = CairoExtensions.GdkColorToCairoColor (Window.Style.Dark (StateType.Active));
            var grad = new LinearGradient (0, 0, 0, Allocation.Height);
            
            selected_color.A = 0.4;
            grad.AddColorStop (0, selected_color);
            selected_color.A = 1.0;
            grad.AddColorStop (1, selected_color);
            
            cr.Pattern = grad;
            cr.LineWidth = 1.0;
            cr.Rectangle (0.5, 0.5, Allocation.Width - 1, Allocation.Height - 1);
            cr.Stroke ();
            
            selected_color.A = 0.5;
            cr.Color = selected_color;
            
            double handle_size = 8;
            double ty = 0.5 + Allocation.Height - handle_size - 3;
            double tx = 0.5 + (Window.Direction == TextDirection.Ltr
                ? Allocation.Width - handle_size - 3
                : 3);
                
            cr.Translate (tx, ty);
            
            for (double i = 0; i < 3; i++) {
                if (Window.Direction == TextDirection.Ltr) {
                    cr.MoveTo (i * 3, handle_size);
                    cr.LineTo (handle_size, i * 3);
                } else {
                    cr.MoveTo (0, i * 3);
                    cr.LineTo (handle_size - i * 3, handle_size);
                }
            }
            
            cr.Stroke ();
            
            cr.Translate (-tx, -ty);
        }
    }
}
