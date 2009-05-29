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

namespace Cubano.NowPlaying
{
    public class NowPlayingStage : Group
    {
        private ArtworkDisplay artwork_display;
        private VideoDisplay video_display;
    
        public NowPlayingStage ()
        {
            Add (artwork_display = new ArtworkDisplay () { Visible = true });
            Add (video_display = new VideoDisplay () { Visible = false });
            
            video_display.SizeChange += OnVideoDisplaySizeChange;
        }
        
        public NowPlayingStage (IntPtr raw) : base (raw)
        {
        }
        
        protected override void OnAllocate (ActorBox box, bool absolute_origin_changed)
        {
            base.OnAllocate (box, absolute_origin_changed);
            
            artwork_display.Allocate (new ActorBox (0, 0, Width, Height), absolute_origin_changed);
            
            int video_width, video_height;
            video_display.GetSize (out video_width, out video_height);
            if (video_width > 0 && video_height > 0) {
                AllocateVideoDisplay (video_width, video_height);
            }
        }
        
        private void OnVideoDisplaySizeChange (object o, SizeChangeArgs args)
        {
            AllocateVideoDisplay (args.Width, args.Height);
        }
        
        private void AllocateVideoDisplay (int textureWidth, int textureHeight)
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
            
            video_display.SetPosition (new_x, new_y);
            video_display.SetSize (new_width, new_height);
        }
    }
}
