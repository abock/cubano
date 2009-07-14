// 
// ScalableLabel.cs
//  
// Author:
//   Aaron Bockover <abockover@novell.com>
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

namespace Banshee.Gui.Widgets
{
    public class ScalableLabel : Label
    {
        private int base_point_size;
    
        public ScalableLabel () : base (String.Empty)
        {
        }
        
        protected override void OnRealized ()
        {
            base.OnRealized ();
        }
        
        protected override void OnStyleSet (Style previous)
        {
            base.OnStyleSet (previous);
            base_point_size = PangoContext.FontDescription.Size;
            Layout.FontDescription = PangoContext.FontDescription.Copy ();
            UpdateScale ();
        }
        
        private void UpdateScale ()
        {
            if (Layout.FontDescription != null) {
                Layout.FontDescription.Size = (int)Math.Round (base_point_size * CurrentFontSizeEm);
            }
        }

        private double default_font_size_em = 1.0;
        public double DefaultFontSizeEm {
            get { return default_font_size_em; }
            set { default_font_size_em = value; }
        }
        
        private double current_font_size_em = 1.0;
        public double CurrentFontSizeEm {
            get { return current_font_size_em; }
            set {
                current_font_size_em = value;
                UpdateScale ();
            }
        }
    }
}
