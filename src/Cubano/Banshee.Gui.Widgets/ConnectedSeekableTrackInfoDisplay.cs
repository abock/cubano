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
                
                Opacity = 0;
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
                    EnterTrackInfoChangedState ();
                    break;

                case PlayerEvent.StateChange:
                    switch (((PlayerEventStateChangeArgs)args).Current) {
                        case PlayerState.Contacting:
                        case PlayerState.Loading:
                            break;
                        case PlayerState.Idle:
                            StartIdleTimeout ();
                            break;
                    }
                    
                    break;

                case PlayerEvent.Iterate:
                    OnPlayerEngineTick ();
                    break;
            }
        }
        
        private void Show ()
        {
            AnimateDouble ("Opacity").To (1).Repeat (1).Start ();
        }
        
        private void Hide ()
        {
            AnimateDouble ("Opacity").To (0).Repeat (1).Start ();
        }
        
        private void StartIdleTimeout ()
        {
            CancelIdleTimeout ();
            idle_timeout_id = GLib.Timeout.Add (500, OnIdleTimeout);
        }
        
        private void CancelIdleTimeout ()
        {
            if (idle_timeout_id > 0) {
                GLib.Source.Remove (idle_timeout_id);
                idle_timeout_id = 0;
            }
        }
        
        private bool OnIdleTimeout ()
        {
            if (ServiceManager.PlayerEngine.CurrentTrack == null || 
                ServiceManager.PlayerEngine.CurrentState == PlayerState.Idle) {
                EnterIdleState ();
            }
            
            return false;
        }
        
        private void EnterIdleState ()
        {
            Position = 0;
            Duration = 0;
            UpdateCurrentTrack (null);
        }
        
        private void EnterTrackInfoChangedState ()
        {
            if (Opacity != 1) {
                Show ();
            }
            
            UpdateCurrentTrack (ServiceManager.PlayerEngine.CurrentTrack);
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

    }
}
