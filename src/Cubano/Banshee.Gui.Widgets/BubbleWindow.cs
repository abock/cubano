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
using Hyena.Gui.Theming;

namespace Banshee.Gui.Widgets
{
    public class BubbleWindow : ShapedPopupWindow
    {
        private int arrow_size;
        private int corner_radius;
        
        private Theme theme;
    
        public BubbleWindow ()
        {
            ConfigureDefault ();
        }
        
        private void ConfigureDefault ()
        {
            ArrowSize = 20;
            CornerRadius = 5;
            PopupXAlign = 0.5;
            PopupYAlign = 1.0;
        }
        
        protected override void OnRealized ()
        {
            base.OnRealized ();
        }
        
        protected override bool OnExposeEvent (Gdk.EventExpose evnt)
        {
            using (var cr = Gdk.CairoHelper.Create (evnt.Window)) {
                DrawShape (cr);
            }
        
            return true;
        }
        
        protected override void DrawShape (Cairo.Context cr)
        {
            theme = theme ?? Hyena.Gui.Theming.ThemeEngine.CreateTheme (this);
        
            cr.SetSourceRGBA (0, 0, 0, 0);
            cr.Operator = Cairo.Operator.Source;
            cr.Paint ();
            
            var box_height = Allocation.Height - ArrowSize;
            var arrow_x = (Allocation.Width - ArrowSize) / 2;
            /*
            cr.NewPath ();
            
            CairoExtensions.RoundedRectangle (cr, 0, 0, 
                Allocation.Width, box_height, CornerRadius);

            cr.Operator = Cairo.Operator.In;

            cr.MoveTo (arrow_x, box_height);
            cr.LineTo (Allocation.Width / 2, Allocation.Height);
            cr.LineTo (arrow_x + ArrowSize, box_height);
            
            cr.ClosePath ();

            var grad = new Cairo.LinearGradient (0, 0, 0, Allocation.Height);
            grad.AddColorStop (0, theme.Colors.GetWidgetColor (GtkColorClass.Background, StateType.Normal));
            grad.AddColorStop (1, theme.Colors.GetWidgetColor (GtkColorClass.Background, StateType.Active));
            
            cr.Operator = Cairo.Operator.Over;
            cr.Pattern = grad;
            cr.FillPreserve ();
            
            cr.Color = theme.Colors.GetWidgetColor (GtkColorClass.Dark, StateType.Active);
            cr.Stroke ();*/
            
            
            double x = 0, y = 0;
            double w = Allocation.Width;
            double h = box_height;
            double r = 20;
            
            cr.NewPath ();
            
            cr.MoveTo (x + r, y);
            cr.LineTo (x + w - r, y);
            cr.Arc (x + w - r, y + r, r, Math.PI * 1.5, Math.PI * 2);
            cr.LineTo (x + w, y + h - r);
            cr.Arc (x + w - r, y + h - r, r, 0, Math.PI * 0.5);
            cr.LineTo (x + r, y + h);
            cr.Arc (x + r, y + h - r, r, Math.PI * 0.5, Math.PI);
            cr.LineTo (x, y + r);
            cr.Arc (x + r, y + r, r, Math.PI, Math.PI * 1.5);
            
            cr.ClosePath ();
            
            cr.Color = new Cairo.Color (0, 0, 0, 1);
            cr.Fill ();
            
            
            //grad.Destroy ();
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
