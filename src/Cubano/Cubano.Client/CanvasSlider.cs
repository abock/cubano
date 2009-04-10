// 
// CanvasSlider.cs
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

using Hyena.Gui;
using Hyena.Gui.Theming;

namespace Hyena.Gui.Canvas
{    
    public class CanvasSlider : CanvasItem
    {
        public CanvasSlider ()
        {
        }
        
        private void SetPendingValueFromX (double x)
        {
            IsValueUpdatePending = true;
            PendingValue = x / Width;
        }
        
        protected override void OnButtonPress (double x, double y, uint button)
        {
            SetPendingValueFromX (x);
            base.OnButtonPress (x, y, button);
        }

        protected override void OnButtonRelease ()
        {
            Value = PendingValue;
            IsValueUpdatePending = false;
            base.OnButtonRelease ();
        }
        
        protected override void OnPointerMotion (double x, double y)
        {
            if (ButtonPressed >= 0) {
                SetPendingValueFromX (x);
            }
        
            base.OnPointerMotion (x, y);
        }
        
        protected override void ClippedRender (Cairo.Context cr)
        {   
            int steps = ShadowSize;
        
            double throbber_r = ThrobberSize / 2;
            
            double bar_x = throbber_r;
            double bar_y = ThrobberSize <= BarSize ? 0 : Math.Round ((ThrobberSize - BarSize) / 2);
            double bar_w = InnerWidth - 2 * throbber_r;
            double bar_h = BarSize;
            
            double fill_x = bar_x + steps;
            double fill_y = bar_y + steps;
            double fill_w = bar_w * Value - 2 * steps;
            double fill_h = bar_h - 2 * steps;
            
            if (fill_w < 0) {
                fill_w = 0;
            }
            
            cr.Translate (0, 0.5);
            
            double throbber_o = Math.Max (throbber_r, steps) + 1;
            double throbber_x = throbber_o + (InnerWidth - 2 * throbber_o) * (IsValueUpdatePending ? PendingValue : Value);
            double throbber_y = (BarSize <= ThrobberSize ? 0 : Math.Round ((BarSize - ThrobberSize) / 2)) + throbber_r;
            
            throbber_x = Math.Round (throbber_x);
            fill_w = Math.Round (fill_w);
            
            Color color = Theme.Colors.GetWidgetColor (GtkColorClass.Dark, Gtk.StateType.Active);

            for (int i = 0; i < steps; i++) {
                CairoExtensions.RoundedRectangle (cr, bar_x + i, bar_y + i,
                    bar_w - 2 * i, bar_h - 2 * i - 1, steps - i);
                
                color.A = i == steps - 1 
                    ? 0.4
                    : 0.075 + i * 0.09;
                
                cr.Color = color;
                cr.LineWidth = 1.0;
                cr.Stroke ();
            }
            
            cr.Translate (0, -0.5);
            
            Color fill_color = CairoExtensions.ColorShade (color, 0.4);
            Color light_fill_color = CairoExtensions.ColorShade (color, 0.8);
            fill_color.A = 1.0;
            light_fill_color.A = 1.0;
            
            LinearGradient fill = new LinearGradient (fill_x, fill_y, fill_x, fill_y + fill_h);
            fill.AddColorStop (0, light_fill_color);
            fill.AddColorStop (0.5, fill_color);
            fill.AddColorStop (1, light_fill_color);
            
            cr.Rectangle (fill_x, fill_y, fill_w + 1, fill_h);
            cr.Pattern = fill;
            cr.Fill ();
            
            cr.Color = fill_color;
            cr.Arc (throbber_x, throbber_y, throbber_r, 0, Math.PI * 2);
            cr.Fill ();
        }

        public override void SizeRequest (out double width, out double height)
        {
            base.SizeRequest (out width, out height);
            width += 4 * Height;
            height += Height;
        }
        
        public override double Height {
            get { return Math.Max (ThrobberSize, BarSize) + PaddingTop + PaddingBottom; }
            set { }
        }
        
        private int shadow_size = 4;
        public virtual int ShadowSize {
            get { return shadow_size; }
            set { shadow_size = value; }
        }
        
        private double bar_size = 11;
        public virtual double BarSize {
            get { return bar_size; }
            set { bar_size = value; }
        }
        
        private double throbber_size = 7;
        public virtual double ThrobberSize {
            get { return throbber_size; }
            set { throbber_size = value; }
        }
        
        private double value;
        public virtual double Value {
            get { return this.value; }
            set {
                if (value < 0.0 || value > 1.0) {
                    throw new ArgumentOutOfRangeException ("Value", "Must be between 0.0 and 1.0 inclusive");
                } else if (this.value == value) {
                    return;
                }
                
                this.value = value;
                OnRerender ();
            }
        }
        
        private bool is_value_update_pending;
        public virtual bool IsValueUpdatePending {
            get { return is_value_update_pending; }
            set { is_value_update_pending = value; }
        }
        
        private double pending_value;
        public virtual double PendingValue {
            get { return pending_value; }
            set {
                if (value < 0.0 || value > 1.0) {
                    throw new ArgumentOutOfRangeException ("Value", "Must be between 0.0 and 1.0 inclusive");
                } else if (pending_value == value) {
                    return;
                }
                
                pending_value = value;
                OnRerender ();
            }
        }
    }
}
