// 
// CanvasBox.cs
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

namespace Hyena.Gui.Canvas2
{
    public abstract class CanvasBox : CanvasContainer
    {
        public override void SizeRequest (out double width, out double height)
        {
            int visible_child_count = 0;
            double [] request = { 0, 0 };
            double [] child_request = { 0, 0 };
            
            foreach (var child in Children) {
                if (!child.Visible) {
                    continue;
                }
                
                visible_child_count++;
                child.SizeRequest (out child_request[StaticDimension], out child_request[VariableDimension]);
                
                request[StaticDimension] = Math.Max (request[StaticDimension], child_request[StaticDimension]);
                request[VariableDimension] += child_request[VariableDimension];
            }
            
            request[VariableDimension] += (visible_child_count - 1) * Spacing;
            
            base.SizeRequest (out width, out height);
            width += request[StaticDimension];
            height += request[VariableDimension];
        }
    
        public override void Layout ()
        {
            double variable_offset = VariableOffset;
            double static_offset = StaticOffset;
            
            double static_space = 0;
            double flex_space = 0;
            int flex_count = 0;
            int visible_child_count = 0;
            
            double [] child_request = { 0, 0 };
            
            foreach (var child in Children) {
                if (!child.Visible) {
                    continue;
                }
                
                visible_child_count++;
                child.SizeRequest (out child_request[StaticDimension], out child_request[VariableDimension]);
                static_space += child.Expanded ? 0 : child_request[VariableDimension];
                flex_count += child.Expanded ? 1 : 0;
            }
            
            flex_space = VariableSize - static_space - (visible_child_count - 1) * Spacing;
            if (flex_space < 0) {
                flex_space = 0;
            }
        
            foreach (var child in Children) {
                if (!child.Visible) {
                    return;
                }
            
                SetChildVariableOffset (child, variable_offset);
                SetChildStaticOffset (child, static_offset);    

                if (child.Expanded && flex_count > 0) {
                    double size = flex_space / flex_count--;
                    flex_space -= size;
                    if (flex_count == 0) {
                        size += flex_space;
                    }
                    
                    SetChildVariableSize (child, size);
                } else {
                    child.SizeRequest (out child_request[StaticDimension], out child_request[VariableDimension]);
                    SetChildVariableSize (child, child_request[VariableDimension]);
                }
                
                SetChildStaticSize (child, StaticSize);

                variable_offset += GetChildVariableSize (child) + Spacing;

                child.Layout ();
            }
        }
        
        protected abstract int StaticDimension { get; }
        protected abstract int VariableDimension { get; }
        protected abstract double VariableOffset { get; }
        protected abstract double StaticOffset { get; } 
        protected abstract double VariableSize { get; }
        protected abstract double StaticSize { get; } 
        
        protected abstract void SetChildVariableOffset (ICanvasItem child, double offset);
        protected abstract void SetChildStaticOffset (ICanvasItem child, double offset);
        protected abstract void SetChildVariableSize (ICanvasItem child, double size);
        protected abstract void SetChildStaticSize (ICanvasItem child, double size);
        
        protected abstract double GetChildVariableOffset (ICanvasItem child);
        protected abstract double GetChildStaticOffset (ICanvasItem child);
        protected abstract double GetChildVariableSize (ICanvasItem child);
        protected abstract double GetChildStaticSize (ICanvasItem child);
        
        private double spacing;
        public double Spacing {
            get { return spacing; }
            set { spacing = value; }
        }
    }
    
    public class CanvasVBox : CanvasBox
    {
        protected override int StaticDimension   { get { return 0;           } }
        protected override double StaticSize     { get { return InnerWidth;  } }
        protected override double StaticOffset   { get { return ParentLeft + PaddingLeft; } } 
        protected override int VariableDimension { get { return 1;           } }
        protected override double VariableSize   { get { return InnerHeight; } }
        protected override double VariableOffset { get { return ParentTop + PaddingTop;  } } 

        protected override double GetChildStaticOffset   (ICanvasItem child) { return child.Left;   }
        protected override double GetChildStaticSize     (ICanvasItem child) { return child.Width;  }
        protected override double GetChildVariableOffset (ICanvasItem child) { return child.Top;    }
        protected override double GetChildVariableSize   (ICanvasItem child) { return child.Height; }

        protected override void SetChildStaticOffset   (ICanvasItem child, double offset) { child.Left = offset; }
        protected override void SetChildStaticSize     (ICanvasItem child, double size)   { child.Width = size;  }
        protected override void SetChildVariableOffset (ICanvasItem child, double offset) { child.Top = offset;  }
        protected override void SetChildVariableSize   (ICanvasItem child, double size)   { child.Height = size; } 
    }
        
    public class CanvasHBox : CanvasBox
    {
        protected override int StaticDimension   { get { return 1;           } }
        protected override double StaticSize     { get { return InnerHeight;  } }
        protected override double StaticOffset   { get { return ParentTop + PaddingTop; } } 
        protected override int VariableDimension { get { return 0;           } }
        protected override double VariableSize   { get { return InnerWidth; } }
        protected override double VariableOffset { get { return ParentLeft + PaddingLeft;  } } 

        protected override double GetChildStaticOffset   (ICanvasItem child) { return child.Top;   }
        protected override double GetChildStaticSize     (ICanvasItem child) { return child.Height;  }
        protected override double GetChildVariableOffset (ICanvasItem child) { return child.Left;    }
        protected override double GetChildVariableSize   (ICanvasItem child) { return child.Width; }

        protected override void SetChildStaticOffset   (ICanvasItem child, double offset) { child.Top = offset; }
        protected override void SetChildStaticSize     (ICanvasItem child, double size)   { child.Height = size;  }
        protected override void SetChildVariableOffset (ICanvasItem child, double offset) { child.Left = offset;  }
        protected override void SetChildVariableSize   (ICanvasItem child, double size)   { child.Width = size; } 
    }
}
