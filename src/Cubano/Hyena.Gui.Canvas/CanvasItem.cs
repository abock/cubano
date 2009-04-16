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
using System.Collections.Generic;

using Hyena.Gui.Theming;

namespace Hyena.Gui.Canvas
{
    public class CanvasItem
    {
        private CanvasManager manager;
        private CanvasItem parent;
        private Theme theme;
        private Thickness margin;
        private Size desired_size;
        private Size actual_size;
        private double height = Double.NaN;
        private double width = Double.NaN;
        private Dictionary<string, object> properties;
        private Rect allocation;
        private bool visible = true;
        
        public event EventHandler<EventArgs> SizeChanged;
        public event EventHandler<EventArgs> LayoutUpdated;
        
        public CanvasItem ()
        {
        }
        
        public void InvalidateArrange ()
        {
            CanvasItem root = RootAncestor;
            if (root != null && root.Manager != null) {
                root.Manager.QueueArrange (this);
            }
        }
        
        public void InvalidateMeasure ()
        {
            CanvasItem root = RootAncestor;
            if (root != null && root.Manager != null) {
                root.Manager.QueueMeasure (this);
            }
        }
        
        public virtual void Arrange ()
        {
            ActualSize = Measure ();
        }
        
        public virtual Size Measure ()
        {
            return DesiredSize = new Size (
                Width == Double.NaN ? Allocation.Width : (Width + Margin.Left + Margin.Right),
                Height == Double.NaN ? Allocation.Height : (Height + Margin.Top + Margin.Bottom)
            );
        }
        
        public virtual void Render (Cairo.Context cr)
        {
            cr.Save ();
            cr.Translate (Allocation.Left + Margin.Left, Allocation.Top + Margin.Top);
            cr.Antialias = Cairo.Antialias.Default;
            ClippedRender (cr);
            cr.Restore ();
        }
        
        protected virtual void ClippedRender (Cairo.Context cr)
        {
        }
        
        protected virtual void OnSizeChanged ()
        {
            EventHandler<EventArgs> handler = SizeChanged;
            if (handler != null) {
                handler (this, EventArgs.Empty);
            }
        }
        
        protected virtual void OnLayoutUpdated ()
        {
            EventHandler<EventArgs> handler = LayoutUpdated;
            if (handler != null) {
                handler (this, EventArgs.Empty);
            }
        }
        
        internal CanvasManager Manager {
            get { return manager; }
            set { manager = value; }
        }
        
        public CanvasItem RootAncestor {
            get {
                CanvasItem root = Parent;
                while (root != null) {
                    root = root.Parent;
                }
                return root;
            }
        }
        
        public CanvasItem Parent {
            get { return parent; }
            set { parent = value; }
        }
        
        public Theme Theme {
            get { return theme ?? (Parent != null ? Parent.Theme : null); }
            set { theme = value; }
        }
        
        public Thickness Margin {
            get { return margin; }
            set { margin = value; }
        }
        
        public Size DesiredSize {
            get { return desired_size; }
            protected set { desired_size = value; }
        }
        
        public Size ActualSize {
            get { return actual_size; }
            set { actual_size = value; }
        }
        
        public double Width {
            get { return width; }
            set { width = value; }
        }
        
        public double Height {
            get { return height; }
            set { height = value; }
        }
        
        protected double RenderWidth {
            get { return Math.Max (0, Allocation.Width - Margin.Left - Margin.Right); }
        }
        
        protected double RenderHeight {
            get { return Math.Max (0, Allocation.Height - Margin.Top - Margin.Bottom); }
        }
        
        public Rect Allocation {
            get { return allocation; }
            set { allocation = value; }
        }
        
        public bool Visible {
            get { return visible; }
            set { visible = value; }
        }
        
        public object this[string property] {
            get {
                object result;
                if (properties != null && properties.TryGetValue (property, out result)) {
                    return result;
                }
                return result;
            }
            
            set {
                if (properties == null) {
                    properties = new Dictionary<string, object> ();
                }
                
                if (properties.ContainsKey (property)) {
                    properties[property] = value;
                } else {
                    properties.Add (property, value);
                }
            }
        }
        
        public T GetValue<T> (string property)
        {
            object result = this[property];
            if (result == null) {
                return default (T);
            }
            
            try {
                result = Convert.ChangeType (result, typeof (T));
            } catch {
                return default (T);
            }
            
            return (T)result;
        }
        
        public void SetValue (string property, object value)
        {
            this[property] = value;
        }
    }
}
