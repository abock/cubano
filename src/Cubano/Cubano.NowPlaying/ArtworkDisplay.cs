// 
// ArtworkDisplay.cs
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
using System.Collections.Generic;

using Clutter;

namespace Cubano.NowPlaying
{
    public class ArtworkDisplay : Actor, ContainerImplementor
    {
        private List<Actor> children = new List<Actor> ();
    
        public ArtworkDisplay () : base ()
        {
            
        }
        
        private float padding;
        public float Padding {
            get { return padding; }
            set {
                padding = value;
                QueueRelayout ();
            }
        }
        
        private float spacing;
        public float Spacing {
            get { return spacing; }
            set {
                spacing = value;
                QueueRelayout ();
            }
        }
        
        private bool use_transformed_box;
        public bool UseTransformedBox {
            get { return use_transformed_box; }
            set {
                use_transformed_box = value;
                QueueRelayout ();
            }
        }
        
#region ClutterActor overrides

        private void GetPreferredDimension (bool xdim, out float min_dim, out float natural_dim)
        {
            float min_a = 0, min_b = 0;
            float natural_a = 0, natural_b = 0;
            bool first = true;
            
            foreach (var child in children) {
                float child_dim, child_min, child_natural, ignore;
                
                if (xdim) {
                    child_dim = child.Xu;
                    child.GetPreferredSize (out child_min, out ignore, out child_natural, out ignore);
                } else {
                    child_dim = child.Yu;
                    child.GetPreferredSize (out ignore, out child_min, out ignore, out child_natural);
                }
                
                if (first) {
                    first = false;
                    min_a = child_dim;
                    natural_a = child_dim;
                    min_b = min_a + child_min;
                    natural_b = natural_a + child_natural;
                } else {
                    min_a = Math.Min (child_dim, min_a);
                    natural_a = Math.Min (child_dim, natural_a);
                    min_b = Math.Max (child_dim + child_min, min_b);
                    natural_b = Math.Max (child_dim + child_natural, natural_b);
                }
            }
                
            min_a = Math.Max (0, min_a);
            natural_a = Math.Max (0, natural_a);
            min_b = Math.Max (0, min_b);
            natural_b = Math.Max (0, natural_b);
            
            min_dim = min_b - min_a;
            natural_dim = natural_b - min_a;
        }

        protected override void OnGetPreferredWidth (float for_height, 
            out float min_width, out float natural_width)
        {
            GetPreferredDimension (true, out min_width, out natural_width);
        }
        
        protected override void OnGetPreferredHeight (float for_width, 
            out float min_height, out float natural_height)
        {
            GetPreferredDimension (false, out min_height, out natural_height);
        }
        
        protected override void OnAllocate (ActorBox box, bool absolute_origin_changed)
        {
            base.OnAllocate (box, absolute_origin_changed);
            
            float current_x = Padding;
            float current_y = Padding;
            float max_row_height = 0;
            
            foreach (var child in children) {
                float natural_width, natural_height, ignore;
                
                child.GetPreferredSize (out ignore, out ignore, out natural_width, out natural_height);
                
                // if it fits in the current row, keep it;
                // otherwise reflow into another row
                if (current_x + natural_width > box.X2 - box.X1 - Padding) {
                    current_x = Padding;
                    current_y += max_row_height + Spacing;
                    max_row_height = 0;
                }
                
                child.Allocate (new ActorBox () {
                    X1 = current_x,
                    Y1 = current_y,
                    Width = natural_width,
                    Height = natural_height
                }, absolute_origin_changed);      
                
                // if we take into account the transformation of the children
                // then we first check if it's transformed; then we get the
                // onscreen coordinates of the two points of the bounding box
                // of the actor (origin(x, y) and (origin + size)(x,y)) and
                // we update the coordinates and area given to the next child
                //
                if (UseTransformedBox && (child.IsScaled || child.IsRotated)) {
                    var v1 = new Vertex ();
                    if (absolute_origin_changed) {
                        v1.X = box.X1;
                        v1.Y = box.Y1;
                    }
                    
                    var v2 = child.ApplyTransformToPoint (v1);
                    var transformed_box = new ActorBox () {
                        X1 = v2.X,
                        Y1 = v2.Y
                    };
                    
                    v1.X = natural_width;
                    v1.Y = natural_height;
                    v2 = child.ApplyTransformToPoint (v1);
                    transformed_box.X2 = v2.X;
                    transformed_box.Y2 = v2.Y;
                    
                    natural_width = transformed_box.Width;
                    natural_height = transformed_box.Height;
                }
                               
                // Record the maximum child height on current row to know
                // what's the increment that should be used for the next row
                if (natural_height > max_row_height) {
                    max_row_height = natural_height;
                }
                
                current_x += natural_width + Spacing;
            }            
        }
        
        protected override void OnPainted ()
        {
            Cogl.General.PushMatrix ();
            
            foreach (var child in children) {
                if (child.Visible) {
                    child.Paint ();
                }
            }
            
            Cogl.General.PopMatrix ();
        }
        
#endregion
        
#region ContainerImplementer
        
        public void Add (Actor actor)
        {
            children.Add (actor);
            actor.Parent = this;
            
            GLib.Signal.Emit (this, "actor-added", actor);
            
            QueueRelayout ();
        }
        
        public void Remove (Actor actor)
        {
            if (children.Remove (actor)) {
                actor.Unparent ();
                
                GLib.Signal.Emit (this, "actor-removed", actor);
                
                QueueRelayout ();
            }
        }
        
        public void Foreach (Callback cb)
        {
            foreach (var child in children) {
                cb (child);
            }
        }
        
        public void Raise (Actor actor, Actor sibling)
        {
        }
        
        public void Lower (Actor actor, Actor sibling)
        {
        }
        
        public void SortDepthOrder ()
        {
        }
        
        public void CreateChildMeta (Actor actor)
        {
        }
        
        public void DestroyChildMeta (Actor actor)
        {
        }
        
        public ChildMeta GetChildMeta (Actor actor)
        {
            return null;
        }
        
#endregion

    }
}
