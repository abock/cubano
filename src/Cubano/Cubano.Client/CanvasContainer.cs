// 
// CanvasContainer.cs
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

namespace Hyena.Gui.Canvas2
{
    public class CanvasContainer : CanvasItem
    {
        private List<ICanvasItem> children = new List<ICanvasItem> ();
        protected List<ICanvasItem> Children {
            get { return children; }
        }
        
        public CanvasContainer ()
        {
        }
        
        public override void SizeRequest (out double width, out double height)
        {
            width = height = 0;
            foreach (var child in children) {
                if (!child.Visible) {
                    continue;
                }
                
                double w, h;
                child.SizeRequest (out w, out h);
                if (h > height) height = h;
                if (w > width) width = w;
            }
        }
        
        public override void Layout ()
        {
            foreach (var child in children) {
                if (!child.Visible) {
                    continue;
                }
                
                child.Left = 0;
                child.Top = 0;
                child.Width = Width;
                child.Height = Height;
                child.Layout ();
            }
        }
        
        public override void Render (Cairo.Context cr)
        {
            foreach (var child in children) {
                if (child.Visible) {
                    child.Render (cr);
                }
            }
        }
        
        public void Add (ICanvasItem child)
        {
            if (!children.Contains (child)) {
                children.Add (child);
                child.Parent = this;
                Layout ();
            }
        }
        
        private void Unparent (ICanvasItem child)
        {
            child.Parent = null;
        }
        
        public void Remove (ICanvasItem child)
        {
            if (children.Remove (child)) {
                Unparent (child);
                Layout ();
            }
        }
        
        public void Clear ()
        {
            foreach (var child in children) {
                Unparent (child);
            }
            children.Clear ();
        }
    }
}
