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
        private double height = Double.NaN;
        private double width = Double.NaN;
        private Dictionary<string, object> properties;
        private Rect allocation;
        private bool visible = true;
        
        public event EventHandler<EventArgs> SizeChanged;
        public event EventHandler<EventArgs> LayoutUpdated;
        
        public CanvasItem ()
        {
            InstallProperty<double> ("Opacity", 1.0);
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

        protected void InvalidateRender ()
        {
            InvalidateRender (Allocation);
        }

        protected void InvalidateRender (Rect area)
        {
            CanvasItem root = RootAncestor;
            if (root != null && root.Manager != null) {
                root.Manager.QueueRender (this, area);
            }
        }
        
        public virtual void Arrange ()
        {
        }
        
        public virtual Size Measure (Size available)
        {
            double m_x = Margin.Left + Margin.Right;
            double m_y = Margin.Top + Margin.Bottom;
            
            double a_w = available.Width - m_x;
            double a_h = available.Height - m_y;
            
            return DesiredSize = new Size (
                Math.Max (0, Math.Min (a_w, Double.IsNaN (Width) ? a_w : Width + m_x)),
                Math.Max (0, Math.Min (a_h, Double.IsNaN (Height) ? a_h : Height + m_y))
            );
        }
        
        public virtual void Render (Cairo.Context cr)
        {
            if (ContentAllocation.Width > 0 && ContentAllocation.Height > 0) {
                cr.Save ();
                cr.Translate (Math.Round (ContentAllocation.X), Math.Round (ContentAllocation.Y));
                cr.Antialias = Cairo.Antialias.Default;
                ClippedRender (cr);
                cr.Restore ();
            }
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
            get { return manager ?? (Parent != null ? Parent.Manager : null); }
            set { manager = value; }
        }
        
        public CanvasItem RootAncestor {
            get {
                CanvasItem root = Parent ?? this;
                while (root != null && root.Parent != null) {
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
        
        public double Width {
            get { return width; }
            set { width = value; }
        }
        
        public double Height {
            get { return height; }
            set { height = value; }
        }

        public Rect Allocation {
            get { return allocation; }
            set { allocation = value; }
        }
        
        public Rect ContentAllocation {
            get {
                return new Rect (
                    Allocation.X + Margin.Left, 
                    Allocation.Y + Margin.Top,
                    Math.Max (0, Allocation.Width - Margin.Left - Margin.Right),
                    Math.Max (0, Allocation.Height - Margin.Top - Margin.Bottom)
                );
            }
        }
        
        public Size ContentSize {
            get { return new Size (ContentAllocation.Width, ContentAllocation.Height); }
        }
        
        protected Size RenderSize {
            get { return new Size (Math.Round (ContentAllocation.Width), Math.Round (ContentAllocation.Height)); }
        }
        
        public bool Visible {
            get { return visible; }
            set { visible = value; }
        }
        
        protected void InstallProperty<T> (string property)
        {
            InstallProperty (property, default (T));
        }
        
        protected void InstallProperty<T> (string property, T defaultValue)
        {
            if (properties == null) {
                properties = new Dictionary<string, object> ();
            }
            
            if (properties.ContainsKey (property)) {
                throw new InvalidOperationException ("Property is already installed: " + property);
            }
            
            properties.Add (property, defaultValue);
        }
        
        protected virtual void OnPropertyChange (string property, object value)
        {
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
                if (properties == null || !properties.ContainsKey (property)) {
                    throw new InvalidOperationException ("Property does not exist: " + property);
                } else if (properties[property].GetType () != value.GetType ()) {
                    throw new InvalidOperationException ("Invalid value type " + 
                        value.GetType () + " for property: " + property);
                }
                
                if (properties[property] != value) {
                    properties[property] = value;
                    OnPropertyChange (property, value);
                    InvalidateRender ();
                }
            }
        }
        
        public T GetValue<T> (string property)
        {
            return GetValue (property, default (T));
        }
        
        public T GetValue<T> (string property, T fallback)
        {
            object result = this[property];
            if (result == null) {
                return fallback;
            }
            
            try {
                result = Convert.ChangeType (result, typeof (T));
            } catch {
                return fallback;
            }
            
            return (T)result;
        }
        
        public void SetValue<T> (string property, T value)
        {
            this[property] = value;
        }
        
        public T Animate<T> (T animation) where T : Animation
        {
            animation.Item = this;
            AnimationManager.Instance.Animate (animation);
            return animation;
        }
        
        public DoubleAnimation AnimateDouble (string property)
        {
            return Animate (new DoubleAnimation (property));
        }
    }
}
