// 
// CanvasSeekSlider.cs
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

using Hyena.Gui.Canvas2;

using Banshee.MediaEngine;
using Banshee.ServiceStack;

namespace Banshee.Gui.Widgets
{
    public class CanvasSeekSlider : CanvasSlider
    {
        public CanvasSeekSlider ()
        {
            ServiceManager.PlayerEngine.ConnectEvent (OnPlayerEvent, 
                PlayerEvent.Iterate | 
                PlayerEvent.Buffering |
                PlayerEvent.StartOfStream |
                PlayerEvent.StateChange);
        }
        
        private void OnPlayerEvent (PlayerEventArgs args)
        {
            switch (args.Event) {
                case PlayerEvent.Iterate:
                    OnPlayerEngineTick ();
                    break;
                /*case PlayerEvent.StartOfStream:
                    stream_position_label.StreamState = StreamLabelState.Playing;
                    seek_slider.CanSeek = ServiceManager.PlayerEngine.CanSeek;
                    break;
                case PlayerEvent.Buffering:
                    PlayerEventBufferingArgs buffering = (PlayerEventBufferingArgs)args;
                    if (buffering.Progress >= 1.0) {
                        stream_position_label.StreamState = StreamLabelState.Playing;
                        break;
                    }
                    
                    stream_position_label.StreamState = StreamLabelState.Buffering;
                    stream_position_label.BufferingProgress = buffering.Progress;
                    seek_slider.SetIdle ();
                    break;
                case PlayerEvent.StateChange:
                    switch (((PlayerEventStateChangeArgs)args).Current) {
                        case PlayerState.Contacting:
                            transitioning = false;
                            stream_position_label.StreamState = StreamLabelState.Contacting;
                            seek_slider.SetIdle ();
                            break;
                        case PlayerState.Loading:
                            transitioning = false;
                            if (((PlayerEventStateChangeArgs)args).Previous == PlayerState.Contacting) {
                                stream_position_label.StreamState = StreamLabelState.Loading;
                                seek_slider.SetIdle ();
                            }
                            break;
                        case PlayerState.Idle:
                            seek_slider.CanSeek = false;
                            if (!transitioning) {
                                stream_position_label.StreamState = StreamLabelState.Idle;
                                seek_slider.Duration = 0;
                                seek_slider.SeekValue = 0;
                                seek_slider.SetIdle ();
                            }
                            break;
                        default:
                            transitioning = false;
                            break;
                    }
                    break;*/
            }
        }
        
        private void OnPlayerEngineTick ()
        {
            if (ServiceManager.PlayerEngine == null) {
                return;
            }
            
            double length = ServiceManager.PlayerEngine.Length;
            Value = length <= 0 ? 0 : ServiceManager.PlayerEngine.Position / length;
        }
    }
}
