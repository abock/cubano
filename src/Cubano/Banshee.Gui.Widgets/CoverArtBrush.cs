// 
// CoverArtBrush.cs
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
using Hyena.Gui.Canvas;

using Banshee.Collection;
using Banshee.Collection.Gui;
using Banshee.ServiceStack;

namespace Banshee.Gui.Widgets
{
    public class CoverArtBrush : ImageBrush
    {
        private ImageSurface missing_audio_surface;
        private ImageSurface missing_video_surface;
        private int missing_artwork_size = 48;
    
        private string artwork_id;
        private int artwork_size;
        private TrackInfo track;
        private ArtworkManager artwork_manager;
        
        public TrackInfo Track {
            get { return track; }
        }
    
        public CoverArtBrush ()
        {
            if (ServiceManager.Contains<ArtworkManager> ()) {
                artwork_manager = ServiceManager.Get<ArtworkManager> ();
            }
        }
        
        public bool ShouldUpdate (TrackInfo track, int artworkSize)
        {
            string new_artwork_id = track == null ? null : track.ArtworkId;
            return new_artwork_id != artwork_id || artwork_size != artworkSize;
        }
        
        public bool Load (TrackInfo track, int artworkSize)
        {
            string new_artwork_id = track == null ? null : track.ArtworkId;
            if (this.track != track) {
                this.track = track;
            }
            
            if (new_artwork_id != artwork_id || artwork_size != artworkSize) {
                artwork_id = new_artwork_id;
                artwork_size = artworkSize;
                LoadImage ();
                return true;
            }
            
            return false;
        }
        
        private void LoadImage ()
        {
            DestroyCurrentSurface ();
            LoadMissingImages ();
        
            Surface = artwork_manager.LookupScaleSurface (artwork_id, artwork_size);
            if (Surface == null && track != null) {
                Surface = (track.MediaAttributes & TrackMediaAttributes.VideoStream) != 0
                    ? missing_video_surface
                    : missing_audio_surface;
            } else if (track == null) {
                Surface = null;
            }
        }
        
        private void LoadMissingImages ()
        {
            if (missing_audio_surface == null) {
                missing_audio_surface = new PixbufImageSurface (IconThemeUtils.LoadIcon (
                    missing_artwork_size, "audio-x-generic"), true);
            }
            
            if (missing_video_surface == null) {
                missing_video_surface = new PixbufImageSurface (IconThemeUtils.LoadIcon (
                    missing_artwork_size, "video-x-generic"), true);
            }
        }
        
        private void DestroyCurrentSurface ()
        {
            if (Surface != null && Surface != missing_audio_surface && Surface != missing_video_surface) {
                Surface.Destroy ();
                Surface = null;
            }
        }
        
        public void Clear ()
        {
        }
        
        public int MissingArtworkSize {
            get { return missing_artwork_size; }
            set { 
                if (missing_artwork_size != value) {
                    if (missing_audio_surface != null) {
                        missing_audio_surface.Destroy ();
                        missing_audio_surface = null;
                    }
                    
                    if (missing_video_surface != null) {
                        missing_video_surface.Destroy ();
                        missing_video_surface = null;
                    }
                    
                    missing_artwork_size = value;
                }
            }
        }
    }
}
