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

using Hyena.Data;
using Hyena.Collections;
using Banshee.Base;
using Banshee.Sources;
using Banshee.ServiceStack;
using Banshee.Collection;
using Banshee.Collection.Gui;
using Banshee.Collection.Database;

namespace Cubano.NowPlaying
{
    internal class ActorCache : LruCache<string, Actor>
    {
        public ActorCache (int maxCount) : base (maxCount)
        {
        }
        
        public static int GetActorCountForScreen (double textureSize)
        {
            return GetActorCountForScreen (textureSize, Gdk.Screen.Default);
        }
        
        public static int GetActorCountForScreen (double textureSize, Gdk.Screen screen)
        {
            return GetActorCountForArea (textureSize, screen.Width, screen.Height);
        }
        
        public static int GetActorCountForArea (double textureSize, int areaWidth, int areaHeight)
        {
            return (int)(Math.Ceiling (areaWidth / textureSize) * Math.Ceiling (areaHeight / textureSize));
        }
    }

    public class AlbumArtActor : Texture
    {
        private static ArtworkManager artwork_manager;
    
        private AlbumInfo album;
        public AlbumInfo Album {
            get { return album; }
        }
        
        public AlbumArtActor (AlbumInfo album, double textureSize) : base ()
        {
            if (artwork_manager == null && ServiceManager.Contains<ArtworkManager> ()) {
                artwork_manager = ServiceManager.Get<ArtworkManager> ();
            }
            
            var image = artwork_manager.LookupScalePixbuf (album.ArtworkId, (int)textureSize);
            if (image != null) {
                SetFromRgbData (image.Pixels, image.HasAlpha, image.Width, image.Height,
                    image.Rowstride, image.HasAlpha ? 4 : 3, TextureFlags.None);
                Width = (uint)image.Width;
                Height = (uint)image.Height;
            }
            
            this.album = album;
        }
    }

    public class ArtworkDisplay : Actor
    {
        private static readonly double ActorSize = 100;
    
        private List<Actor> children = new List<Actor> ();
        private ActorCache actor_cache = new ActorCache (ActorCache.GetActorCountForScreen (ActorSize));

        public ArtworkDisplay () : base ()
        {
        }
        
        private IListModel<AlbumInfo> album_model;
        public IListModel<AlbumInfo> Model {
            get { return album_model; }
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
        
        public void SetSource (DatabaseSource source)
        {
            if (album_model != null) {
                album_model.Reloaded -= OnModelReloaded;
                album_model = null;
            }
            
            foreach (IListModel model in source.CurrentFilters) {
                album_model = model as IListModel<AlbumInfo>;
                if (album_model != null) {
                    break;
                }
            }
            
            if (album_model != null) {
                album_model.Reloaded += OnModelReloaded;
            }
            
            QueueRelayout ();
        }

        private void OnModelReloaded (object o, EventArgs args)
        {
            Rebuild ();
        }
        
#region IListModel<T> Interaction

        private Actor GetActorForAlbumInfo (AlbumInfo info)
        {
            Actor actor = null;
            if (actor_cache.TryGetValue (info.ArtworkId, out actor)) {
                return actor;
            }
            
            actor_cache.Add (info.ArtworkId, actor = new AlbumArtActor (info, ActorSize));
            return actor;
        }
        
        private void Rebuild ()
        {
            Clear ();
            
            int max_items = ActorCache.GetActorCountForArea (ActorSize, (int)Width, (int)Height);
            if (max_items == 0) {
                return;
            }
            
            Console.WriteLine (max_items);
            for (int i = 0, n = Model.Count; i < n && children.Count <= max_items; i++) {
                var info = Model[i];
                if (info == null || info.ArtworkId == null) {
                    continue;
                }
                
                var actor = GetActorForAlbumInfo (info);
                actor.Parent = this;
                children.Add (actor);
            }
            
            Console.WriteLine ("ACTOR COUNT = {0}", children.Count);
            Console.WriteLine ("CACHE COUNT = {0}", actor_cache.Count);
        }
        
        public void Clear ()
        {
            children.ForEach (actor => actor.Unparent ());
            children.Clear ();
        }

#endregion
        
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
            
            Rebuild ();
            
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

    }
}
