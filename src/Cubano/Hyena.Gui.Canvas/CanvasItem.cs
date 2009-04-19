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
        private Size desired_size;
        private Dictionary<string, object> properties;
        private Rect allocation;
        private bool visible = true;
        
        public event EventHandler<EventArgs> SizeChanged;
        public event EventHandler<EventArgs> LayoutUpdated;
        
        public CanvasItem ()
        {
            InstallProperty<double> ("Opacity", 1.0);
            InstallProperty<double> ("Width", Double.NaN);
            InstallProperty<double> ("Height", Double.NaN);
            InstallProperty<Thickness> ("Margin", new Thickness (0));
            InstallProperty<Brush> ("Foreground", Brush.Black);
            InstallProperty<Brush> ("Background", Brush.White);
            InstallProperty<MarginStyle> ("MarginStyle", MarginStyle.None);
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
                
                double opacity = Opacity;
                if (opacity < 1.0) {
                    cr.PushGroup ();
                }
                
                MarginStyle margin_style = MarginStyle;
                if (margin_style != null && margin_style != MarginStyle.None) {
                    cr.Translate (Math.Round (Allocation.X), Math.Round (Allocation.Y));
                    cr.Save ();
                    margin_style.Apply (this, cr);
                    cr.Restore ();
                    cr.Translate (Math.Round (Margin.Left), Math.Round (Margin.Top));
                } else {
                    cr.Translate (Math.Round (ContentAllocation.X), Math.Round (ContentAllocation.Y));
                }
                
                cr.Antialias = Cairo.Antialias.Default;
                ClippedRender (cr);
                
                if (opacity < 1.0) {
                    cr.PopGroupToSource ();
                    cr.PaintWithAlpha (Opacity);
                }
                
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
        
        public Size DesiredSize {
            get { return desired_size; }
            protected set { desired_size = value; }
        }
        
        public Thickness Margin {
            get { return GetValue<Thickness> ("Margin"); }
            set { SetValue<Thickness> ("Margin", value); }
        }
        
        public MarginStyle MarginStyle {
            get { return GetValue<MarginStyle> ("MarginStyle"); }
            set { SetValue<MarginStyle> ("MarginStyle", value); }
        }
        
        public double Width {
            get { return GetValue<double> ("Width"); }
            set { SetValue<double> ("Width", value); }
        }
        
        public double Height {
            get { return GetValue<double> ("Height"); }
            set { SetValue<double> ("Height", value); }
        }
        
        public double Opacity {
            get { return GetValue<double> ("Opacity"); }
            set { SetValue<double> ("Opacity", value); }
        }
        
        public Brush Foreground {
            get { return GetValue<Brush> ("Foreground"); }
            set { SetValue<Brush> ("Foreground", value); }
        }
        
        public Brush Background {
            get { return GetValue<Brush> ("Background"); }
            set { SetValue<Brush> ("Background", value); }
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
        
        protected virtual bool OnPropertyChange (string property, object value)
        {
            switch (property) {
                case "Foreground":
                case "Background":
                case "Opacity":
                case "MarginStyle":
                    InvalidateRender ();
                    return true;
                /* case "Width":
                case "Height":
                case "Margin":
                    InvalidateArrange ();
                    InvalidateMeasure ();
                    return true; */
            }
            
            return false;
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
                }
                
                object existing = properties[property];
                
                Type existing_type = existing.GetType ();
                Type new_type = value.GetType ();
                
                if (existing_type != new_type && !new_type.IsSubclassOf (existing_type)) {
                    throw new InvalidOperationException ("Invalid value type " + 
                        value.GetType () + " for property: " + property);
                }
                
                if (existing != value) {
                    properties[property] = value;
                    OnPropertyChange (property, value);
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
