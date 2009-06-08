// 
// NowPlayingStage.cs
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

using Clutter;

using Banshee.Collection;
using Banshee.ServiceStack;
using Banshee.MediaEngine;

namespace Cubano.NowPlaying
{
    public class NowPlayingStage : Group
    {
        private ArtworkDisplay artwork_display;
        
        public Texture VideoTexture { get; private set; }
        private bool video_texture_mapped = false;
    
        public NowPlayingStage (Texture video_texture)
        {
            VideoTexture = video_texture;
            
            Add (VideoTexture);
            Add (artwork_display = new ArtworkDisplay () { IsVisible = true });
            
            ConfigureVideo ();
        }
        
        public NowPlayingStage (IntPtr raw) : base (raw)
        {
        }
                
        public void Pause ()
        {
            artwork_display.Pause ();
        }
        
        public void Resume ()
        {
            artwork_display.Resume ();
        }
        
#region Actor Overrides
        
        protected override void OnAllocate (ActorBox box, AllocationFlags flags)
        {
            base.OnAllocate (box, flags);
            
            artwork_display.Allocate (new ActorBox (0, 0, Width, Height), flags);
            
            int video_width, video_height;
            VideoTexture.GetSize (out video_width, out video_height);
            if (video_width > 0 && video_height > 0) {
                AllocateVideoTexture (video_width, video_height);
            }
        }
        
#endregion

#region Video

        private void ConfigureVideo ()
        {
            VideoTexture.SizeChange += OnVideoTextureSizeChange;
            
            ServiceManager.PlayerEngine.ConnectEvent (OnPlayerEvent,
                PlayerEvent.StartOfStream |
                PlayerEvent.EndOfStream);
            
            ToggleVideoVisibility ();
        }
        
        private void OnVideoTextureSizeChange (object o, SizeChangeArgs args)
        {
            AllocateVideoTexture (args.Width, args.Height);
        }
        
        private void AllocateVideoTexture (int textureWidth, int textureHeight)
        {
            int stage_width = (int)Width;
            int stage_height = (int)Height;
            
            int new_x, new_y, new_width, new_height;
            
            new_height = (textureHeight * stage_width) / textureWidth;
            if (new_height <= stage_height) {
                new_width = stage_width;
                new_x = 0;
                new_y = (stage_height - new_height) / 2;
            } else {
                new_width = (textureWidth * stage_height) / textureHeight;
                new_height = stage_height;
                new_x = (stage_width - new_width) / 2;
                new_y = 0;
            }
            
            VideoTexture.SetPosition (new_x, new_y);
            VideoTexture.SetSize (new_width, new_height);
        }
        
        private void OnPlayerEvent (PlayerEventArgs args)
        {
            ToggleVideoVisibility ();
        }
        
        private void ToggleVideoVisibility ()
        {
            TrackInfo track = ServiceManager.PlayerEngine.CurrentTrack;
            bool video_playing = (track != null && (track.MediaAttributes & TrackMediaAttributes.VideoStream) != 0);
            
            artwork_display.AnimationChain
                .SetEasing (video_playing ? AnimationMode.EaseInQuad : AnimationMode.EaseOutQuad)
                .SetDuration (750)
                .Animate ("opacity", video_playing ? 0 : 255);
        }
        
#endregion

    }
}
