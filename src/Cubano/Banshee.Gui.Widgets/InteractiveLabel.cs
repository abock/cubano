// 
// InteractiveLabel.cs
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
    public class InteractiveLabel : ScalableLabel
    {
        private bool changing_style = false;
    
        protected override void OnStyleSet (Style previous)
        {
            base.OnStyleSet (previous);
            if (changing_style) {
                return;
            }
            
            changing_style = true;
            
            ModifyFg (StateType.Selected, Style.Text (StateType.Normal));
            ModifyFg (StateType.Normal, Hyena.Gui.Theming.GtkTheme.GetGdkTextMidColor (this));
        
            changing_style = false;
        }
        
        private bool is_active = false;
        public bool IsActive {
            get { return is_active; }
            set {
                is_active = value;
                if (is_active) {
                    State = StateType.Selected;
                    CurrentFontSizeEm = ActiveFontSizeEm;
                } else {
                    State = StateType.Normal;
                    CurrentFontSizeEm = DefaultFontSizeEm;
                }
            }
        }
        
        public double active_font_size_em = 1.0;
        public double ActiveFontSizeEm {
            get { return active_font_size_em; }
            set { active_font_size_em = value; }
        }
    }
}
