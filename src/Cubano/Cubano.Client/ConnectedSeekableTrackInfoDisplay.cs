// 
// ConnectedSeekableTrackInfoDisplay.cs
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

using Banshee.Collection;
using Banshee.MediaEngine;
using Banshee.ServiceStack;

namespace Banshee.Gui.Widgets
{
    public class ConnectedSeekableTrackInfoDisplay : SeekableTrackInfoDisplay, IDisposable
    {
        private uint idle_timeout_id = 0;
    
        public ConnectedSeekableTrackInfoDisplay ()
        {
            ServiceManager.PlayerEngine.ConnectEvent (OnPlayerEvent, 
                PlayerEvent.Iterate | 
                PlayerEvent.Buffering |
                PlayerEvent.TrackInfoUpdated |
                PlayerEvent.StartOfStream |
                PlayerEvent.StateChange);
        }
        
        public void Dispose ()
        {
            if (idle_timeout_id > 0) {
                GLib.Source.Remove (idle_timeout_id);
            }
            
            if (ServiceManager.PlayerEngine != null) {
                ServiceManager.PlayerEngine.DisconnectEvent (OnPlayerEvent);
            }
        }
        
#region State / Event / Timing Handlers
        
        private void OnPlayerEvent (PlayerEventArgs args)
        {
            switch (args.Event) {
                case PlayerEvent.StartOfStream:
                case PlayerEvent.TrackInfoUpdated:
                    LoadCurrentTrack ();
                    break;

                case PlayerEvent.StateChange:
                    if (IncomingTrack != null /* || incoming_image != null*/) {
                        PlayerEventStateChangeArgs state = (PlayerEventStateChangeArgs)args;
                        if (state.Current == PlayerState.Idle) {
                            if (idle_timeout_id > 0) {
                                GLib.Source.Remove (idle_timeout_id);
                            } else {
                                GLib.Timeout.Add (100, IdleTimeout);
                            }
                        }
                    }
                    break;

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
        
        private bool IdleTimeout ()
        {
            if (ServiceManager.PlayerEngine.CurrentTrack == null || 
                ServiceManager.PlayerEngine.CurrentState == PlayerState.Idle) {
                // incoming_track = null;
                // incoming_image = null;
            }
            
            idle_timeout_id = 0;
            return false;
        }
        
        private void OnPlayerEngineTick ()
        {
            if (ServiceManager.PlayerEngine == null) {
                return;
            }
            
            Duration = ServiceManager.PlayerEngine.Length;
            Position = ServiceManager.PlayerEngine.Position;
        }
        
#endregion

#region Data / Display
        
        private void LoadCurrentTrack ()
        {
            TrackInfo track = ServiceManager.PlayerEngine.CurrentTrack;

            if (track == CurrentTrack /* && !IsMissingImage (current_image)*/) {
                return;
            } else if (track == null) {
                IncomingTrack = null;
                // incoming_image = null;
                return;
            }

            IncomingTrack = track;
            
            // LoadImage (track);

            // if (stage.Actor == null) {
            //     stage.Reset ();
            // }
            
            OnIteration ();
        }
        
        private void OnIteration ()
        {
            /*Invalidate ();
            
            if (stage.Actor != null) {
                last_fps = stage.Actor.FramesPerSecond;
                return;
            }
            
            InvalidateCache ();
            
            if (ApplicationContext.Debugging) {
                Log.DebugFormat ("TrackInfoDisplay RenderAnimation: {0:0.00} FPS", last_fps);
            }
            
            if (current_image != null && current_image != incoming_image && !IsMissingImage (current_image)) {
                current_image.Destroy ();
            }*/
            
            // current_image = incoming_image;
            CurrentTrack = IncomingTrack;
            IncomingTrack = null;
            
            UpdateMetadataDisplay ();
            
            // OnArtworkChanged ();
        }
        
#endregion

    }
}
