//
// CubanoTheme.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//
// Copyright (C) 2007-2009 Novell, Inc.
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
using Cairo;
using Gtk;

using Hyena.Gui;
using Hyena.Gui.Theming;

namespace Cubano.Client
{
    public class CubanoTheme : GtkTheme
    {
        private Cairo.Color rule_color;
        private Cairo.Color border_color;

        public CubanoTheme (Widget widget) : base (widget)
        {
        }

        protected override void OnColorsRefreshed ()
        {
            base.OnColorsRefreshed ();

            rule_color = CairoExtensions.ColorShade (ViewFill, 0.95);
            border_color = Colors.GetWidgetColor (GtkColorClass.Dark, StateType.Active);
        }

        public override void DrawFrameBackground (Cairo.Context cr, Gdk.Rectangle alloc, Cairo.Color color, Cairo.Pattern pattern)
        {
            // Hack to only render the black background for now playing
            if (pattern == null && color.R != 0 && color.G != 0 && color.B != 0) {
                return;
            }
        
            color.A = Context.FillAlpha;
            if (pattern != null) {
                cr.Pattern = pattern;
            } else {
                cr.Color = color;
            }
            cr.Rectangle (alloc.X, alloc.Y, alloc.Width, alloc.Height);
            cr.Fill ();
        }
        
        public override void DrawFrameBorder (Cairo.Context cr, Gdk.Rectangle alloc)
        {
            /*cr.LineWidth = BorderWidth;
            cr.Color = border_color;
            double offset = (double)BorderWidth / 2.0;
            CairoExtensions.RoundedRectangle (cr, alloc.X + offset, alloc.Y + offset,
                alloc.Width - BorderWidth, alloc.Height - BorderWidth, Context.Radius, CairoCorners.All);
            cr.Stroke();*/
        }
        
        public override void DrawColumnHighlight (Cairo.Context cr, Gdk.Rectangle alloc, Cairo.Color color)
        {
            Cairo.Color light_color = CairoExtensions.ColorShade (color, 1.6);
            Cairo.Color dark_color = CairoExtensions.ColorShade (color, 1.3);
            
            LinearGradient grad = new LinearGradient (alloc.X, alloc.Y, alloc.X, alloc.Bottom - 1);
            grad.AddColorStop (0, light_color);
            grad.AddColorStop (1, dark_color);
            
            cr.Pattern = grad;
            cr.Rectangle (alloc.X + 1.5, alloc.Y + 1.5, alloc.Width - 3, alloc.Height - 2);
            cr.Fill ();
            grad.Destroy ();
        }

        public override void DrawHeaderBackground (Cairo.Context cr, Gdk.Rectangle alloc)
        {
            /*Cairo.Color gtk_background_color = Colors.GetWidgetColor (GtkColorClass.Background, StateType.Normal);
            Cairo.Color light_color = CairoExtensions.ColorShade (gtk_background_color, 1.1);
            Cairo.Color dark_color = CairoExtensions.ColorShade (gtk_background_color, 0.95);
            
            CairoCorners corners = CairoCorners.TopLeft | CairoCorners.TopRight;

            LinearGradient grad = new LinearGradient (alloc.X, alloc.Y, alloc.X, alloc.Bottom);
            grad.AddColorStop (0, light_color);
            grad.AddColorStop (0.75, dark_color);
            grad.AddColorStop (0, light_color);
        
            cr.Pattern = grad;
            CairoExtensions.RoundedRectangle (cr, alloc.X, alloc.Y, alloc.Width, alloc.Height, Context.Radius, corners);
            cr.Fill ();
            
            cr.Color = border_color;
            cr.Rectangle (alloc.X, alloc.Bottom, alloc.Width, BorderWidth);
            cr.Fill ();
            grad.Destroy ();*/
            
            Cairo.Color gtk_background_color = Colors.GetWidgetColor (GtkColorClass.Background, StateType.Normal);
            Cairo.Color dark_color = CairoExtensions.ColorShade (gtk_background_color, 0.80);
            cr.Color = dark_color;
            cr.MoveTo (alloc.X, alloc.Bottom + 0.5);
            cr.LineTo (alloc.Right, alloc.Bottom + 0.5);
            cr.LineWidth = 1.0;
            cr.Stroke ();
        }
        
        public override void DrawHeaderSeparator (Cairo.Context cr, Gdk.Rectangle alloc, int x)
        {
            Cairo.Color gtk_background_color = Colors.GetWidgetColor (GtkColorClass.Background, StateType.Normal);
            Cairo.Color dark_color = CairoExtensions.ColorShade (gtk_background_color, 0.80);
            Cairo.Color light_color = CairoExtensions.ColorShade (gtk_background_color, 1.1);
            
            int y_1 = alloc.Top + 4;
            int y_2 = alloc.Bottom - 3;
            
            cr.LineWidth = 1;
            cr.Antialias = Cairo.Antialias.None;
            
            cr.Color = dark_color;
            cr.MoveTo (x, y_1);
            cr.LineTo (x, y_2);
            cr.Stroke ();
            
            cr.Color = light_color;
            cr.MoveTo (x + 1, y_1);
            cr.LineTo (x + 1, y_2);
            cr.Stroke ();
            
            cr.Antialias = Cairo.Antialias.Default;
        }
        
        public override void DrawListBackground (Context cr, Gdk.Rectangle alloc, Color color)
        {
            /*color.A = Context.FillAlpha;
            cr.Color = color;
            cr.Rectangle (alloc.X, alloc.Y, alloc.Width, alloc.Height);
            cr.Fill ();*/
        }
        
        public override void DrawRowSelection (Cairo.Context cr, int x, int y, int width, int height,
            bool filled, bool stroked, Cairo.Color color, CairoCorners corners)
        {
            color.A *= 0.85;
            Cairo.Color selection_color = color;
            Cairo.Color selection_stroke = CairoExtensions.ColorShade (selection_color, 0.85);
            selection_stroke.A = color.A;
            
            if (filled) {
                Cairo.Color selection_fill_light = CairoExtensions.ColorShade (selection_color, 1.05);
                Cairo.Color selection_fill_dark = selection_color;
                
                selection_fill_light.A = color.A;
                selection_fill_dark.A = color.A;
                
                LinearGradient grad = new LinearGradient (x, y, x, y + height);
                grad.AddColorStop (0, selection_fill_dark);
                grad.AddColorStop (1, selection_fill_light);
                
                cr.Pattern = grad;
                cr.Rectangle (x, y, width, height);
                cr.Fill ();
                grad.Destroy ();
            }
            
            if (stroked && !filled) {
                cr.LineWidth = 1.0;
                cr.Color = selection_stroke;
                cr.Rectangle (x + 0.5, y + 0.5, width - 1, height - 1);
                cr.Stroke ();
            }
        }
        
        public override void DrawRowRule(Cairo.Context cr, int x, int y, int width, int height)
        {
            /*cr.Color = new Cairo.Color (rule_color.R, rule_color.G, rule_color.B, Context.FillAlpha);
            cr.Rectangle (x, y, width, height);
            cr.Fill ();*/
        }
    }
}