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

    internal class AlbumArtActor : Texture
    {
        private static ArtworkManager artwork_manager;
    
        private AlbumInfo album;
        public AlbumInfo Album {
            get { return album; }
        }
        
        public AlbumArtActor (IntPtr raw) : base (raw)
        {
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
            }
            
            this.album = album;
        }
    }

    public class ArtworkDisplay : Actor
    {
        private static readonly double ActorSize = 100;

        private ActorCache actor_cache = new ActorCache (ActorCache.GetActorCountForScreen (ActorSize));
        private Random random = new Random ();
        private int allocation_seed;
        
        private Clone [,] grid = null;
        private float last_alloc_width, last_alloc_height;
        private int visible_grid_width, visible_grid_height;
        private int visible_actor_count;
        
        private Timeline cell_select_timeline;
        private bool paused = true;

        public ArtworkDisplay () : base ()
        {
            SetSource (ServiceManager.SourceManager.MusicLibrary);
            
            cell_select_timeline = new Timeline () {
                Speed = 3,
                Duration = 1,
                Loop = true
            };
            
            cell_select_timeline.Completed += OnCellSelectTimelineCompleted;
            
            allocation_seed = random.Next ();
        }
        
        public ArtworkDisplay (IntPtr raw) : base (raw)
        {
        }
        
        private void BuildCloneGrid ()
        {
            if (grid != null) {
                return;
            }
            
            float pixel_x = 0;
            float pixel_y = 0;
            float size = (float)ActorSize;
        
            int grid_width = (int)(Math.Ceiling (Gdk.Screen.Default.Width / size));
            int grid_height = (int)(Math.Ceiling (Gdk.Screen.Default.Height / size));
            int x = 0, y = 0;
            
            grid = new Clone[grid_width, grid_height];
            
            // Populate a collection with enough clones to build
            // a grid large enough to cover the entire screen
            for (int i = 0; i < actor_cache.MaxCount; i++) {
                var slot = new Clone (null) {
                    Parent = this,
                    Visible = true
                };
                
                slot.Allocate (new ActorBox (pixel_x, pixel_y, size, size), false);
                
                grid[x, y] = slot;
                
                pixel_x += size;
                if (x++ == grid_width - 1) {
                    x = 0;
                    y++;
                    pixel_x = 0;
                    pixel_y += size;
                }
            }
        }

        protected override void OnAllocate (ActorBox box, bool absolute_origin_changed)
        {
            base.OnAllocate (box, absolute_origin_changed);
            
            float width = Width;
            float height = Height;
            
            if (width <= 0 || height <= 0 || 
                width == last_alloc_width || height == last_alloc_height || 
                Model == null) {
                return;
            }
            
            // Calculate the number of actors that will fit in our window
            visible_actor_count = ActorCache.GetActorCountForArea (ActorSize, (int)Width, (int)Height);
            if (visible_actor_count <= 0) {
                return;
            }
            
            BuildCloneGrid ();
            
            last_alloc_width = width;
            last_alloc_height = height;
            
            var rand = new Random (allocation_seed);
            float size = (float)ActorSize;
            
            visible_grid_width = (int)(Math.Ceiling (width / size));
            visible_grid_height = (int)(Math.Ceiling (height / size));

            for (int i = 0; i < visible_actor_count; i++) {
                var actor = GetRandomActor (rand);
                // Assign the artwork actor to a clone slot in the screen grid
                grid[i % visible_grid_width, i / visible_grid_width].Source = actor;
            }
            
            Resume ();
        }
        
        protected override void OnPainted ()
        {
            if (grid == null || Width <= 0 || Height <= 0) {
                return;
            }
            
            Cogl.General.PushMatrix ();
            
            foreach (var child in waiting_for_slot) {
                child.Paint ();
            }
            
            foreach (var child in grid) {
                child.Paint ();
            }
            
            Cogl.General.PopMatrix ();
        }
        
#region Animation

        private List<Actor> waiting_for_slot = new List<Actor> ();

        private void OnCellSelectTimelineCompleted (object o, EventArgs args)
        {
            if (visible_actor_count <= 0 || visible_grid_width <= 0) {
                return;
            }
        
            int target_x = 0, target_y = 0;
            Clone cell_target = null;
            
            while (cell_target == null) {
                int index = random.Next (0, visible_actor_count);
                target_x = index % visible_grid_width;
                target_y = index / visible_grid_width;
                cell_target = grid[target_x, target_y];
                if (waiting_for_slot.Contains (cell_target)) {
                    cell_target = null;
                }
            }
            
            var cell_source = new Clone (GetRandomActor ()) { Visible = true, Parent = this };
            cell_source.Allocate (cell_target.Allocation, false);
            
            waiting_for_slot.Insert (0, cell_source);

            cell_target.AnimationChain
                .SetEasing (AnimationMode.EaseInQuad)
                .SetDuration (1000)
                .WhenFinished ((a) => {
                    grid[target_x, target_y] = cell_source;
                    waiting_for_slot.Remove (cell_source);
                    QueueRedraw ();
                 })
                .Animate ("opacity", 0);
        }

        public void Pause ()
        {
            paused = true;
            cell_select_timeline.Pause ();
            
            foreach (var child in waiting_for_slot) {
                if (child.AnimationChain.LastAnimation != null) {
                    child.AnimationChain.LastAnimation.Timeline.Stop ();
                }
            }
            
            waiting_for_slot.Clear ();
        }
        
        public void Resume ()
        {
            paused = false;
            InnerResume ();
        }
        
        private void InnerResume ()
        {
            if (!cell_select_timeline.IsPlaying && !paused) {
                cell_select_timeline.Start ();
            }
        }

#endregion
        
#region Data Model

        private IListModel<AlbumInfo> album_model;
        public IListModel<AlbumInfo> Model {
            get { return album_model; }
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
            QueueRelayout ();
        }
        
        private Actor GetRandomActor ()
        {
            return GetRandomActor (null);
        }
        
        private Actor GetRandomActor (Random rand)
        {
            while (true) {
                // FIXME: do a smarter job at picking a random piece of art
                // so that duplicate artwork is only displayed when there isn't
                // enough to actually fill the entire grid
                var actor = GetActorAtIndex ((rand ?? random).Next (0, Model.Count - 1));
                if (actor != null) {
                    if (actor.Allocation.Width == 0 || actor.Allocation.Height == 0) {
                        actor.Allocate (new ActorBox (0, 0, 
                            (float)ActorSize, (float)ActorSize), false);
                    }
                    return actor;
                }
            }
        }
                
        private Actor GetActorAtIndex (int index)
        {
            if (index < 0 || index > Model.Count) {
                return null;
            }
            
            return GetActorForAlbumInfo (Model[index]);
        }
        
        private Actor GetActorForAlbumInfo (AlbumInfo info)
        {
            Actor actor = null;
            
            if (info == null || info.ArtworkId == null) {
                return null;
            }

            if (actor_cache.TryGetValue (info.ArtworkId, out actor)) {
                return actor;
            }
            
            if (!CoverArtSpec.CoverExists (info.ArtworkId)) {
                return null;
            }
            
            actor = new AlbumArtActor (info, ActorSize) { Parent = this };
            actor_cache.Add (info.ArtworkId, actor);
            return actor;
        }

#endregion

    }
}
