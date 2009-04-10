// 
// CanvasItem.cs
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
    public interface ICanvasItem
    {
        event EventHandler<EventArgs> Resize;
        event EventHandler<EventArgs> Rerender;
        
        ICanvasItem Parent { get; set; }
    
        void Render (Context cr);
        void SizeRequest (out double width, out double height);
        
        void ButtonPress (double x, double y, uint button);
        void ButtonRelease ();
        void PointerMotion (double x, double y);
        
        Theme Theme { get; set; }
        
        double Left { get; set; }
        double Top { get; set; }
        double Width { get; set; }
        double Height { get; set; }
        double PaddingLeft { get; set; }
        double PaddingTop { get; set; }
        double PaddingRight { get; set; }
        double PaddingBottom { get; set; }
    }

    public abstract class CanvasItem : ICanvasItem
    {
        public event EventHandler<EventArgs> Resize;
        public event EventHandler<EventArgs> Rerender;
    
        public CanvasItem ()
        {
        }
        
        public virtual void Render (Context cr)
        {
            cr.Save ();
            cr.Translate (Left + PaddingLeft, Top + PaddingTop);
            cr.Antialias = Cairo.Antialias.Default;
            ClippedRender (cr);
            cr.Restore ();
        }
        
        protected virtual void ClippedRender (Context cr)
        {
        }
        
        public virtual void SizeRequest (out double width, out double height)
        {
            width = PaddingLeft + PaddingRight;
            height = PaddingTop + PaddingBottom;
        }
        
        public void ButtonPress (double x, double y, uint button)
        {
            button_pressed = (int)button;
            OnButtonPress (x, y, button);
        }
        
        public void ButtonRelease ()
        {
            button_pressed = -1;
            OnButtonRelease ();
        }
        
        public void PointerMotion (double x, double y)
        {
            OnPointerMotion (x, y);
        }
        
        protected virtual void OnButtonPress (double x, double y, uint button)
        {
        }
        
        protected virtual void OnButtonRelease ()
        {
        }
        
        protected virtual void OnPointerMotion (double x, double y)
        {
        }
        
        protected virtual void OnResize ()
        {
            EventHandler<EventArgs> handler = Resize;
            if (handler != null) {
                handler (this, EventArgs.Empty);
            }
        }
        
        protected virtual void OnRerender ()
        {
            EventHandler<EventArgs> handler = Rerender;
            if (handler != null) {
                handler (this, EventArgs.Empty);
            }
        }
        
        private ICanvasItem parent;
        public ICanvasItem Parent {
            get { return parent; }
            set { parent = value; }
        }
        
        protected double InnerTop {
            get { return Top + PaddingTop; }
        }
        
        protected double InnerLeft {
            get { return Left + PaddingLeft; }
        }
        
        protected double InnerWidth {
            get { return Width - PaddingLeft - PaddingRight; }
        }
        
        protected double InnerHeight {
            get { return Height - PaddingTop - PaddingBottom; }
        }
        
        private Theme theme;
        public virtual Theme Theme {
            get { return theme ?? (Parent == null ? null : Parent.Theme); }
            set { theme = value; }
        }
        
        private double width;
        public virtual double Width {
            get { return width; }
            set { width = value; }
        }
        
        private double height;
        public virtual double Height {
            get { return height; }
            set { height = value; }
        }
        
        private double left;
        public virtual double Left {
            get { return left; }
            set { left = value; }
        }
        
        private double top;
        public virtual double Top {
            get { return top; }
            set { top = value; }
        }
        
        private double padding_left;
        public virtual double PaddingLeft {
            get { return padding_left; }
            set { padding_left = value; }
        }
        
        private double padding_right;
        public virtual double PaddingRight {
            get { return padding_right; }
            set { padding_right = value; }
        }
        
        private double padding_top;
        public virtual double PaddingTop {
            get { return padding_top; }
            set { padding_top = value; }
        }
        
        private double padding_bottom;
        public virtual double PaddingBottom {
            get { return padding_bottom; }
            set { padding_bottom = value; }
        }
        
        private int button_pressed = -1;
        internal protected int ButtonPressed {
            get { return button_pressed; }
            set { button_pressed = value; }
        }
    }
}
